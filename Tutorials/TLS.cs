using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPulsar;
using SharpPulsar.Configuration;

namespace Tutorials
{
    internal static class TLS
    {
        public static void TLSMain()
        {
            //pulsar client settings builder
            Console.WriteLine("Please enter cmd");
            var cmd = Console.ReadLine();
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650");
            if (cmd.Equals("txn", StringComparison.OrdinalIgnoreCase))
                clientConfig.EnableTransaction(true);

            //pulsar actor system
            var pulsarSystem = PulsarSystem.GetInstance(clientConfig);
        }
    }
}
