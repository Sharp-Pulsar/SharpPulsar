using k8s;
using SharpPulsar.Deployment.Kubernetes.Models.Certificate;
using System;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes.Certificate
{
    public class AzureClusterIssuer
    {
        private IKubernetes _client;
        private V1alpha2ClusterIssuer _issuer;
        public AzureClusterIssuer(IKubernetes client)
        {
            _client = client;
            _issuer = new V1alpha2ClusterIssuer
            {
                Metadata = new k8s.Models.V1ObjectMeta
                {
                    Name = "letsencrypt"
                },
                Spec = new SpecAcme 
                { 
                    Solvers = new Solver
                    {
                        Dns01 = new Dns01Solver
                        {
                            AzureDns = new AzureDns
                            {

                            }
                        }
                    }
                }
            };
        }
        public AzureClusterIssuer Server(string server)
        {
            _issuer.Spec.Server = server;
            return this;
        }
        public AzureClusterIssuer Email(string email)
        {
            _issuer.Spec.Email = email;
            return this;
        }
        public AzureClusterIssuer DnsEmail(string email)
        {
            _issuer.Spec.Solvers.Dns01.AzureDns.Email = email;
            return this;
        }
        public AzureClusterIssuer ClientId(string clientid)
        {
            _issuer.Spec.Solvers.Dns01.AzureDns.ClientID = clientid;
            return this;
        }
        public AzureClusterIssuer HostedZoneName(string hostedZoneName)
        {
            _issuer.Spec.Solvers.Dns01.AzureDns.HostedZoneName = hostedZoneName;
            return this;
        }
        public AzureClusterIssuer ResourceGroupName(string resourceGroupName)
        {
            _issuer.Spec.Solvers.Dns01.AzureDns.ResourceGroupName = resourceGroupName;
            return this;
        }
        public AzureClusterIssuer SubscriptionId(string subscriptionId)
        {
            _issuer.Spec.Solvers.Dns01.AzureDns.SubscriptionID = subscriptionId;
            return this;
        }
        public AzureClusterIssuer TenantId(string tenantId)
        {
            _issuer.Spec.Solvers.Dns01.AzureDns.TenantID = tenantId;
            return this;
        }
        public AzureClusterIssuer ClientSecretSecretRef(string key, string name)
        {
            _issuer.Spec.Solvers.Dns01.AzureDns.ClientSecretSecretRef = new ClientSecretSecretRef 
            { 
                Key = key,
                Name = name
            };
            return this;
        }
        public AzureClusterIssuer PrivateKey(string keyName)
        {
            _issuer.Spec.PrivateKeySecretRef = new PrivateKeySecretRef
            {
                Name = keyName
            };
            return this;
        }
        public V1alpha2ClusterIssuer Run()
        {
            if (string.IsNullOrWhiteSpace(_issuer.Spec.Email))
                throw new ArgumentException("Email is missing"); 
            
            if (string.IsNullOrWhiteSpace(_issuer.Spec.Server))
                throw new ArgumentException("Server is missing");            
            
            if (string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.Email))
                throw new ArgumentException("DnsEmail is missing");
            
            if (string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.ClientID))
                throw new ArgumentException("ClientId is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.HostedZoneName))
                throw new ArgumentException("HostedZoneName is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.ResourceGroupName))
                throw new ArgumentException("ResourceGroupName is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.SubscriptionID))
                throw new ArgumentException("SubscriptionId is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.TenantID))
                throw new ArgumentException("TenantId is missing");
            
            if (_issuer.Spec.PrivateKeySecretRef == null || string.IsNullOrWhiteSpace(_issuer.Spec.PrivateKeySecretRef.Name))
                throw new ArgumentException("PrivateKeySecretRef not set");
            
            if (_issuer.Spec.Solvers.Dns01.AzureDns.ClientSecretSecretRef == null || string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.ClientSecretSecretRef.Key) || string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.ClientSecretSecretRef.Name))
                throw new ArgumentException("ClientSecretSecretRef not set");

            return (V1alpha2ClusterIssuer)_client.CreateClusterCustomObject(_issuer, "cert-manager.io", "v1alpha2", "clusterissuers", "true");
        }

        public async Task<V1alpha2ClusterIssuer> RunAsync()
        {
            if (string.IsNullOrWhiteSpace(_issuer.Spec.Email))
                throw new ArgumentException("Email is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Server))
                throw new ArgumentException("Server is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.Email))
                throw new ArgumentException("DnsEmail is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.ClientID))
                throw new ArgumentException("ClientId is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.HostedZoneName))
                throw new ArgumentException("HostedZoneName is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.ResourceGroupName))
                throw new ArgumentException("ResourceGroupName is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.SubscriptionID))
                throw new ArgumentException("SubscriptionId is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.TenantID))
                throw new ArgumentException("TenantId is missing");

            if (_issuer.Spec.PrivateKeySecretRef == null || string.IsNullOrWhiteSpace(_issuer.Spec.PrivateKeySecretRef.Name))
                throw new ArgumentException("PrivateKeySecretRef not set");

            if (_issuer.Spec.Solvers.Dns01.AzureDns.ClientSecretSecretRef == null || string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.ClientSecretSecretRef.Key) || string.IsNullOrWhiteSpace(_issuer.Spec.Solvers.Dns01.AzureDns.ClientSecretSecretRef.Name))
                throw new ArgumentException("ClientSecretSecretRef not set");

            var result = await _client.CreateClusterCustomObjectAsync(_issuer, "cert-manager.io", "v1alpha2", "clusterissuers", "true");
            return (V1alpha2ClusterIssuer)result;
        }
    }
}
