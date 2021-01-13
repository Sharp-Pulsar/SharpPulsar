
namespace SharpPulsar.Messages
{
    public sealed class UpdateService
    {
        public UpdateService(string service)
        {
            Service = service;
        }

        public string Service { get; }
    }
}
