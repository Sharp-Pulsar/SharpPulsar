using k8s;
using SharpPulsar.Deployment.Kubernetes.Models.Certificate;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes.Certificate
{//https://cert-manager.io/docs/installation/kubernetes/
    internal class WildcardCertificate
    {
        private readonly V1alpha2Certificate _cert;
        private readonly IKubernetes _client;

        public WildcardCertificate(IKubernetes client)
        {
            _client = client;
            _cert = new V1alpha2Certificate
            {
                Metadata = new k8s.Models.V1ObjectMeta
                {
                    Name = "pulsar-wildcard"
                },
                Spec = new V1alpha2CertificateSpec
                {
                    SecretName = $"{Values.ReleaseName}-azure-dns-secret",
                    CommonName = $"*.{Values.Ingress.DomainSuffix}",
                    DnsNames = new[] {Values.Ingress.DomainSuffix},
                    IssuerRef = new IssuerRef
                    {
                        Name = "letsencrypt",
                        Kind = "ClusterIssuer"
                    }
                }
            };
        }
        
        public V1alpha2Certificate Run()
        {
            var result = _client.CreateClusterCustomObject(_cert, "cert-manager.io", "v1alpha2", "certificates", "true");
            return (V1alpha2Certificate)result;
        }
        public async Task<V1alpha2Certificate> RunAsync()
        {
            var result = await _client.CreateClusterCustomObjectAsync(_cert, "cert-manager.io", "v1alpha2", "certificates", "true");
            return (V1alpha2Certificate)result;
        }
    }
}
