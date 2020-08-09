using k8s;
using k8s.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Models.Certificate
{
    [KubernetesEntity(Group = "cert-manager.io", Kind = "ClusterIssuer", ApiVersion = "v1alpha2", PluralName = "clusterissuers")]
    public class V1alpha2ClusterIssuer : IKubernetesObject<V1ObjectMeta>, ISpec<V1alpha2ClusterIssuerSpec>
    {
        public const string KubeApiVersion = "v1alpha2";
        public const string KubeKind = "ClusterIssuer";
        public const string KubeGroup = "cert-manager.io";

        [JsonProperty(PropertyName = "apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public V1ObjectMeta Metadata { get; set; }

        [JsonProperty(PropertyName = "spec")]
        public V1alpha2ClusterIssuerSpec Spec { get; set; }
    }
    public class V1alpha2ClusterIssuerSpec
    {
        [JsonProperty(PropertyName = "acme")]
        public SpecAcme Acme { get; set; }
    }
}
