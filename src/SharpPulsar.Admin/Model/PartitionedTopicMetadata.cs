
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SharpPulsar.Admin.Model
{
    public class PartitionedTopicMetadata
    {
        /// <summary>
        /// </summary>
        [JsonPropertyName("partitions")]
        public int? Partitions { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("properties")]
        public IDictionary<string, string> Properties { get; set; }

    }
}
