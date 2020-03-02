using SharpPulsar.Api;

namespace SharpPulsar.Impl.Schema
{
    public static class SchemaFields
    {
        public static readonly ISchema Bytes = DefaultImplementation.NewBytesSchema();
    }
}
