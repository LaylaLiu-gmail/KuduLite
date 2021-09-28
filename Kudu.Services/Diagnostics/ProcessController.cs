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

            // appNamespace = "appservice-ns";

            var result = await k8seClient.GetPodAllProcessAsync(appNamespace, instanceName);
            return Ok(ParseProcessInfo(result));
        }

        [HttpDelete]
        public async Task<IActionResult> KillProcess([FromQuery] string instanceName, string id)
        {
            var appNamespace = K8SEDeploymentHelper.GetAppNamespace(HttpContext);
            using var k8seClient = new K8SEClient();

            // appNamespace = "appservice-ns";

            await k8seClient.KillPodProcessAsync(appNamespace, instanceName, id);
            return Ok();
        }

        private List<ProcessInfo> ParseProcessInfo(string content)
        {
            var processes = new List<ProcessInfo>();
            var splitLines = content.Split(Environment.NewLine);
            for (int index = 0; index < splitLines.Length; ++index)
            {
                if (index == 0 || string.IsNullOrWhiteSpace(splitLines[index]))
                {
                    continue;
                }

                // Default will split by whitespace characters
                // https://docs.microsoft.com/en-us/dotnet/api/system.string.split?view=net-5.0#remarks
                var items = splitLines[index].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (items.Length < 11)
                {
                    throw new Exception("Parse process info failed: column length should be greater than 11");
                }

                var command = new string[items.Length - 10];
                Array.Copy(items, 10, command, 0, command.Length);
                processes.Add(new ProcessInfo
                {
                    User = items[0],
                    PID = items[1],
                    CPU = items[2],
                    Memory = items[3],
                    VSZ = items[4],
                    RSS = items[5],
                    TTY = items[6],
                    STAT = items[7],
                    Start = items[8],
                    Time = items[9],
                    Command = string.Join(" ", command)
                });
            }

            return processes;
        }
    }
}
