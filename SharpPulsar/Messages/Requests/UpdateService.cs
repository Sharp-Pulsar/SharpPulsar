namespace SharpPulsar.Messages.Requests
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
