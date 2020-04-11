using SharpPulsar.Api;

namespace Samples
{
    public class ServiceUrlProviderImpl: ServiceUrlProvider
    {
        private string _serviceUrl;

        public ServiceUrlProviderImpl(string serviceUrl)
        {
            _serviceUrl = serviceUrl;
        }

        public string ServiceUrl
        {
            get => _serviceUrl;
        }
    }
}
