using k8s;
using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.IngressSetup.ConfigMaps
{
    internal class ValidatingWebhookConfiguration
    {
        private readonly IKubernetes _client;
        public ValidatingWebhookConfiguration(IKubernetes client)
        {
            _client = client;
        }
        public RunResult Run(string dryRun = default)
        {
            var validate = new V1ValidatingWebhookConfiguration
            {
                Metadata = new V1ObjectMeta
                {
                    Name = "ingress-nginx-admission", 
                    Labels = new Dictionary<string, string>
                    {
                        {"helm.sh/chart", "ingress-nginx-2.11.1"},
                        {"app.kubernetes.io/name", "ingress-nginx"},
                        {"app.kubernetes.io/instance", "ingress-nginx"},
                        {"app.kubernetes.io/version", "0.34.1"},
                        {"app.kubernetes.io/managed-by", "Helm"},
                        {"app.kubernetes.io/component", "admission-webhook"}
                    }
                },
                Webhooks = new List<V1ValidatingWebhook>
                {
                    new V1ValidatingWebhook
                    {
                        Name = "validate.nginx.ingress.kubernetes.io",
                        Rules = new List<V1RuleWithOperations>
                        {
                            new V1RuleWithOperations
                            {
                                ApiGroups = new [] {"extensions" , "networking.k8s.io" },
                                ApiVersions = new [] { "v1beta1" },
                                Operations = new [] {"CREATE", "UPDATE" },
                                Resources = new [] { "ingresses" }
                            }
                        },
                        FailurePolicy = "Fail",
                        SideEffects = "None",
                        AdmissionReviewVersions = new [] { "v1", "v1beta1" },
                        ClientConfig = new Admissionregistrationv1WebhookClientConfig
                        {
                            Service = new Admissionregistrationv1ServiceReference
                            {
                                NamespaceProperty = "ingress-nginx", 
                                Name = "ingress-nginx-controller-admission",
                                Path = "/extensions/v1beta1/ingresses"
                            }
                            
                        }
                    }
                }
            };
            var result = new RunResult();
            try
            {
                result.Response = _client.CreateValidatingWebhookConfiguration(validate, dryRun);
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
