/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace SharpPulsar.Test.Fixtures
{
    using Microsoft.Extensions.Configuration;
    using SharpPulsar.Configuration;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Xunit;

    public class PulsarStandaloneClusterFixture : IAsyncLifetime
    {
        public PulsarSystem System;
        public PulsarClientConfigBuilder ClientBuilder;
        public IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }
        public async Task InitializeAsync()
        {
            SetupSystem();
            await DeployPulsar();
        }
        public async Task DeployPulsar()
        {
            //TakeDownPulsar(); // clean-up if anything was left running from previous run

            //RunProcess("docker-compose", "-f docker-compose-standalone-tests.yml up -d");

            var waitTries = 20;

            using var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true
            };

            using var client = new HttpClient(handler);

            while (waitTries > 0)
            {
                try
                {
                    await client.GetAsync("http://127.0.0.1:8080/metrics/").ConfigureAwait(false);
                    return;
                }
                catch
                {
                    waitTries--;
                    await Task.Delay(5000).ConfigureAwait(false);
                }
            }

            throw new Exception("Unable to confirm Pulsar has initialized");
        }
        public async Task DisposeAsync()
        {
            //TakeDownPulsar();
            await System.Shutdown();
        }

        private static void TakeDownPulsar()
            => RunProcess("docker-compose", "-f docker-compose-standalone-tests.yml down");

        private static void RunProcess(string name, string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = name,
                Arguments = arguments,
                RedirectStandardOutput = true,
            };

            processStartInfo.Environment["TAG"] = "test";
            processStartInfo.Environment["CONFIGURATION"] = "Debug";
            processStartInfo.Environment["COMPUTERNAME"] = Environment.MachineName;

            var process = Process.Start(processStartInfo);
            if (process is null)
                throw new Exception("Process.Start returned null");

            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception($"Exit code {process.ExitCode} when running process {name} with arguments {arguments}");
        }
        private void SetupSystem()
        {
            var client = new PulsarClientConfigBuilder();
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var config = GetIConfigurationRoot(path);
            var clienConfigSetting = config.GetSection("client");
            var serviceUrl = clienConfigSetting.GetSection("service-url").Value;
            var webUrl = clienConfigSetting.GetSection("web-url").Value;
            var authPluginClassName = clienConfigSetting.GetSection("authPluginClassName").Value;
            var authParamsString = clienConfigSetting.GetSection("authParamsString").Value;
            var authCertPath = clienConfigSetting.GetSection("authCertPath").Value;
            var connectionsPerBroker = int.Parse(clienConfigSetting.GetSection("connections-per-broker").Value);
            var statsInterval = int.Parse(clienConfigSetting.GetSection("stats-interval").Value);
            var operationTime = int.Parse(clienConfigSetting.GetSection("operationTime").Value);
            var allowTlsInsecureConnection = bool.Parse(clienConfigSetting.GetSection("allowTlsInsecureConnection").Value);
            var enableTls = bool.Parse(clienConfigSetting.GetSection("enableTls").Value);
            var enableTxn = bool.Parse(clienConfigSetting.GetSection("enableTransaction").Value);


            client.EnableTransaction(enableTxn);
            if (operationTime > 0)
                client.OperationTimeout(operationTime);

            if (!string.IsNullOrWhiteSpace(authCertPath))
                client.AddTrustedAuthCert(new X509Certificate2(File.ReadAllBytes(authCertPath)));

            if (!string.IsNullOrWhiteSpace(authPluginClassName) && !string.IsNullOrWhiteSpace(authParamsString))
                client.Authentication(authPluginClassName, authParamsString);

            client.ServiceUrl(serviceUrl);
            client.WebUrl(webUrl);
            client.ConnectionsPerBroker(connectionsPerBroker);
            client.StatsInterval(statsInterval);
            client.AllowTlsInsecureConnection(allowTlsInsecureConnection);
            client.EnableTls(enableTls);
            ClientBuilder = client;
            //client.UseDedicatedConnections(true);
            System = PulsarSystem.GetInstance(client);
        }
    }
}
