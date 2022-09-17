
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SharpPulsar.Admin.Model
{
    public class ResourceDescription
    {
        /// <summary>
        /// </summary>
        [JsonPropertyName("resourceUsage")]
        public IDictionary<string, ResourceUsage> ResourceUsage { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("usagePct")]
        public int? UsagePct { get; set; }

    }
}
