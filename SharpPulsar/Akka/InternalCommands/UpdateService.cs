
namespace SharpPulsar.Akka.InternalCommands
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
