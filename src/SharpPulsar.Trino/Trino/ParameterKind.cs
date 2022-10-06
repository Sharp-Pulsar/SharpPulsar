using System.Text.Json.Serialization;

namespace SharpPulsar.Trino.Trino
{
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum ParameterKind
    {
        TYPE,
        NamedType,
        LONG,
        VARIABLE
    }
}
