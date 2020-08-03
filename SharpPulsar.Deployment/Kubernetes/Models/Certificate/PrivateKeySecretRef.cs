using Newtonsoft.Json;


//https://cert-manager.io/docs/concepts/certificate/
namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    public class PrivateKeySecretRef
    {
        public PrivateKeySecretRef() { }

        public PrivateKeySecretRef(string name = null)
        {
            Name = name;
        }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}

