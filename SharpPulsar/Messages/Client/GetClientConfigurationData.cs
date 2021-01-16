using SharpPulsar.Configuration;

namespace SharpPulsar.Messages.Client
{
    public sealed class GetClientConfigurationData
    {
        public static GetClientConfigurationData Instance = new GetClientConfigurationData();
    }
    public sealed class GetClientConfigurationDataResponse
    {
        public ClientConfigurationData ClientConfigurationData { get; }
        public GetClientConfigurationDataResponse(ClientConfigurationData clientConfigurationData)
        {
            ClientConfigurationData = clientConfigurationData;
        }
    }
}
