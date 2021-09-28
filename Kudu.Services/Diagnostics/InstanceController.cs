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

            // appNamespace = "appservice-ns";
            // appName = "test2";

            var pods = k8seClient.GetPodsForDeployment(appNamespace, appName);
            if (pods == null || pods.Count == 0)
            {
                return NotFound($"No instance found for the app '{appName}'");
            }

            return Ok(pods);
        }
    }
}
