
using Akka.Persistence.Pulsar.Journal;
using SharpPulsar;
using SharpPulsar.User;

namespace Akka.Persistence.Pulsar
{
    public static class PulsarStatic
    {
        public static PulsarSystem System { get; set; }
        public static PulsarClient Client { get; set; }
        public static Producer<JournalEntry> Producer { get; set; }
    }
}
