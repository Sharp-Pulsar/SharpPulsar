using k8s;
using SharpPulsar.Deployment.Kubernetes.Models.Certificate;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes.Certificate
{
    public class WildcardCertificate
    {
        private V1alpha2Certificate _cert;
        private IKubernetes _client;

        public WildcardCertificate(IKubernetes client, string name)
        {
            _client = client;
            _cert = new V1alpha2Certificate
            {
                Metadata = new k8s.Models.V1ObjectMeta
                {
                    Name = name
                },
                Spec = new V1alpha2CertificateSpec
                {
                    SecretName = name
                }
            };
        }
        public WildcardCertificate CommonName(string cname)
        {
            _cert.Spec.CommonName = cname.StartsWith("*.") ? cname : $"*.{cname}";
            return this;
        }
        public WildcardCertificate IssuerRef(string name)
        {
            _cert.Spec.IssuerRef = new IssuerRef
            {
                Name = name,
                Kind = "ClusterIssuer"
            };
            return this;
        }
        public WildcardCertificate DnsNames(List<string> names)
        {
            _cert.Spec.DnsNames = new List<string>(names);
            return this;
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
