
using System.Text.Json.Serialization;

namespace SharpPulsar.Admin.Model
{
    public class NamespaceBundleStats
    {
        /// <summary>
        /// </summary>
        [JsonPropertyName("msgRateIn")]
        public double? MsgRateIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("msgThroughputIn")]
        public double? MsgThroughputIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("msgRateOut")]
        public double? MsgRateOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("msgThroughputOut")]
        public double? MsgThroughputOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("consumerCount")]
        public int? ConsumerCount { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("producerCount")]
        public int? ProducerCount { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("topics")]
        public long? Topics { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("cacheSize")]
        public long? CacheSize { get; set; }
    }
}
