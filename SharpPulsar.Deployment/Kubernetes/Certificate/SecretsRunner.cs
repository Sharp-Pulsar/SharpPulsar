using k8s;
using SharpPulsar.Deployment.Kubernetes.Certificate.Secrets;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Certificate
{
    internal class SecretsRunner
    {
        private readonly BookieSecret _bookieSecret;
        private readonly BrokerSecret _brokerSecret;
        private readonly ProxySecret _proxySecret;
        private readonly ZooKeeperSecret _zooKeeperSecret;
        private readonly AzureDnsSecret _azureDnsSecret;
        private readonly RecoverySecret _recoverySecret;
        private readonly TlsCa _tlsCa;
        public SecretsRunner(Secret secret)
        {
            _bookieSecret = new BookieSecret(secret);
            _brokerSecret = new BrokerSecret(secret);
            _zooKeeperSecret = new ZooKeeperSecret(secret);
            _proxySecret = new ProxySecret(secret);
            _azureDnsSecret = new AzureDnsSecret(secret);
            _recoverySecret = new RecoverySecret(secret);
            _tlsCa = new TlsCa(secret);
        }
        public IEnumerable<RunResult> Run(string dryRun = default)
        {
            RunResult result;

            result = _azureDnsSecret.Run(dryRun);
            yield return result;

            if (Values.Tls.Enabled)
            {
                result = _tlsCa.Run(dryRun);
                yield return result;

                result = _recoverySecret.Run(dryRun);
                yield return result;

                result = _bookieSecret.Run(dryRun);
                yield return result;

                result = _brokerSecret.Run(dryRun);
                yield return result;

                result = _zooKeeperSecret.Run(dryRun);
                yield return result;

                result = _proxySecret.Run(dryRun);
                yield return result;
            }
        }
    }
}
