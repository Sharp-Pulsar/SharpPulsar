using SharpPulsar.Api;

namespace SharpPulsar.Pulsar.Schema
{
    public static class SchemaFields
    {
        public static readonly ISchema Bytes = DefaultImplementation.NewBytesSchema();
    }
}
