using k8s;

namespace SharpPulsar.Deployment.Kubernetes.Certificate
{
    internal class CertRunner
    {
        private IKubernetes _client;

        public CertRunner(IKubernetes client)
        {
            _client = client;
        }

        public void CreateClusterIssuer()
        {
            if (Values.Tls.Enabled)
            {
                if (Values.Tls.Proxy.Enabled)
                {

                }
                if (Values.Tls.Broker.Enabled)
                {

                }
                if (Values.Tls.Presto.Enabled)
                {

                }
            }
        }
        public void CreateIngress()
        {
            var ingress = new ClusterIngress(_client);
            if (Values.Ingress.Enabled)
            {
                if (Values.Ingress.Proxy.Enabled)
                {
                    if(Values.Tls.Proxy.Enabled)
                    {
                        ingress.AddTls(new[] { $"pulsar-data.{Values.Ingress.DomainSuffix}", $"pulsar-admin.{Values.Ingress.DomainSuffix}" }, Values.Tls.Proxy.CertName);
                    }
                    ingress.AddRule($"pulsar-data.{Values.Ingress.DomainSuffix}", "/", Values.Proxy.ServiceName, 6650);
                    ingress.AddRule($"pulsar-admin.{Values.Ingress.DomainSuffix}", "/", Values.Proxy.ServiceName, 8080);
                }
                if (Values.Ingress.Broker.Enabled)
                {
                    if (Values.Tls.Enabled && Values.Tls.Broker.Enabled)
                    {

                    }
                    else
                    {

                    }
                }
                if (Values.Ingress.Presto.Enabled)
                {
                    if (Values.Tls.Enabled && Values.Tls.Presto.Enabled)
                    {

                    }
                    else
                    {

                    }
                }
            }
        }
    }
}
