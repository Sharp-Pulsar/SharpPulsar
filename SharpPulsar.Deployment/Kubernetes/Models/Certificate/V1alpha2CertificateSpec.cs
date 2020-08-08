using Newtonsoft.Json;
using System.Collections.Generic;


//https://cert-manager.io/docs/concepts/certificate/
namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    public class V1alpha2CertificateSpec
    {
        [JsonProperty(PropertyName = "secretName")]
        public string SecretName { get; set; }

        [JsonProperty(PropertyName = "duration")]
        public string Duration { get;set; }

        [JsonProperty(PropertyName = "renewBefore")]
        public string RenewBefore { get; set; }

        [JsonProperty(PropertyName = "organization")]
        public string Organization { get; set; }

        [JsonProperty(PropertyName = "commonName")]
        public string CommonName { get; set; }

        [JsonProperty(PropertyName = "isCA")]
        public bool IsCa { get; set; }

        [JsonProperty(PropertyName = "keySize")]
        public int KeySize { get; set; }

        [JsonProperty(PropertyName = "keyAlgorithm")]
        public string KeyAlgorithm { get; set; }

        [JsonProperty(PropertyName = "keyEncoding")]
        public string KeyEncoding { get; set; }

        [JsonProperty(PropertyName = "dnsNames")]
        public IList<string> DnsNames { get; set; }

        [JsonProperty(PropertyName = "usages")]
        public IList<string> Usages { get; set; }

        [JsonProperty(PropertyName = "issuerRef")]
        public IssuerRef IssuerRef { get; set; }
    }
}

