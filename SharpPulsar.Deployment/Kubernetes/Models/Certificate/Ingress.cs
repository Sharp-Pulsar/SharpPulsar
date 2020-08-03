using Newtonsoft.Json;


//https://cert-manager.io/docs/concepts/certificate/
namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    public class Ingress
    {
        public Ingress() { }

        public Ingress(string classP = null)
        {
            Class = classP;
        }

        [JsonProperty(PropertyName = "class")]
        public string Class { get; set; }
    }
}

