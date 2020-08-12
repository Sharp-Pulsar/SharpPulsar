using k8s;
using SharpPulsar.Deployment.Kubernetes.Models.Certificate;
using System;
using System.Threading.Tasks;

namespace SharpPulsar.Deployment.Kubernetes.Certificate
{//https://cert-manager.io/docs/installation/kubernetes/
    internal class AzureClusterIssuer
    {
        private readonly IKubernetes _client;
        private readonly V1alpha2ClusterIssuer _issuer;
        public AzureClusterIssuer(IKubernetes client)
        {
            _client = client;
            _issuer = new V1alpha2ClusterIssuer
            {
                Metadata = new k8s.Models.V1ObjectMeta
                {
                    Name = "letsencrypt"
                },
                Spec = new V1alpha2ClusterIssuerSpec
                {
                    Acme = new SpecAcme
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
                }
            };
        }
        public AzureClusterIssuer Server(string server)
        {
            _issuer.Spec.Acme.Server = server;
            return this;
        }
        public AzureClusterIssuer Email(string email)
        {
            _issuer.Spec.Acme.Email = email;
            return this;
        }
        public AzureClusterIssuer DnsEmail(string email)
        {
            _issuer.Spec.Acme.Solvers.Dns01.AzureDns.Email = email;
            return this;
        }
        public AzureClusterIssuer ClientId(string clientid)
        {
            _issuer.Spec.Acme.Solvers.Dns01.AzureDns.ClientID = clientid;
            return this;
        }
        public AzureClusterIssuer HostedZoneName(string hostedZoneName)
        {
            _issuer.Spec.Acme.Solvers.Dns01.AzureDns.HostedZoneName = hostedZoneName;
            return this;
        }
        public AzureClusterIssuer ResourceGroupName(string resourceGroupName)
        {
            _issuer.Spec.Acme.Solvers.Dns01.AzureDns.ResourceGroupName = resourceGroupName;
            return this;
        }
        public AzureClusterIssuer SubscriptionId(string subscriptionId)
        {
            _issuer.Spec.Acme.Solvers.Dns01.AzureDns.SubscriptionID = subscriptionId;
            return this;
        }
        public AzureClusterIssuer TenantId(string tenantId)
        {
            _issuer.Spec.Acme.Solvers.Dns01.AzureDns.TenantID = tenantId;
            return this;
        }
        public AzureClusterIssuer ClientSecretSecretRef(string key, string name)
        {
            _issuer.Spec.Acme.Solvers.Dns01.AzureDns.ClientSecretSecretRef = new ClientSecretSecretRef 
            { 
                Key = key,
                Name = name
            };
            return this;
        }
        public AzureClusterIssuer PrivateKey(string keyName)
        {
            _issuer.Spec.Acme.PrivateKeySecretRef = new PrivateKeySecretRef
            {
                Name = keyName
            };
            return this;
        }
        public RunResult Run()
        {
            var result = new RunResult();
            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Email))
                throw new ArgumentException("Email is missing"); 
            
            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Server))
                throw new ArgumentException("Server is missing");            
            
            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.Email))
                throw new ArgumentException("DnsEmail is missing");
            
            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.ClientID))
                throw new ArgumentException("ClientId is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.HostedZoneName))
                throw new ArgumentException("HostedZoneName is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.ResourceGroupName))
                throw new ArgumentException("ResourceGroupName is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.SubscriptionID))
                throw new ArgumentException("SubscriptionId is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.TenantID))
                throw new ArgumentException("TenantId is missing");
            
            if (_issuer.Spec.Acme.PrivateKeySecretRef == null || string.IsNullOrWhiteSpace(_issuer.Spec.Acme.PrivateKeySecretRef.Name))
                throw new ArgumentException("PrivateKeySecretRef not set");
            
            if (_issuer.Spec.Acme.Solvers.Dns01.AzureDns.ClientSecretSecretRef == null || string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.ClientSecretSecretRef.Key) || string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.ClientSecretSecretRef.Name))
                throw new ArgumentException("ClientSecretSecretRef not set");

            try
            {
                result.Response = _client.CreateClusterCustomObject(_issuer, "cert-manager.io", "v1alpha2", "clusterissuers", "true");
                result.Success = true;
            }
            catch (Microsoft.Rest.RestException ex)
            {
                if (ex is Microsoft.Rest.HttpOperationException e)
                    result.HttpOperationException = e;
                else
                    result.Exception = ex;
                result.Success = false;
            }
            return result;
        }

        public async Task<RunResult> RunAsync()
        {
            var result = new RunResult();
            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Email))
                throw new ArgumentException("Email is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Server))
                throw new ArgumentException("Server is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.Email))
                throw new ArgumentException("DnsEmail is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.ClientID))
                throw new ArgumentException("ClientId is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.HostedZoneName))
                throw new ArgumentException("HostedZoneName is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.ResourceGroupName))
                throw new ArgumentException("ResourceGroupName is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.SubscriptionID))
                throw new ArgumentException("SubscriptionId is missing");

            if (string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.TenantID))
                throw new ArgumentException("TenantId is missing");

            if (_issuer.Spec.Acme.PrivateKeySecretRef == null || string.IsNullOrWhiteSpace(_issuer.Spec.Acme.PrivateKeySecretRef.Name))
                throw new ArgumentException("PrivateKeySecretRef not set");

            if (_issuer.Spec.Acme.Solvers.Dns01.AzureDns.ClientSecretSecretRef == null || string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.ClientSecretSecretRef.Key) || string.IsNullOrWhiteSpace(_issuer.Spec.Acme.Solvers.Dns01.AzureDns.ClientSecretSecretRef.Name))
                throw new ArgumentException("ClientSecretSecretRef not set");

            try
            {
                result.Response = await _client.CreateClusterCustomObjectAsync(_issuer, "cert-manager.io", "v1alpha2", "clusterissuers", "true");
                result.Success = true;
            }
            catch (Microsoft.Rest.RestException ex)
            {
                if (ex is Microsoft.Rest.HttpOperationException e)
                    result.HttpOperationException = e;
                else
                    result.Exception = ex;
                result.Success = false;
            }
            return result;
        }
    }
}
