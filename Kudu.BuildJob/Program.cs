using Kudu.Contracts.Infrastructure;
using Kudu.Contracts.Settings;
using Kudu.Contracts.Tracing;
using Kudu.Core;
using Kudu.Core.Deployment;
using Kudu.Core.Deployment.Generator;
using Kudu.Core.Helpers;
using Kudu.Core.Hooks;
using Kudu.Core.Infrastructure;
using Kudu.Core.Settings;
using Kudu.Core.SourceControl;
using Kudu.Core.SourceControl.Git;
using Kudu.Core.Tracing;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using XmlSettings;

namespace Kudu.BuildJob
{
    class Program
    {
        private static IEnvironment env;
        private static IDeploymentSettingsManager settingsManager;
        public static int Main(string[] args)
        {
            if (args.Length != 0)
            {
                System.Console.WriteLine(string.Join(" ", args));
            }

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            // Turn flag on in app.config to wait for debugger on launch

            if (args.Length < 2)
            {
                System.Console.WriteLine("Usage: kudu.buildjob.exe siteRoot gitRepositoryUri");
                return 1;
            }

            // The post receive hook launches the exe from sh and intereprets newline differently.
            // This fixes very wacky issues with how the output shows up in the console on push
            System.Console.Error.NewLine = "\n";
            System.Console.Out.NewLine = "\n";

            var appRoot = args[0];
            string gitRepositoryUri = args[1];
            string deployer = args.Length == 2 ? null : args[2];
            string requestId = System.Environment.GetEnvironmentVariable(Constants.RequestIdHeader);

            env = GetEnvironment(appRoot, requestId);
            ISettings settings = new XmlSettings.Settings(GetSettingsPath(env));
            settingsManager = new DeploymentSettingsManager(settings);

            // Setup the trace
            TraceLevel level = settingsManager.GetTraceLevel();
            ITracer tracer = GetTracer(env, level);
            ITraceFactory traceFactory = new TracerFactory(() => tracer);

            var logger = new ConsoleLogger();

            var gitRepository = new GitExeRepository(env, settingsManager, traceFactory)
            {
                SkipPostReceiveHookCheck= true
            };

            gitRepository.Initialize();
            gitRepository.FetchWithoutConflict(gitRepositoryUri, "master");

            // Calculate the lock path
            string lockPath = Path.Combine(env.SiteRootPath, Constants.LockPath);
            string deploymentLockPath = Path.Combine(lockPath, Constants.DeploymentLockFile);

            IOperationLock deploymentLock = DeploymentLockFile.GetInstance(deploymentLockPath, traceFactory);

            //start to build
            string statusLockPath = Path.Combine(appRoot, Constants.StatusLockFile);
            string hooksLockPath = Path.Combine(lockPath, Constants.HooksLockFile);

            IOperationLock statusLock = new LockFile(statusLockPath, traceFactory);

            Console.WriteLine($"lockPath: ${lockPath}, deploymentLockPath: ${deploymentLockPath}, statusLockPath: ${statusLockPath}, hooksLockPath: ${hooksLockPath}");

            IBuildPropertyProvider buildPropertyProvider = new BuildPropertyProvider();

            ISiteBuilderFactory builderFactory = new SiteBuilderFactory(buildPropertyProvider, env, null);
            IOperationLock hooksLock = new LockFile(hooksLockPath, traceFactory);

            env.CurrId = gitRepository.GetChangeSet(settingsManager.GetBranch()).Id;

            Console.WriteLine($"env.CurrId: ${env.CurrId}");

            IServerConfiguration serverConfiguration = new ServerConfiguration();

            IAnalytics analytics = new Analytics(settingsManager, serverConfiguration, traceFactory);

            IWebHooksManager hooksManager = new WebHooksManager(tracer, env, hooksLock);

            IDeploymentStatusManager deploymentStatusManager = new DeploymentStatusManager(env, analytics, statusLock);

            IDeploymentManager deploymentManager = new DeploymentManager(builderFactory,
                                                          env,
                                                          traceFactory,
                                                          analytics,
                                                          settingsManager,
                                                          deploymentStatusManager,
                                                          deploymentLock,
                                                          GetLogger(env, level, logger),
                                                          hooksManager,
                                                          null); // K8 todo

            var step = tracer.Step(XmlTracer.ExecutingExternalProcessTrace, new Dictionary<string, string>
            {
                { "type", "process" },
                { "path", "kudubuildjob.exe" },
                { "arguments", appRoot + " " + gitRepositoryUri }
            });

            using (step)
            {
                try
                {

                    // although the api is called DeployAsync, most expensive works are done synchronously.
                    // need to launch separate task to go async explicitly (consistent with FetchDeploymentManager)
                    var deploymentTask = Task.Run(async () => await deploymentManager.DeployAsync(gitRepository, changeSet: null, deployer: deployer, clean: false));

#pragma warning disable 4014
                    // Track pending task
                    PostDeploymentHelper.TrackPendingOperation(deploymentTask, TimeSpan.Zero);
#pragma warning restore 4014

                    deploymentTask.Wait();

                    if (PostDeploymentHelper.IsAutoSwapEnabled())
                    {
                        Console.WriteLine($"PostDeploymentHelper.IsAutoSwapEnabled: ${PostDeploymentHelper.IsAutoSwapEnabled()}");
                        string branch = settingsManager.GetBranch();
                        ChangeSet changeSet = gitRepository.GetChangeSet(branch);
                        IDeploymentStatusFile statusFile = deploymentStatusManager.Open(changeSet.Id, env);
                        if (statusFile != null && statusFile.Status == DeployStatus.Success)
                        {
                            PostDeploymentHelper.PerformAutoSwap(env.RequestId,
                                    new PostDeploymentTraceListener(tracer, deploymentManager.GetLogger(changeSet.Id)))
                                .Wait();
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.InnerException);
                    tracer.TraceError(e);
                    System.Console.Error.WriteLine($"Deployment Error {e.GetBaseException().Message}");
                    
                    return 1;
                }
                finally
                {
                    System.Console.WriteLine("Deployment Logs : '" +
                    env.AppBaseUrlPrefix + "/newui/jsonviewer?view_url=/api/deployments/" +
                    gitRepository.GetChangeSet(settingsManager.GetBranch()).Id + "/log'");
                }
            }

            if (logger.HasErrors)
            {
                return 1;
            }
            tracer.Step("Perform deploy exiting successfully");


            return 0;
        }

        private static string PackageArtifactFromFolder(IEnvironment environment, IDeploymentSettingsManager settings, ITracer tracer
            , ILogger logger, string artifactFilename)
        {
            tracer.Trace("Writing the artifacts to a squashfs file");
            string file = Path.Combine(environment.DeploymentsPath, artifactFilename);
            ExternalCommandFactory commandFactory = new ExternalCommandFactory(environment, settings, env.RepositoryPath);
            Executable exe = commandFactory.BuildExternalCommandExecutable(environment.RepositoryPath, environment.DeploymentsPath, logger);
            try
            {
                exe.ExecuteWithProgressWriter(logger, tracer, $"mksquashfs . {file} -noappend");
            }
            catch (Exception)
            {
                logger.LogError();
                throw;
            }

            return file;
        }

        private static ILogger GetLogger(IEnvironment env, TraceLevel level, ILogger primary)
        {
            if (level > TraceLevel.Off)
            {
                string logFile = System.Environment.GetEnvironmentVariable(Constants.TraceFileEnvKey);
                if (!String.IsNullOrEmpty(logFile))
                {
                    string logPath = Path.Combine(env.RootPath, Constants.DeploymentTracePath, logFile);
                    //return new CascadeLogger(primary, new TextLogger(logPath));
                    return new CascadeLogger(primary, new TextLogger(logPath));
                }
            }

            return primary;
        }

        private static ITracer GetTracer(IEnvironment env, TraceLevel level)
        {
            if (level > TraceLevel.Off)
            {
                var tracer = new XmlTracer(env.TracePath, level);
                string logFile = System.Environment.GetEnvironmentVariable(Constants.TraceFileEnvKey);
                if (!String.IsNullOrEmpty(logFile))
                {
                    // Kudu.exe is executed as part of git.exe (post-receive), giving its initial depth of 4 indentations
                    string logPath = Path.Combine(env.TracePath, logFile);
                    // since git push is "POST", which then run kudu.exe
                    return new CascadeTracer(tracer, new TextTracer(logPath, level, 4), new ETWTracer(env.RequestId, requestMethod: HttpMethod.Post.Method));
                }

                return tracer;
            }

            return NullTracer.Instance;
        }

        private static string GetSettingsPath(IEnvironment environment)
        {
            return Path.Combine(environment.DeploymentsPath, Constants.DeploySettingsPath);
        }

        private static IEnvironment GetEnvironment(string siteRoot, string requestId)
        {
            string root = Path.GetFullPath(Path.Combine(siteRoot, ".."));
            string appName = root.Replace("/home/apps/", "");

            // CORE TODO : test by setting SCM_REPOSITORY_PATH 
            // REVIEW: this looks wrong because it ignores SCM_REPOSITORY_PATH
            string repositoryPath = Path.Combine(siteRoot, Constants.RepositoryPath);

            // SCM_BIN_PATH is introduced in Kudu apache config file 
            // Provide a way to override Kudu bin path, to resolve issue where we can not find the right Kudu bin path when running on mono
            // CORE TODO I don't think this is needed anymore? This env var is not used anywhere but here.
            string binPath = System.Environment.GetEnvironmentVariable("SCM_BIN_PATH");
            if (string.IsNullOrWhiteSpace(binPath))
            {
                // CORE TODO Double check. Process.GetCurrentProcess() always gets the dotnet.exe process,
                // so changed to Assembly.GetEntryAssembly().Location
                binPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }

            System.Console.WriteLine($"binPath ${binPath}");
           
            // CORE TODO Handing in a null IHttpContextAccessor (and KuduConsoleFullPath) again
            var env = new Kudu.Core.Environment(root,
                EnvironmentHelper.NormalizeBinPath(binPath),
                repositoryPath,
                requestId,
                Path.Combine(AppContext.BaseDirectory, "KuduConsole", "kudu.dll"),
                null,
                appName);
            System.Console.WriteLine($"scriptPath ${env.ScriptPath}");
            return env;
        }
    }
}
