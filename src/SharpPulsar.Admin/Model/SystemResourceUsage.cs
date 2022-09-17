
using System.Text.Json.Serialization;

namespace SharpPulsar.Admin.Model
{
    public class SystemResourceUsage
    {

        /// <summary>
        /// </summary>
        [JsonPropertyName("bandwidthIn")]
        public ResourceUsage BandwidthIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("bandwidthOut")]
        public ResourceUsage BandwidthOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("cpu")]
        public ResourceUsage Cpu { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("memory")]
        public ResourceUsage Memory { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("directMemory")]
        public ResourceUsage DirectMemory { get; set; }

    }
}
