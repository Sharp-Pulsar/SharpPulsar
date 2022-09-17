using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SharpPulsar.Admin.Admin.Models;

namespace SharpPulsar.Admin.Model
{
    public class ResourceUnit
    {
        /// <summary>
        /// </summary>
        [JsonPropertyName("resourceId")]
        public string ResourceId { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("availableResource")]
        public ResourceDescription AvailableResource { get; set; }
    }
}
