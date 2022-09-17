namespace SharpPulsar.Admin.Model
{
    using System.Text.Json.Serialization;

    public  class ResourceUsage
    {
        /// <summary>
        /// </summary>
        [JsonPropertyName("usage")]
        public double? Usage { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("limit")]
        public double? Limit { get; set; }

    }
}
