using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using SharpPulsar.TestContainer;

namespace SharpPulsar.Test.Integration.Fixtures
{
    public sealed class IntegrationContainerConfiguration : PulsarTestcontainerConfiguration
    {
        public IntegrationContainerConfiguration(string image, int port) : base(image, port)
        {
        }

        public override Func<IRunningDockerContainer, CancellationToken, Task> StartupCallback => (container, ct) =>
        {
            var startupScript = new StringBuilder();
            startupScript.AppendLine("#!/bin/sh");
            startupScript.AppendLine("echo up and running");
            return container.CopyFileAsync(StartupScriptPath, Encoding.UTF8.GetBytes(startupScript.ToString()), 0x1ff, ct: ct);
        };

    }
}
