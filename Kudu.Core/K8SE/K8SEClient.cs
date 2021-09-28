﻿using k8s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Kudu.Core.K8SE
{
    public class K8SEClient : IDisposable
    {
        private readonly IKubernetes kubernetesClient;

        public K8SEClient()
        {
            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            kubernetesClient = new Kubernetes(config);
        }

        public List<PodInstance> GetPodsForDeployment(string namespaceName, string deploymentName)
        {
            var deploy = kubernetesClient.ReadNamespacedDeployment(deploymentName, namespaceName);
            var labelSelector = string.Empty;
            foreach (var item in deploy.Spec.Selector.MatchLabels)
            {
                labelSelector = labelSelector + item.Key + "=" + item.Value + ",";
            }

            labelSelector = labelSelector.Substring(0, labelSelector.Length - 1);
            var pods = kubernetesClient.ListNamespacedPod(namespaceName, labelSelector: labelSelector);

            return pods.Items.Select(pod => new PodInstance()
            {
                Name = pod.Metadata.Name,
                NodeName = pod.Spec.NodeName,
                IpAddress = pod.Status.PodIP,
                HostIpAddress = pod.Status.HostIP,
                StartTime = pod.Status.StartTime.ToString()
            }).ToList();
        }

        public List<PodInstance> GetPodsForDeamonSet(string namespaceName, string deamonSetName)
        {
            Console.WriteLine($"Read Deamonset {deamonSetName} in namespace {namespaceName}");
            var deamonSet = kubernetesClient.ReadNamespacedDaemonSet(deamonSetName, namespaceName);
            var labelSelector = string.Empty;
            foreach (var item in deamonSet.Spec.Selector.MatchLabels)
            {
                labelSelector = labelSelector + item.Key + "=" + item.Value + ",";
            }

            labelSelector = labelSelector.Substring(0, labelSelector.Length - 1);
            var pods = kubernetesClient.ListNamespacedPod(namespaceName, labelSelector: labelSelector);
            Console.WriteLine($"List pods in namespace {namespaceName} with label selector {labelSelector}");

            return pods.Items.Select(pod => new PodInstance()
            {
                Name = pod.Metadata.Name,
                NodeName = pod.Spec.NodeName,
                IpAddress = pod.Status.PodIP,
                HostIpAddress = pod.Status.HostIP,
                StartTime = pod.Status.StartTime.ToString()
            }).ToList();
        }

        public async Task<string> GetPodAllProcessAsync(string namespaceName, string podName)
        {
            // For command with params, it should split into command list
            var command = new List<string>()
            {
                "ps",
                "-aux"
            };

            return await ExecuteCommandInPodAsync(namespaceName, podName, command);
        }

        public async Task<string> GetPodFileAsync(string namespaceName, string podName, string fileName)
        {
            Console.WriteLine($"Start reading log file {fileName} in pod {podName} within namespace {namespaceName}");
            var command = new List<string>()
            {
                "cat",
                fileName
            };

            return await ExecuteCommandInPodAsync(namespaceName, podName, command);
        }

        public async Task<string> ListPodFileAsync(string namespaceName, string podName, string filePath)
        {
            var command = new List<string>()
            {
                "ls",
                filePath
            };

            return await ExecuteCommandInPodAsync(namespaceName, podName, command);
        }

        private async Task<string> ExecuteCommandInPodAsync(string namespaceName, string podName, IEnumerable<string> command, string containerName = null)
        {
            var webSocket = await kubernetesClient.WebSocketNamespacedPodExecAsync(podName, namespaceName, command, containerName);

            var demux = new StreamDemuxer(webSocket);
            demux.Start();

            var stream = demux.GetStream(1, 1);
            var streamReader = new StreamReader(stream);
            return await streamReader.ReadToEndAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                kubernetesClient.Dispose();
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~K8SEClient()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
