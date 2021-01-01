using SharpPulsar.Shared;

namespace SharpPulsar.Pulsar.Api.Schema
{
    public interface ISchemaInfo
    {
        string Name { get; set; }
        sbyte[] Schema { get; set; }
        string SchemaDefinition { get; }
        SchemaType Type { get; set; }

        string ToString();
    }
}