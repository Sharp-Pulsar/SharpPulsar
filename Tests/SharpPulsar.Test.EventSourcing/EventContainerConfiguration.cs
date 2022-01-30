using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using SharpPulsar.TestContainer;

namespace SharpPulsar.Test.EventSourcing
{
    public sealed class EventContainerConfiguration : PulsarTestcontainerConfiguration
    {
        public EventContainerConfiguration(string image, int port):base(image, port)
        {
        }

        public override Func<IRunningDockerContainer, CancellationToken, Task> StartupCallback => (container, ct) =>
        {
            var startupScript = new StringBuilder();
            startupScript.AppendLine("#!/bin/sh");
            startupScript.AppendLine("bin/pulsar sql-worker start");//start the worker as daemon process.
            return container.CopyFileAsync(StartupScriptPath, Encoding.UTF8.GetBytes(startupScript.ToString()), 0x1ff, ct: ct);
        };
    }
}
