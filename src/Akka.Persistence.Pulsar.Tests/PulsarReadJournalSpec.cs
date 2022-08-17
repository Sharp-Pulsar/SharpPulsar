
using Akka.Configuration;
using Xunit;

namespace Akka.Persistence.Pulsar.Tests
{
    [Collection("PulsarReadJournalSpec")]
    public class PulsarReadJournalSpec
    {
        private static readonly Config SpecConfig = ConfigurationFactory.ParseString(@"
            akka.persistence.journal.plugin = ""akka.persistence.journal.pulsar""
            akka.test.single-expect-default = 180s
        ").WithFallback(PulsarPersistence.DefaultConfiguration());
    }
}
