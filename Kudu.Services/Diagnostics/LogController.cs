using Microsoft.AspNetCore.Mvc;
using Kudu.Core.K8SE;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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
            List<string> logFiles = k8seClient.ListPodFileAsync(appNamespace, pods[instance].Name, "/var/log/containers/").Result.Split('\n').ToList();
            var httpLogFile = "";
            
            foreach (var file in logFiles)
            {
                if (file.Contains("envoy"))
                {
                    httpLogFile = file.Substring(7, file.Length - 11);
                    break;
                }
            }
            
            var result = await k8seClient.GetPodFileAsync(appNamespace, pods[instance].Name, "/var/log/containers/" + httpLogFile);
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
            List<string> logFiles = k8seClient.ListPodFileAsync(appNamespace, pods[instance].Name, "/var/log/containers/").Result.Split('\n').ToList();
            var logFile = "";

            foreach (var file in logFiles)
            {
                if (file.Contains(appName))
                {
                    if (file.Contains("http"))
                    {
                        logFile = file.Substring(7, file.Length - 11);
                        break;
                    } 
                }
            }

            var result = await k8seClient.GetPodFileAsync(appNamespace, pods[instance].Name, "/var/log/containers/" + logFile);
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
            List<string> logFiles = k8seClient.ListPodFileAsync(appNamespace, pods[instance].Name, "/var/log/containers/").Result.Split('\n').ToList();
            var logFile = "";

            foreach (var file in logFiles)
            {
                if (file.Contains("app-init"))
                {
                    if (file.Contains(appName))
                    {
                        logFile = file.Substring(7, file.Length - 11);
                        break;
                    }

                }
            }

            var result = await k8seClient.GetPodFileAsync(appNamespace, pods[instance].Name, "/var/log/containers/" + logFile);
            return Ok(result);
        }
    }
}
