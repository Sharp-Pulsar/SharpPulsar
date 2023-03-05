using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages
{
    public readonly record struct GetSchemaResponse
    {
        public CommandGetSchemaResponse Response { get; }
        public GetSchemaResponse(CommandGetSchemaResponse response)
        {
            Response = response;
        }
    }
    public readonly record struct GetOrCreateSchemaResponse
    {
        public CommandGetOrCreateSchemaResponse Response { get; }
        public GetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse response)
        {
            Response = response;
        }
    }
    public readonly record struct NullSchema { }
}
