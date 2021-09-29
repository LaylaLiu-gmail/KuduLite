using Kudu.Core.K8SE;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Kudu.Services.Diagnostics
{
    public class InstanceController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetAllInstances()
        {
            var appNamespace = K8SEDeploymentHelper.GetAppNamespace(HttpContext);
            var appName = K8SEDeploymentHelper.GetAppName(HttpContext);
            using var k8seClient = new K8SEClient();

            // appName = "test2";

            var pods = k8seClient.GetPodsForDeployment(appNamespace, appName);
            if (pods == null || pods.Count == 0)
            {
                return NotFound($"No instance found for the app '{appName}'");
            }

            return Ok(pods);
        }

        [HttpPut]
        public async Task<IActionResult> RestartInstance(string instanceName)
        {
            var appNamespace = K8SEDeploymentHelper.GetAppNamespace(HttpContext);
            using var k8seClient = new K8SEClient();

            var processes = await k8seClient.GetPodAllProcessAsync(appNamespace, instanceName);
            foreach (var proc in processes)
            {
                await k8seClient.KillPodProcessAsync(appNamespace, instanceName, proc.PID);
            }

            return Ok();
        }
    }
}
