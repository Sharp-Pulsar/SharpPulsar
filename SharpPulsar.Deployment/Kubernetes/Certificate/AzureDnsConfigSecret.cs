using k8s;
using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes.Certificate
{
    public class AzureDnsConfigSecret
    {
        private V1Secret _secret;
        private IKubernetes _client;
        private string _namespace;

        public AzureDnsConfigSecret(IKubernetes client, string name, string ns, string password64)
        {
            _client = client;
            _secret = new V1Secret
            {
                Metadata = new V1ObjectMeta 
                { 
                     Name = name,
                     NamespaceProperty = ns
                },
                StringData = new Dictionary<string, string> 
                {
                    //echo <service principal password> | openssl base64
                    {"password", password64 }
                }
            };
        }

        public V1Secret Run(string dryRun = default)
        {
           return _client.CreateNamespacedSecret(_secret, _namespace, dryRun);
        }
        public async Task<V1Secret> RunAsync(string dryRun = default)
        {
           return await _client.CreateNamespacedSecretAsync(_secret, _namespace, dryRun);
        }
    }
}
