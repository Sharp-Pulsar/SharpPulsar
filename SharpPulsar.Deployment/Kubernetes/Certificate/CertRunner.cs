using k8s;
using System;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes.Certificate
{
    internal class CertRunner
    {
        private readonly AzureClusterIssuer _azureClusterIssuer;
        private readonly AzureDnsConfigSecret _secret;
        private readonly WildcardCertificate _wild;
        public CertRunner(IKubernetes client, Secret secret)
        {
            _azureClusterIssuer = new AzureClusterIssuer(client);
            _secret = new AzureDnsConfigSecret(secret);
            _wild = new WildcardCertificate(client);
        }

        public IEnumerable<object> Run(string dryRun = default)
        {
            object result;
            if (Values.Tls.Enabled)
            {

                result =_azureClusterIssuer.Run();
                yield return result;

                if (string.IsNullOrWhiteSpace(Values.Tls.SecretPassword))
                    throw new ArgumentException("SecretPassword is required");

                result = _secret.Run(Values.Tls.SecretPassword, dryRun);
                yield return result;

                result = _wild.Run();
                yield return result;
            }
        }
    }
}
