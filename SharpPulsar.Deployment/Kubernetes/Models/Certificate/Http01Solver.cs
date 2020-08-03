using Newtonsoft.Json;


//https://cert-manager.io/docs/concepts/certificate/
namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    public class Http01Solver
    {
        public Http01Solver() { }

        public Http01Solver(Ingress ingress = null)
        {
            Ingress = ingress;
        }

        [JsonProperty(PropertyName = "ingress")]
        public Ingress Ingress { get; set; }
    }
}

