using Newtonsoft.Json;


//https://cert-manager.io/docs/concepts/certificate/
namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    internal class SpecAcme
    {
        [JsonProperty(PropertyName = "server")]
        public string Server { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "privateKeySecretRef")]
        public PrivateKeySecretRef PrivateKeySecretRef { get; set; }

        [JsonProperty(PropertyName = "solvers")]
        public Solver Solvers { get; set; }
    }
}

