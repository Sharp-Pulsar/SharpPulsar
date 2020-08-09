using Newtonsoft.Json;


//https://cert-manager.io/docs/concepts/certificate/
namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    internal class PrivateKeySecretRef
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}

