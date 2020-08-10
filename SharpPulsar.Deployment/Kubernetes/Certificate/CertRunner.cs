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
        private Dictionary<string, object> _results;
        public CertRunner(IKubernetes client, Secret secret)
        {
            _results = new Dictionary<string, object>();
            _azureClusterIssuer = new AzureClusterIssuer(client);
            _secret = new AzureDnsConfigSecret(secret);
            _wild = new WildcardCertificate(client);
        }

        public bool Run(out Dictionary<string, object> results, string dryRun = default)
        {

            if (Values.Tls.Enabled)
            {
                _results = new Dictionary<string, object>();

                var azc =_azureClusterIssuer.Run();
                _results.Add("AzureClusterIssuer", azc);

                if (string.IsNullOrWhiteSpace(Values.Tls.SecretPassword))
                    throw new ArgumentException("SecretPassword is required");

                var secret = _secret.Run(Values.Tls.SecretPassword, dryRun);
                _results.Add("Secret", secret);

                var cert = _wild.Run();
                _results.Add("WildcardCertificate", cert);
            }
            results = _results;
            return true;
        }
    }
}
