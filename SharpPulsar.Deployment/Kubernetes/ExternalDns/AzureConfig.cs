using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.ExternalDns
{
    internal class AzureConfig
    {
        private readonly Secret _secret;
        public AzureConfig(Secret secret)
        {
            _secret = secret;
        }
        public RunResult Run(string dryRun = default)
        {
            _secret.Builder()
                .Metadata("azure-config-file", "cert-manager")
                .KeyValue("azure.json", $@"{{
      ""tenantId"": ""{Values.ExternalDnsConfig.TenantId}"",
      ""subscriptionId"": ""{Values.ExternalDnsConfig.SubscriptionId }"",
      ""resourceGroup"": ""{Values.ExternalDnsConfig.ResourceGroup }"",
      ""aadClientId"": ""{Values.ExternalDnsConfig.ServicePrincipalAppId }"",
      ""aadClientSecret"": ""{Values.ExternalDnsConfig.ServicePrincipalSecret }""}}");

            return _secret.Run(_secret.Builder(), "cert-manager", dryRun);
        }
    }
}
