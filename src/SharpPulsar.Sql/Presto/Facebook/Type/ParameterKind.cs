using System.Text.Json.Serialization;

namespace SharpPulsar.Sql.Presto.Facebook.Type
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
