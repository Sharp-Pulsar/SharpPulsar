using Newtonsoft.Json;


//https://cert-manager.io/docs/concepts/certificate/
namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    public class Http01Solver
    {
        [JsonProperty(PropertyName = "ingress")]
        public Ingress Ingress { get; set; }
    }
}

