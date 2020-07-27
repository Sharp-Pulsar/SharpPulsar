using System.Text.Json.Serialization;

namespace SharpPulsar.Presto.Facebook.Type
{
    [JsonConverter(typeof(JsonStringEnumConverter))] 

    public enum ParameterKind
    {
        LONG_LITERAL,
        TYPE_SIGNATURE,
        NAMED_TYPE_SIGNATURE
    }
}
