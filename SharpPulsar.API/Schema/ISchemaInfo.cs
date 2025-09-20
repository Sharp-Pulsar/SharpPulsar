using SharpPulsar.Shared;

namespace SharpPulsar.API.Schema
{
    public interface ISchemaInfo
    {
        string Name { get; set; }
        byte[] Schema { get; set; }
        string SchemaDefinition { get; }
        SchemaType Type { get; set; }

        IDictionary<string, string> Properties { get; set; }

        string ToString();
    }
}