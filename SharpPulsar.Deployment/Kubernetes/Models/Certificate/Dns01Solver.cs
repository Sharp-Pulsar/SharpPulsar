using Newtonsoft.Json;

namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    internal class Dns01Solver
    {
        [JsonProperty(PropertyName = "cnameStrategy")]
        public string CnameStrategy { get; set; } = "Follow";

        [JsonProperty(PropertyName = "azuredns")]
        public AzureDns AzureDns { get; set; }
    }
}
