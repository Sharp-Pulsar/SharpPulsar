using Newtonsoft.Json;


//https://cert-manager.io/docs/concepts/certificate/
namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    public class Solver
    {
        [JsonProperty(PropertyName = "http01")]
        public Http01Solver Http01 { get; set; }

        [JsonProperty(PropertyName = "dns01")]
        public Dns01Solver Dns01 { get; set; }
    }
}

