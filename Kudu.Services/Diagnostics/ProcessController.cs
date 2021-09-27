using Microsoft.AspNetCore.Mvc;
using k8s;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;
using Kudu.Core.K8SE;
using Microsoft.AspNetCore.Http;

namespace Kudu.Services.Diagnostics
{
    public class ProcessController : Controller
    {
        [HttpGet]
        public IActionResult GetAllProcesses()
        {
            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            IKubernetes client = new Kubernetes(config);

            var appNamespace = K8SEDeploymentHelper.GetAppNamespace(HttpContext);
            var appName = K8SEDeploymentHelper.GetAppName(HttpContext);

            Console.WriteLine("===" + appNamespace + "===");
            Console.WriteLine("===" + appName + "===");

            if( appNamespace == "") {
                appNamespace = "appservice-ns";
            }

            if (appName == "") {
                appName = "test2-appservice-ext-k8se-envoy-668b876986-6j4q2";
            }

            var cmd = "ls";
            var cmdQuery = HttpContext.Request.Query["cmd"];
            if (cmdQuery.Count != 0) {
                cmd = cmdQuery[0];
            }
            string a = ExecInPod(client, appNamespace, appName, cmd).Result;

            return new JsonResult(a);
        }

        private async static Task<string> ExecInPod(IKubernetes client, string namespaceName, string podName, string command, string containerName = null)
        {
            var webSocket = await client.WebSocketNamespacedPodExecAsync(podName, namespaceName, command, containerName).ConfigureAwait(false);

            var demux = new StreamDemuxer(webSocket);
            demux.Start();

            var buff = new byte[4096];
            var stream = demux.GetStream(1, 1);
            var read = stream.Read(buff, 0, 4096);

            byte[] result = new byte[read];
            Array.Copy(buff, 0, result, 0, read);

            var str = System.Text.Encoding.Default.GetString(result);
            str = removeAnsi(str);
            return str;
        }

        private static string removeAnsi(string s) {
            string pattern = @"[\u001b\u009b][[()#;?]*(?:[0-9]{1,4}(?:;[0-9]{0,4})*)?[0-9A-ORZcf-nqry=><]";
            string replacement = "";
            Regex rgx = new Regex(pattern);
            string result = rgx.Replace(s, replacement);
            return result;
        }
    }
}