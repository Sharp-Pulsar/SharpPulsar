using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages.Consumer
{
    public class SubscribeSuccess
    {
        public SubscribeSuccess(Schema schema, long requestId, bool hasSchema)
        {
            Schema = schema;
            RequestId = requestId;
            HasSchema = hasSchema;
        }

        public Schema Schema { get; }
        public long RequestId { get; }
        public bool HasSchema { get; }
    }
}
