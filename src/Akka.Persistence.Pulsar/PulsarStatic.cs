
using SharpPulsar;
using SharpPulsar.User;

namespace Akka.Persistence.Pulsar
{
    public static class PulsarStatic
    {
        public static PulsarSystem System { get; set; }
        public static PulsarClient Client { get; set; }
    }
}
