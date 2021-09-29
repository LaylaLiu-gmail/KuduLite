using Microsoft.AspNetCore.Mvc;
using Kudu.Core.K8SE;
using System.Threading.Tasks;
using Kudu.Services.Models;
using System.Collections.Generic;
using System;

namespace Kudu.Services.Diagnostics
{
    public class ProcessController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetAllProcesses([FromQuery] string instanceName)
        {
            var appNamespace = K8SEDeploymentHelper.GetAppNamespace(HttpContext);
            using var k8seClient = new K8SEClient();

            var result = await k8seClient.GetPodAllProcessAsync(appNamespace, instanceName);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> KillProcess([FromQuery] string instanceName, string id)
        {
            var appNamespace = K8SEDeploymentHelper.GetAppNamespace(HttpContext);
            using var k8seClient = new K8SEClient();

            await k8seClient.KillPodProcessAsync(appNamespace, instanceName, id);
            return Ok();
        }
    }
}
