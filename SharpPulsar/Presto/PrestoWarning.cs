using System.Text.Json.Serialization;

namespace SharpPulsar.Presto
{
    public class PrestoWarning
    {
        [JsonPropertyName("warningCode")]
        public WarningCode WarningCode { get; set; }
        [JsonPropertyName("message")]
        public  string Message { get; set; }
    }
}
