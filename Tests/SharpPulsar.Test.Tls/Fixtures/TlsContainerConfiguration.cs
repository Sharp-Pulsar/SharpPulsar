using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using SharpPulsar.TestContainer;

namespace SharpPulsar.Test.Tls.Fixtures
{
    public sealed class TlsContainerConfiguration : PulsarTestcontainerConfiguration
    {
        public TlsContainerConfiguration(string image, int port) : base(image, port)
        {
        }
        public override void Env(params (string key, string value)[] envs)
        {
            base.Env(envs);
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
