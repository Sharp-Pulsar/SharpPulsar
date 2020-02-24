using System;
using System.Threading;
using DotNetty.Common.Internal.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using SharpPulsar.Api;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            InternalLoggerFactory.DefaultFactory = SharpPulsar.Utility.Log.Logger;//.AddProvider(new ConsoleLoggerProvider(new OptionsMonitor<ConsoleLoggerOptions>(null, null, null)));
            ILogger logger = InternalLoggerFactory.DefaultFactory.CreateLogger<Program>();
            
            logger.LogInformation("Example log message");

            var client = IPulsarClient.Builder().ServiceUrl("pulsar://localhost:6650").Build();

            var producer = client.NewProducer().Topic("persistent://my-tenant/my-ns/my-topic").Create().GetAwaiter().GetResult();

            for (var i = 0; i < 10; i++)
            {
                var m =producer.Send("my-message".GetBytes());
                Console.WriteLine(m);
            }

            client.Dispose();
            while (true)
            {
               Thread.Sleep(100);
            }
        }
    }
}
