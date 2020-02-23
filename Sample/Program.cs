using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using SharpPulsar.Api;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            
            ILogger logger = SharpPulsar.Utility.Log.Logger.CreateLogger<Program>();
            logger.LogInformation("Example log message");

            var client = IPulsarClient.Builder().ServiceUrl("pulsar://localhost:6650").Build();

            var producer = client.NewProducer().Topic("persistent://my-tenant/my-ns/my-topic").Create().GetAwaiter().GetResult();

            for (var i = 0; i < 10; i++)
            {
                var m =producer.Send("my-message".GetBytes());
                Console.WriteLine(m);
            }

            client.Dispose();
        }
    }
}
