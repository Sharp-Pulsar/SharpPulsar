
using SharpPulsar.User;

namespace SharpPulsar.ServiceProvider.Messages
{
    public sealed class Initialize
    {
        public readonly PulsarClient Client;
        public Initialize(PulsarClient client)
        {
            Client = client;    
        }
    }
}
