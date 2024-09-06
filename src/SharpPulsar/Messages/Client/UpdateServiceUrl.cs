

namespace SharpPulsar.Messages.Client
{
    public record struct UpdateServiceUrl(string ServiceUrl);
    public record struct GetUpdateServiceUrl(string ServiceUrl);
    public record struct GetServiceUrl()
    {
        public static readonly GetServiceUrl Instance = new GetServiceUrl();
    }
    public record struct GetResolvedHost()
    {
        public static readonly GetResolvedHost Instance = new GetResolvedHost();
    }
    
}
