

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SharpPulsar.Admin.Model
{
    public class LoadReport
    {

        /// <summary>
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("brokerVersionString")]
        public string BrokerVersionString { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("webServiceUrl")]
        public string WebServiceUrl { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("webServiceUrlTls")]
        public string WebServiceUrlTls { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("pulsarServiceUrl")]
        public string PulsarServiceUrl { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("pulsarServiceUrlTls")]
        public string PulsarServiceUrlTls { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("persistentTopicsEnabled")]
        public bool? PersistentTopicsEnabled { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("nonPersistentTopicsEnabled")]
        public bool? NonPersistentTopicsEnabled { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("timestamp")]
        public long? Timestamp { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("msgRateIn")]
        public double? MsgRateIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("msgRateOut")]
        public double? MsgRateOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("numTopics")]
        public int? NumTopics { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("numConsumers")]
        public int? NumConsumers { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("numProducers")]
        public int? NumProducers { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("numBundles")]
        public int? NumBundles { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("protocols")]
        public IDictionary<string, string> Protocols { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("systemResourceUsage")]
        public SystemResourceUsage SystemResourceUsage { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("bundleStats")]
        public IDictionary<string, NamespaceBundleStats> BundleStats { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("bundleGains")]
        public IList<string> BundleGains { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("bundleLosses")]
        public IList<string> BundleLosses { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("allocatedCPU")]
        public double? AllocatedCPU { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("allocatedMemory")]
        public double? AllocatedMemory { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("allocatedBandwidthIn")]
        public double? AllocatedBandwidthIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("allocatedBandwidthOut")]
        public double? AllocatedBandwidthOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("allocatedMsgRateIn")]
        public double? AllocatedMsgRateIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("allocatedMsgRateOut")]
        public double? AllocatedMsgRateOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("preAllocatedCPU")]
        public double? PreAllocatedCPU { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("preAllocatedMemory")]
        public double? PreAllocatedMemory { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("preAllocatedBandwidthIn")]
        public double? PreAllocatedBandwidthIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("preAllocatedBandwidthOut")]
        public double? PreAllocatedBandwidthOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("preAllocatedMsgRateIn")]
        public double? PreAllocatedMsgRateIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("preAllocatedMsgRateOut")]
        public double? PreAllocatedMsgRateOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("msgThroughputIn")]
        public double? MsgThroughputIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("msgThroughputOut")]
        public double? MsgThroughputOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("bandwidthIn")]
        public ResourceUsage BandwidthIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("directMemory")]
        public ResourceUsage DirectMemory { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("lastUpdate")]
        public long? LastUpdate { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("bandwidthOut")]
        public ResourceUsage BandwidthOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("underLoaded")]
        public bool? UnderLoaded { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("overLoaded")]
        public bool? OverLoaded { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("loadReportType")]
        public string LoadReportType { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("memory")]
        public ResourceUsage Memory { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("cpu")]
        public ResourceUsage Cpu { get; set; }

        
    }
}
