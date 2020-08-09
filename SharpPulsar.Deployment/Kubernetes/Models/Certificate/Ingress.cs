using Newtonsoft.Json;


//https://cert-manager.io/docs/concepts/certificate/
namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    internal class Ingress
    {
        [JsonProperty(PropertyName = "class")]
        public string Class { get; set; }
    }
}

