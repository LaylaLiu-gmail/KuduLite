using Microsoft.AspNetCore.Mvc;
using Kudu.Core.K8SE;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Kudu.Services.Diagnostics
{
    public class LogController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetHttpLog([FromQuery] int instance)
        {
            var appNamespace = K8SEDeploymentHelper.GetAppNamespace(HttpContext);
            var appName = K8SEDeploymentHelper.GetAppName(HttpContext);
            var extensionName = K8SEDeploymentHelper.GetExtensionName(HttpContext);
            var logProcessorName = extensionName + "-log-processor"; // log processor pod name
            using var k8seClient = new K8SEClient();

            //appNamespace = "appservice-ns";
            //appName = "zuh3-site";

            var pods = k8seClient.GetPodsForDeamonSet(appNamespace, logProcessorName);
            if (pods == null || pods.Count == 0)
            {
                return BadRequest($"No pod found for DeamonSet '{logProcessorName}'");
            }

            if (instance >= pods.Count || instance < 0)
            {
                return BadRequest($"Instance index error, valid values are [0, {pods.Count}]");
            }

            //var logFiles = k8seClient.ListPodFileAsync(appNamespace, pods[instance].Name, "/var/log/containers/").Result;
            var logFile = "";
            foreach (var pod in pods)
            {
                List<string> logFiles = k8seClient.ListPodFileAsync(appNamespace, pod.Name, "/var/log/containers/").Result.Split('\n').ToList();
                foreach (var file in logFiles)
                {
                    Console.WriteLine($"Found file: {file} in pod ${pod.Name}");
                    if (file.Contains("envoy"))
                    {
                        logFile = file.Substring(7, file.Length - 11);
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(logFile))
                {
                    break;
                }
            }

            var result = "";
            if (string.IsNullOrEmpty(logFile))
            {
                result = "No log file found";
            }
            else
            {
                result = await k8seClient.GetPodFileAsync(appNamespace, pods[instance].Name, "/var/log/containers/" + logFile);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetSiteLog([FromQuery] int instance)
        {
            var appNamespace = K8SEDeploymentHelper.GetAppNamespace(HttpContext);
            var appName = K8SEDeploymentHelper.GetAppName(HttpContext);
            var extensionName = K8SEDeploymentHelper.GetExtensionName(HttpContext);
            var logProcessorName = extensionName + "-log-processor"; // log processor pod name
            using var k8seClient = new K8SEClient();

            //appNamespace = "appservice-ns";
            //appName = "zuh3-site";

            var pods = k8seClient.GetPodsForDeamonSet(appNamespace, logProcessorName);
            if (pods == null || pods.Count == 0)
            {
                return BadRequest($"No pod found for DeamonSet '{logProcessorName}'");
            }

            if (instance >= pods.Count || instance < 0)
            {
                return BadRequest($"Instance index error, valid values are [0, {pods.Count}]");
            }

            //var logFiles = k8seClient.ListPodFileAsync(appNamespace, pods[instance].Name, "/var/log/containers/").Result;
            var logFile = "";
            foreach (var pod in pods)
            {
                List<string> logFiles = k8seClient.ListPodFileAsync(appNamespace, pod.Name, "/var/log/containers/").Result.Split('\n').ToList();
                foreach (var file in logFiles)
                {
                    Console.WriteLine($"Found file: {file} in pod ${pod.Name}");
                    if (file.Contains(appName))
                    {
                        Console.WriteLine($"Found file: {file}");
                        if (file.Contains("http"))
                        {
                            logFile = file.Substring(7, file.Length - 11);
                            break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(logFile))
                {
                    break;
                }
            }

            var result = "";
            if (string.IsNullOrEmpty(logFile))
            {
                result = "No log file found";
            }
            else
            {
                result = await k8seClient.GetPodFileAsync(appNamespace, pods[instance].Name, "/var/log/containers/" + logFile);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAppInitLog([FromQuery] int instance)
        {
            var appNamespace = K8SEDeploymentHelper.GetAppNamespace(HttpContext);
            var appName = K8SEDeploymentHelper.GetAppName(HttpContext);
            var extensionName = K8SEDeploymentHelper.GetExtensionName(HttpContext);
            var logProcessorName = extensionName + "-log-processor"; // log processor pod name
            using var k8seClient = new K8SEClient();
            /*
            appNamespace = "appservice-ns";
            logProcessorName = "zuharc-eastus-appservice-extension-k8se-log-processor";
            appName = "zuh3-site";
            */
            var pods = k8seClient.GetPodsForDeamonSet(appNamespace, logProcessorName);
            if (pods == null || pods.Count == 0)
            {
                return BadRequest($"No pod found for DeamonSet '{logProcessorName}'");
            }

            if (instance >= pods.Count || instance < 0)
            {
                return BadRequest($"Instance index error, valid values are [0, {pods.Count}]");
            }

            //var logFiles = k8seClient.ListPodFileAsync(appNamespace, pods[instance].Name, "/var/log/containers/").Result;
            var logFile = "";
            foreach (var pod in pods)
            {
                List<string> logFiles = k8seClient.ListPodFileAsync(appNamespace, pod.Name, "/var/log/containers/").Result.Split('\n').ToList();
                foreach (var file in logFiles)
                {
                    Console.WriteLine($"Found file: {file} in pod ${pod.Name}");
                    if (file.Contains("app-init"))
                    {
                        if (file.Contains(appName))
                        {
                            logFile = file.Substring(7, file.Length - 11);
                            break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(logFile))
                {
                    break;
                }
            }
            
            var result = "";
            if (string.IsNullOrEmpty(logFile))
            {
                result = "No log file found";
            }
            else
            {
                result = await k8seClient.GetPodFileAsync(appNamespace, pods[instance].Name, "/var/log/containers/" + logFile);
            }
            return Ok(result);
        }
    }
}
