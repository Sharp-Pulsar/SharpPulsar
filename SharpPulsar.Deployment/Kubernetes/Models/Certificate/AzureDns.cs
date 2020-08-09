using Newtonsoft.Json;

namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    internal class AzureDns
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "clientID")]
        public string ClientID { get; set; }

        [JsonProperty(PropertyName = "clientSecretSecretRef")]
        public ClientSecretSecretRef ClientSecretSecretRef { get; set; }

        [JsonProperty(PropertyName = "hostedZoneName")]
        public string HostedZoneName { get; set; }

        [JsonProperty(PropertyName = "resourceGroupName")]
        public string ResourceGroupName { get; set; }

        [JsonProperty(PropertyName = "subscriptionID")]
        public string SubscriptionID { get; set; }

        [JsonProperty(PropertyName = "tenantID")]
        public string TenantID { get; set; }

        [JsonProperty(PropertyName = "environment")]
        public string Environment { get; set; } = "AzurePublicCloud";
    }
}
