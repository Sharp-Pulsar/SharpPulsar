// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace PulsarAdmin.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class LoadReport
    {
        /// <summary>
        /// Initializes a new instance of the LoadReport class.
        /// </summary>
        public LoadReport()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the LoadReport class.
        /// </summary>
        public LoadReport(string name = default(string), string brokerVersionString = default(string), string webServiceUrl = default(string), string webServiceUrlTls = default(string), string pulsarServiceUrl = default(string), string pulsarServiceUrlTls = default(string), bool? persistentTopicsEnabled = default(bool?), bool? nonPersistentTopicsEnabled = default(bool?), long? timestamp = default(long?), double? msgRateIn = default(double?), double? msgRateOut = default(double?), int? numTopics = default(int?), int? numConsumers = default(int?), int? numProducers = default(int?), int? numBundles = default(int?), IDictionary<string, string> protocols = default(IDictionary<string, string>), SystemResourceUsage systemResourceUsage = default(SystemResourceUsage), IDictionary<string, NamespaceBundleStats> bundleStats = default(IDictionary<string, NamespaceBundleStats>), IList<string> bundleGains = default(IList<string>), IList<string> bundleLosses = default(IList<string>), double? allocatedCPU = default(double?), double? allocatedMemory = default(double?), double? allocatedBandwidthIn = default(double?), double? allocatedBandwidthOut = default(double?), double? allocatedMsgRateIn = default(double?), double? allocatedMsgRateOut = default(double?), double? preAllocatedCPU = default(double?), double? preAllocatedMemory = default(double?), double? preAllocatedBandwidthIn = default(double?), double? preAllocatedBandwidthOut = default(double?), double? preAllocatedMsgRateIn = default(double?), double? preAllocatedMsgRateOut = default(double?), ResourceUsage cpu = default(ResourceUsage), ResourceUsage memory = default(ResourceUsage), ResourceUsage directMemory = default(ResourceUsage), ResourceUsage bandwidthIn = default(ResourceUsage), ResourceUsage bandwidthOut = default(ResourceUsage), long? lastUpdate = default(long?), double? msgThroughputIn = default(double?), bool? underLoaded = default(bool?), bool? overLoaded = default(bool?), string loadReportType = default(string), double? msgThroughputOut = default(double?))
        {
            Name = name;
            BrokerVersionString = brokerVersionString;
            WebServiceUrl = webServiceUrl;
            WebServiceUrlTls = webServiceUrlTls;
            PulsarServiceUrl = pulsarServiceUrl;
            PulsarServiceUrlTls = pulsarServiceUrlTls;
            PersistentTopicsEnabled = persistentTopicsEnabled;
            NonPersistentTopicsEnabled = nonPersistentTopicsEnabled;
            Timestamp = timestamp;
            MsgRateIn = msgRateIn;
            MsgRateOut = msgRateOut;
            NumTopics = numTopics;
            NumConsumers = numConsumers;
            NumProducers = numProducers;
            NumBundles = numBundles;
            Protocols = protocols;
            SystemResourceUsage = systemResourceUsage;
            BundleStats = bundleStats;
            BundleGains = bundleGains;
            BundleLosses = bundleLosses;
            AllocatedCPU = allocatedCPU;
            AllocatedMemory = allocatedMemory;
            AllocatedBandwidthIn = allocatedBandwidthIn;
            AllocatedBandwidthOut = allocatedBandwidthOut;
            AllocatedMsgRateIn = allocatedMsgRateIn;
            AllocatedMsgRateOut = allocatedMsgRateOut;
            PreAllocatedCPU = preAllocatedCPU;
            PreAllocatedMemory = preAllocatedMemory;
            PreAllocatedBandwidthIn = preAllocatedBandwidthIn;
            PreAllocatedBandwidthOut = preAllocatedBandwidthOut;
            PreAllocatedMsgRateIn = preAllocatedMsgRateIn;
            PreAllocatedMsgRateOut = preAllocatedMsgRateOut;
            Cpu = cpu;
            Memory = memory;
            DirectMemory = directMemory;
            BandwidthIn = bandwidthIn;
            BandwidthOut = bandwidthOut;
            LastUpdate = lastUpdate;
            MsgThroughputIn = msgThroughputIn;
            UnderLoaded = underLoaded;
            OverLoaded = overLoaded;
            LoadReportType = loadReportType;
            MsgThroughputOut = msgThroughputOut;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "brokerVersionString")]
        public string BrokerVersionString { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "webServiceUrl")]
        public string WebServiceUrl { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "webServiceUrlTls")]
        public string WebServiceUrlTls { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "pulsarServiceUrl")]
        public string PulsarServiceUrl { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "pulsarServiceUrlTls")]
        public string PulsarServiceUrlTls { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "persistentTopicsEnabled")]
        public bool? PersistentTopicsEnabled { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "nonPersistentTopicsEnabled")]
        public bool? NonPersistentTopicsEnabled { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        public long? Timestamp { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "msgRateIn")]
        public double? MsgRateIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "msgRateOut")]
        public double? MsgRateOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "numTopics")]
        public int? NumTopics { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "numConsumers")]
        public int? NumConsumers { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "numProducers")]
        public int? NumProducers { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "numBundles")]
        public int? NumBundles { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "protocols")]
        public IDictionary<string, string> Protocols { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "systemResourceUsage")]
        public SystemResourceUsage SystemResourceUsage { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "bundleStats")]
        public IDictionary<string, NamespaceBundleStats> BundleStats { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "bundleGains")]
        public IList<string> BundleGains { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "bundleLosses")]
        public IList<string> BundleLosses { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "allocatedCPU")]
        public double? AllocatedCPU { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "allocatedMemory")]
        public double? AllocatedMemory { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "allocatedBandwidthIn")]
        public double? AllocatedBandwidthIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "allocatedBandwidthOut")]
        public double? AllocatedBandwidthOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "allocatedMsgRateIn")]
        public double? AllocatedMsgRateIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "allocatedMsgRateOut")]
        public double? AllocatedMsgRateOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "preAllocatedCPU")]
        public double? PreAllocatedCPU { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "preAllocatedMemory")]
        public double? PreAllocatedMemory { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "preAllocatedBandwidthIn")]
        public double? PreAllocatedBandwidthIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "preAllocatedBandwidthOut")]
        public double? PreAllocatedBandwidthOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "preAllocatedMsgRateIn")]
        public double? PreAllocatedMsgRateIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "preAllocatedMsgRateOut")]
        public double? PreAllocatedMsgRateOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "cpu")]
        public ResourceUsage Cpu { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "memory")]
        public ResourceUsage Memory { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "directMemory")]
        public ResourceUsage DirectMemory { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "bandwidthIn")]
        public ResourceUsage BandwidthIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "bandwidthOut")]
        public ResourceUsage BandwidthOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "lastUpdate")]
        public long? LastUpdate { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "msgThroughputIn")]
        public double? MsgThroughputIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "underLoaded")]
        public bool? UnderLoaded { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "overLoaded")]
        public bool? OverLoaded { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "loadReportType")]
        public string LoadReportType { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "msgThroughputOut")]
        public double? MsgThroughputOut { get; set; }

    }
}