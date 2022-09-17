

using System.Text.Json.Serialization;

namespace SharpPulsar.Admin.Model
{
    public class InternalConfigurationData
    {
        /// <summary>
        /// </summary>
        [JsonPropertyName("zookeeperServers")]
        public string ZookeeperServers { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("configurationStoreServers")]
        public string ConfigurationStoreServers { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("ledgersRootPath")]
        public string LedgersRootPath { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("bookkeeperMetadataServiceUri")]
        public string BookkeeperMetadataServiceUri { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("stateStorageServiceUrl")]
        public string StateStorageServiceUrl { get; set; }
    }
}
