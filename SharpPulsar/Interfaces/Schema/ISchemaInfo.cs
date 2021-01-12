using SharpPulsar.Shared;
using System.Collections.Generic;

namespace SharpPulsar.Interfaces.ISchema
{
    public interface ISchemaInfo
    {
        string Name { get; set; }
        sbyte[] Schema { get; set; }
        string SchemaDefinition { get; }
        SchemaType Type { get; set; }

        IDictionary<string, string> Properties { get; set; }

        string ToString();
    }
}