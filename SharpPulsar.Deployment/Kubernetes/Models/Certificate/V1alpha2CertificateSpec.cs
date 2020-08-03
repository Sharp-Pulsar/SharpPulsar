using Newtonsoft.Json;
using System.Collections.Generic;


//https://cert-manager.io/docs/concepts/certificate/
namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    public class V1alpha2CertificateSpec
    {
        [JsonProperty(PropertyName = "secretName")]
        public string SecretName { get; set; }

        [JsonProperty(PropertyName = "commonName")]
        public string CommonName { get; set; }

        [JsonProperty(PropertyName = "dnsNames")]
        public IList<string> DnsNames { get; set; }

        [JsonProperty(PropertyName = "issuerRef")]
        public IssuerRef IssuerRef { get; set; }
    }
}

