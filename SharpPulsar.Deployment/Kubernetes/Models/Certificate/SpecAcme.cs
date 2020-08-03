using Newtonsoft.Json;
using System.Collections.Generic;


//https://cert-manager.io/docs/concepts/certificate/
namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    public class SpecAcme
    {
        public SpecAcme() { }

        public SpecAcme(
            string server = null,
            string email = null,
            PrivateKeySecretRef privateKeySecretRef = null,
            IList<Solver> solvers = null
        )
        {
            Server = server;
            Email = email;
            PrivateKeySecretRef = privateKeySecretRef;
            Solvers = solvers;
        }

        [JsonProperty(PropertyName = "server")]
        public string Server { get; set; }
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "privateKeySecretRef")]
        public PrivateKeySecretRef PrivateKeySecretRef { get; set; }
        [JsonProperty(PropertyName = "solvers")]
        public IList<Solver> Solvers { get; set; }
    }
}

