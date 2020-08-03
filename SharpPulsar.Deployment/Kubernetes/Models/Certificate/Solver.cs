using Newtonsoft.Json;


//https://cert-manager.io/docs/concepts/certificate/
namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    public class Solver
    {
        public Solver() { }

        public Solver(Http01Solver http01 = null)
        {
            Http01 = http01;
        }

        [JsonProperty(PropertyName = "http01")]
        public Http01Solver Http01 { get; set; }
    }
}

