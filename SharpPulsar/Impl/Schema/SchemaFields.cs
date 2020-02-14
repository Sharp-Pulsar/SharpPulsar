using SharpPulsar.Api;

namespace SharpPulsar.Impl.Schema
{
    public static class SchemaFields
    {
        public static readonly ISchema<sbyte[]> Bytes = DefaultImplementation.NewBytesSchema();
        public static readonly ISchema<string> String = DefaultImplementation.NewStringSchema();
    }
}
