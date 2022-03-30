using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Xunit.Abstractions;

namespace SharpPulsar.Test.OTel
{
    internal class TestConsoleExporter
    {
        // To run this example, run the following command from
        // the reporoot\examples\Console\.
        // (eg: C:\repos\opentelemetry-dotnet\examples\Console\)
        //
        // dotnet run console
        public static object Run(ITestOutputHelper output)
        {
            var exportedItems = new List<Activity>();
            RunWithActivitySource(exportedItems);
            Task.Run(() => 
            {
                // List exportedItems is populated with the Activity objects logged by TracerProvider
                foreach (var activity in exportedItems)
                {
                    output.WriteLine($"ActivitySource: {activity.Source.Name} logged the activity {activity.DisplayName}: {activity.Tags}");
                }
            });
            return null;
        }

        private static object RunWithActivitySource(ICollection<Activity> exportedItems)
        {
            // Enable OpenTelemetry for the sources "Samples.SampleServer" and "Samples.SampleClient"
            // and use Console exporter.
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .AddSource("producer", "consumer")
                     .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("inmemory-test"))
                    .AddInMemoryExporter(exportedItems)
                    .Build();

            return null;
        }

    }
}
