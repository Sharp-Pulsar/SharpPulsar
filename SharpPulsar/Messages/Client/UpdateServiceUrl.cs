namespace SharpPulsar.Messages.Client
{
    public sealed class UpdateServiceUrl
    {
        public string ServiceUrl { get; }
        public UpdateServiceUrl(string serviceUrl)
        {
            ServiceUrl = serviceUrl;
        }
    }
}
