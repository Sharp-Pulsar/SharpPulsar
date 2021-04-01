using System.Text.Json.Serialization;

namespace SharpPulsar.Presto.Facebook.Type
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
