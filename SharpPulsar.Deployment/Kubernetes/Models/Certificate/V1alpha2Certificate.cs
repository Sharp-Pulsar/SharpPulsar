using k8s;
using k8s.Models;
using Newtonsoft.Json;
using System.Collections.Generic;


//https://cert-manager.io/docs/concepts/certificate/
namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    [KubernetesEntity(Group = "cert-manager.io", Kind = "Certificate", ApiVersion = "v1alpha2", PluralName = "certificates")]
    public class V1alpha2Certificate: IKubernetesObject<V1ObjectMeta>, ISpec<V1alpha2CertificateSpec>
    {
        public const string KubeApiVersion = "v1alpha2";
        public const string KubeKind = "Certificate";
        public const string KubeGroup = "cert-manager.io";

        public V1alpha2Certificate() { }

        public V1alpha2Certificate(
            string apiVersion = null,
            string kind = null,
            V1ObjectMeta metadata = null,
            V1alpha2CertificateSpec spec = null
        )
        {
            ApiVersion = apiVersion;
            Kind = kind;
            Metadata = metadata;
            Spec = spec;
        }

        [JsonProperty(PropertyName = "apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public V1ObjectMeta Metadata { get; set; }

        [JsonProperty(PropertyName = "spec")]
        public V1alpha2CertificateSpec Spec { get; set; }
    }
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
    public class IssuerRef
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        [JsonProperty(PropertyName = "group")]
        public string Group { get; set; }
    }
}

