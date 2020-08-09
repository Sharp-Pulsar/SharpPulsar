using Newtonsoft.Json;

namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    internal class ClientSecretSecretRef
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
