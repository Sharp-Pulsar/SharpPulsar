using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes
{
    public static class Values
    {
        // Flag to control whether to run initialize job
        public static  bool Initialize { get; set; } = true;
        //Namespace to deploy pulsar
        public static string Namespace { get; set; } = "pulsar";
        public static string Cluster { get; set; } = "pulsar";
        public static string ReleaseName { get; set; } = "pulsar";
        public static string App { get; set; } = "pulsar";
        public static bool NamespaceCreate { get; set; } = false;
        //// Pulsar Metadata Prefix
        ////
        //// By default, pulsar stores all the metadata at root path.
        //// You can configure to have a prefix (e.g. "/my-pulsar-cluster").
        //// If you do so, all the pulsar and bookkeeper metadata will
        //// be stored under the provided path
        public static string MetadataPrefix { get; set; } = "";
        //// Persistence
        ////
        //// If persistence is enabled, components that have state will
        //// be deployed with PersistentVolumeClaims, otherwise, for test
        //// purposes, they will be deployed with emptyDir
        ////
        //// This is a global setting that is applied to all components.
        //// If you need to disable persistence for a component,
        //// you can set the `volume.persistence` setting to `false` for
        //// that component.
        public static Volume Volumes { get; set; } = new Volume();
        //// AntiAffinity
        ////
        //// Flag to enable and disable `AntiAffinity` for all components.
        //// This is a global setting that is applied to all components.
        //// If you need to disable AntiAffinity for a component, you can set
        //// the `affinity.anti_affinity` settings to `false` for that component.
        public static bool AntiAffinity { get; set; } = true;
        //// Components
        ////
        //// Control what components of Apache Pulsar to deploy for the cluster
        public static Components Components { get; set; } = new Components();
        //// Monitoring Components
        ////
        //// Control what components of the monitoring stack to deploy for the cluster
        public static Monitoring Monitoring { get; set; } = new Monitoring();
        //// Images
        ////
        //// Control what images to use for each component
        public static Images Images { get; set; } = new Images();
        //// TLS
        //// templates/tls-certs.yaml
        ////
        //// The chart is using cert-manager for provisioning TLS certs for
        //// brokers and proxies.
        
    }
    public  sealed class Volume
    {
        public bool Persistence { get; set; } = true;
        // configure the components to use local persistent volume
        // the local provisioner should be installed prior to enable local persistent volume
        public  bool LocalStorage { get; set; } = false;
    }
    public  sealed class Components
    {
        // zookeeper
        public bool Zookeeper { get; set; } = true;
        // bookkeeper
        public bool Bookkeeper { get; set; } = true;
        // bookkeeper - autorecovery
        public bool Autorecovery { get; set; } = true;
        // broker
        public  bool Broker { get; set; } = true;
        // functions
        public bool Functions { get; set; } = true;
        // proxy
        public bool proxy { get; set; } = true;
        // toolset
        public bool toolset { get; set; } = false;
        // pulsar manager
        public bool PulsarManager { get; set; } = false;
        // pulsar sql
        public bool SqlWorker { get; set; } = true;
        // kop
        public bool Kop { get; set; } = false;
        // pulsar detector
        public bool PulsarDetector { get; set; } = false;
    }
    public  sealed class Monitoring
    {
        // monitoring - prometheus
        public bool Prometheus { get; set; } = false;
        // monitoring - grafana
        public bool Grafana { get; set; } = false;
        // monitoring - node_exporter
        public bool NodeExporter { get; set; } = false;
        // alerting - alert-manager
        public bool AlertManager { get; set; } = false;
        // monitoring - loki
        public bool Loki { get; set; } = false;
        // monitoring - datadog
        public bool Datadog { get; set; } = false;
    }
    public  sealed class Images 
    {
        public Image ZooKeeper { get; set; } = new Image();
        public Image Bookie { get; set; } = new Image();
        public Image Presto { get; set; } = new Image();
        public Image Autorecovery { get; set; } = new Image();
        public Image Broker { get; set; } = new Image();
        public Image Proxy { get; set; } = new Image();
        public Image PulsarDetector { get; set; } = new Image();
        public Image Functions { get; set; } = new Image();
        public Image Prometheus { get; set; } = new Image 
        { 
            Repository = "prom/prometheus",
            Tag = "v2.17.2"
        };
        public Image AlertManager { get; set; } = new Image
        {
            Repository = "prom/alertmanager",
            Tag = "v0.20.0"
        };
        public Image Grafana { get; set; } = new Image
        {
            Repository = "streamnative/apache-pulsar-grafana-dashboard-k8s",
            Tag = "0.0.8"
        };
        public Image PulsarManager { get; set; } = new Image
        {
            Repository = "streamnative/pulsar-manager",
            Tag = "0.3.0"
        };
        public Image NodeExporter { get; set; } = new Image
        {
            Repository = "prom/node-exporter",
            Tag = "0.16.0"
        };
        public Image NginxIngressController { get; set; } = new Image
        {
            Repository = "quay.io/kubernetes-ingress-controller/nginx-ingress-controller",
            Tag = "0.26.2"
        };
    }
    public  sealed class Image
    {
        public string Repository { get; set; } = "apachepulsar/pulsar-all";
        public string Tag { get; set; } = "2.6.0";
        public string PullPolicy { get; set; } = "IfNotPresent";
        public bool HasCommand { get; set; } = false;
    }
}
