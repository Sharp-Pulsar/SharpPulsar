using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Prometheus
{
    public class PrometheusYaml
    {
        [JsonProperty(PropertyName = "global")]
        public Global Global { get; set; }

        [JsonProperty(PropertyName = "rule_files")]
        public List<string> RuleFiles { get; set; }

        [JsonProperty(PropertyName = "alerting")]
        public Alerting Alerting { get; set; }

        [JsonProperty(PropertyName = "scrape_configs")]
        public List<IDictionary<string, object>> ScrapeConfigs { get; set; }
    }
    public class Global
    {
        [JsonProperty(PropertyName = "scrape_interval")]
        public string ScrapeInterval { get; set; }
    }
    public class Alerting
    {

        [JsonProperty(PropertyName = "alertmanagers")]
        public AlertManagers AlertManagers { get; set; }
    }
    public class AlertManagers
    {
        [JsonProperty(PropertyName = "static_configs")]
        public StaticConfigs StaticConfigs { get; set; }

        [JsonProperty(PropertyName = "path_prefix")]
        public string PathPrefix { get; set; }
    }
    public class StaticConfigs
    {
        [JsonProperty(PropertyName = "targets")]
        public List<string> Targets { get; set; }
    }
    public class Job
    {
        [JsonProperty(PropertyName = "job_name")]
        public string JobName { get; set; }

        [JsonProperty(PropertyName = "static_configs")]
        public StaticConfigs StaticConfigs { get; set; }

        [JsonProperty(PropertyName = "metrics_path")]
        public string MetricsPath { get; set; }

        [JsonProperty(PropertyName = "bearer_token_file")]
        public string BearerTokenFile { get; set; }

        [JsonProperty(PropertyName = "scheme")]
        public string Scheme { get; set; }

        [JsonProperty(PropertyName = "tls_config")]
        public TlsConfig TlsConfig { get; set; }

        [JsonProperty(PropertyName = "kubernetes_sd_configs")]
        public KubernetesSdConfigs KubernetesSdConfigs { get; set; }

        [JsonProperty(PropertyName = "relabel_configs")]
        public List<RelabelConfig> RelabelConfigs { get; set; }
    }
    public class TlsConfig
    {
        [JsonProperty(PropertyName = "ca_file")]
        public string CaFile { get; set; }
    }
    public class KubernetesSdConfigs
    {
        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }
    }
    public class RelabelConfig
    {
        [JsonProperty(PropertyName = "source_labels")]
        public List<string> SourceLabels { get; set; }

        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "target_label")]
        public string TargetLabel { get; set; }

        [JsonProperty(PropertyName = "regex")]
        public object Regex { get; set; }

        [JsonProperty(PropertyName = "replacement")]
        public string Replacement { get; set; }
    }

    public class RuleGroup
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "rules")]
        public List<IDictionary<string, string>> Rules { get; set; }
    }

    public class RecordRule
    {
        [JsonProperty(PropertyName = "record")]
        public string Record { get; set; }

        [JsonProperty(PropertyName = "expr")]
        public string Expr { get; set; }

        [JsonProperty(PropertyName = "labels")]
        public IDictionary<string, string> Labels { get; set; }
    }
    public class AlertRule
    {
        [JsonProperty(PropertyName = "alert")]
        public string Alert { get; set; }

        [JsonProperty(PropertyName = "expr")]
        public string Expr { get; set; }

        [JsonProperty(PropertyName = "for")]
        public string For { get; set; }

        [JsonProperty(PropertyName = "labels")]
        public IDictionary<string, string> Labels { get; set; }

        [JsonProperty(PropertyName = "annotations")]
        public IDictionary<string, string> Annotations { get; set; }
    }
}
