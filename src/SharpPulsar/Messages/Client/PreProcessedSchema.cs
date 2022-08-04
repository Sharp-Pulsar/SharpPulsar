using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Client
{
    public sealed class PreProcessedSchema<T>
    {
        public ISchema<T> Schema { get; }
        public PreProcessedSchema(ISchema<T> schema)
        {
            Schema = schema;
        }
    }
}
