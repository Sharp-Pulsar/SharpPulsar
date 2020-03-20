using System;
using System.Collections.Generic;
using System.Text;
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
