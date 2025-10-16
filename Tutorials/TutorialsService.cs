using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration.Hocon;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNetty.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpPulsar;
using SharpPulsar.Admin.v2;
using SharpPulsar.API;
using SharpPulsar.Auth.OAuth2;
using SharpPulsar.Builder;
using SharpPulsar.Shared;
using Testcontainers.Pulsar;
using static SharpPulsar.Common.Protocol.Proto.CommandSubscribe;

namespace Tutorials
{
    internal class TutorialsService : IHostedService
    {
        private ILogger<TutorialsService> _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task? _timerTask;
        private readonly IServiceProvider _serviceProvider; 
        private readonly PulsarSystem _pulsarSystem;  //static string myTopic = $"persistent://public/default/mytopic-2";
        private static PulsarContainer _container;
        //private static TestcontainerConfiguration _configuration = new("apachepulsar/pulsar-all:2.10.0", 6650);

        static string myTopic = $"persistent://public/default/mytopic-{Guid.NewGuid()}";
        //static string myTopic = $"persistent://public/default/mytopic-pulsar";
        public static string Token { get; private set; }

        public TutorialsService(IServiceProvider serviceProvider, ILogger<TutorialsService> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider; 
            _pulsarSystem = new PulsarSystem(serviceProvider);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timerTask = StartTimerTask();
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            if (_timerTask != null)
                await _timerTask;
        }
        private async Task StartTimerTask()
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(2));

            try
            {
                while (true)
                {
                    if (await periodicTimer.WaitForNextTickAsync(_cancellationTokenSource.Token))
                    {
                        
                    }
                }
            }
            catch
            {
                // no-op
            }
        }
        private static async ValueTask StartContainer()
        {
            _container = BuildContainer()
              .WithCleanUp(true)
              .Build();

            await _container.StartAsync();
            //await _container.ExecAsync(new List<string> { @"./bin/pulsar", "initialize-transaction-coordinator-metadata", "-cs", "localhost:2181", "-c", "standalone", "--initial-num-transaction-coordinators", "2" });
            Console.WriteLine("Start Test Container");
            await AwaitPortReadiness($"http://127.0.0.1:8080/metrics/");
            await _container.ExecAsync(new List<string> { @"./bin/pulsar", "sql-worker", "start" });
            await AwaitPortReadiness($"http://127.0.0.1:8081/");
            Console.WriteLine("AwaitPortReadiness Test Container");
        }
        private static async ValueTask TokenStartContainer()
        {
            var t = TokenBuildContainer();
            _container = t
              .WithCleanUp(true)
              .Build();

            await _container.StartAsync();
            Console.WriteLine("Start Test Container");
            await AwaitPortReadiness($"http://127.0.0.1:8080/metrics/");

            await Task.Delay(2000);
            var s = await _container.ExecAsync(new List<string> { @"./bin/pulsar", "tokens", "create", "--secret-key", "/pulsar/secret.key", "--subject", "test-user" });
            Token = s.Stdout;
            // await AwaitPortReadiness($"http://127.0.0.1:8081/");
        }
        private static PulsarBuilder BuildContainer()
        {
            return new PulsarBuilder()
                .WithImage("apachepulsar/pulsar-all:4.1.1")
                .WithPortBinding(6650, 6650)
                .WithPortBinding(8080, 8080)
                .WithPortBinding(8081, 8081)
                //.WithPortBinding(8081, true)
                .WithEnvironment("PULSAR_MEM", "-Xms512m -Xmx512m -XX:MaxDirectMemorySize=1g")
                .WithEnvironment("PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled", "true")
                .WithEnvironment("PULSAR_PREFIX_nettyMaxFrameSizeBytes", "5253120")
                .WithEnvironment("PULSAR_PREFIX_transactionCoordinatorEnabled", "true")
                .WithEnvironment("PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled", "true")
                .WithEnvironment("PULSAR_PREFIX_defaultRetentionTimeInMinutes", "-1") //Default message retention time. 0 means retention is disabled. -1 means data is not removed by time quota
                .WithEnvironment("PULSAR_PREFIX_defaultRetentionSizeInMB", "-1") //Default retention size. 0 means retention is disabled. -1 means data is not removed by size quota
                .WithEnvironment("PULSAR_STANDALONE_USE_ZOOKEEPER", "1")
                .WithEnvironment("PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled", "true")
                .WithEnvironment("PULSAR_PREFIX_brokerEntryMetadataInterceptors", "org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor")
                .WithWaitStrategy(Wait.ForUnixContainer())
                .WithStartupCallback((container, ct) =>
                {
                    const char lf = '\n';
                    var startupScript = new StringBuilder();
                    startupScript.Append("#!/bin/bash");
                    startupScript.Append(lf);
                    startupScript.Append("bin/apply-config-from-env.py conf/standalone.conf ");
                    startupScript.Append("&& bin/pulsar standalone --no-functions-worker ");
                    startupScript.Append("&& bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2");

                    return container.CopyAsync(Encoding.Default.GetBytes(startupScript.ToString()), "/testcontainers.sh", Unix.FileMode755, ct: ct);
                });
        }
        private static PulsarBuilder TokenBuildContainer()
        {
            return new PulsarBuilder()
                .WithImage("apachepulsar/pulsar-all:4.1.1")
                .WithPortBinding(8081, true)
                .WithEnvironment("PULSAR_MEM", "-Xms512m -Xmx512m -XX:MaxDirectMemorySize=1g")
                .WithEnvironment("PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled", "true")
                .WithEnvironment("PULSAR_PREFIX_nettyMaxFrameSizeBytes", "5253120")
                .WithEnvironment("PULSAR_PREFIX_transactionCoordinatorEnabled", "true")
                .WithEnvironment("PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled", "true")
                .WithEnvironment("PULSAR_PREFIX_defaultRetentionTimeInMinutes", "-1") //Default message retention time. 0 means retention is disabled. -1 means data is not removed by time quota
                .WithEnvironment("PULSAR_PREFIX_defaultRetentionSizeInMB", "-1") //Default retention size. 0 means retention is disabled. -1 means data is not removed by size quota
                .WithEnvironment("PULSAR_STANDALONE_USE_ZOOKEEPER", "1")
                .WithEnvironment("PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled", "true")
                .WithEnvironment("PULSAR_PREFIX_brokerEntryMetadataInterceptors", "org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor")
                .WithAuthentication()
                .WithWaitStrategy(Wait.ForUnixContainer())
                .WithStartupCallback((container, ct) =>
                {
                    const char lf = '\n';
                    var startupScript = new StringBuilder();
                    startupScript.Append("#!/bin/bash");
                    startupScript.Append(lf);
                    startupScript.Append("bin/apply-config-from-env.py conf/standalone.conf ");
                    startupScript.Append("&& bin/pulsar standalone --no-functions-worker ");
                    startupScript.Append("&& bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2");

                    return container.CopyAsync(Encoding.Default.GetBytes(startupScript.ToString()), "/testcontainers.sh", Unix.FileMode755, ct: ct);
                });
        }
        internal async Task RunOauth()
        {
            var fileUri = new Uri(GetConfigFilePath());
            var issuerUrl = new Uri("https://auth.streamnative.cloud/");
            var audience = "urn:sn:pulsar:o-r7y4o:sharp";

            var serviceUrl = "pulsar://localhost:6650";
            var subscriptionName = "my-subscription";
            var topicName = $"my-topic-%{DateTime.Now.Ticks}";

            var clientConfig = new ClientBuilder()
                .ServiceUrl(serviceUrl)
                //.AddTlsCerts()
                .Authentication(AuthenticationFactoryOAuth2.ClientCredentials(issuerUrl, fileUri, audience));

            //pulsar actor system
            var pulsarClient = await _pulsarSystem.NewClient("RunOauth", clientConfig);

            var producer = pulsarClient.NewProducer(new ProducerBuilder<byte[]>()
                .Topic(topicName));

            var consumer = pulsarClient.NewConsumer(new ConsumerBuilder<byte[]>()
                .Topic(topicName)
                .SubscriptionName(subscriptionName)
                .SubscriptionType((SubscriptionType.Exclusive));

            var messageId = await producer.SendAsync(Encoding.UTF8.GetBytes($"Sent from C# at '{DateTime.Now}'"));
            Console.WriteLine($"MessageId is: '{messageId}'");

            var message = await consumer.ReceiveAsync();
            Console.WriteLine($"Received: {Encoding.UTF8.GetString(message.Data)}");

            await consumer.AcknowledgeAsync(message.MessageId);
        }
        static string GetConfigFilePath()
        {
            var configFolderName = "Oauth2Files";
            var privateKeyFileName = "o-r7y4o-eabanonu.json";
            var startup = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var indexOfConfigDir = startup.IndexOf(startup, StringComparison.Ordinal);
            var examplesFolder = startup.Substring(0, startup.Length - indexOfConfigDir);
            var configFolder = Path.Combine(examplesFolder, configFolderName);
            var ret = Path.Combine(configFolder, privateKeyFileName);
            if (!File.Exists(ret)) throw new FileNotFoundException("can't find credentials file");
            return ret;
        }
        private static async ValueTask AwaitPortReadiness(string address)
        {
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
                    await client.GetAsync(address).ConfigureAwait(false);
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
    }
    public class Students
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
    }
    public class DataOp
    {
        public string Text { get; set; }
    }
    public class JournalEntry
    {
        public string Id { get; set; }

        public string PersistenceId { get; set; }

        public long SequenceNr { get; set; }

        public bool IsDeleted { get; set; }

        public byte[] Payload { get; set; }
        public long Ordering { get; set; }
        public string Tags { get; set; }
    }
}

