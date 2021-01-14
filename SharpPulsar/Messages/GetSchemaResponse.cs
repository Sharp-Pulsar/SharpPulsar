using System.Collections.Immutable;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages
{
    public sealed class GetSchemaResponse
    {
        public CommandGetSchemaResponse Response { get; }
        public GetSchemaResponse(CommandGetSchemaResponse response)
        {
            Response = response;
        }
    }
    public sealed class GetOrCreateSchemaResponse
    {
        public CommandGetOrCreateSchemaResponse Response { get; }
        public GetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse response)
        {
            Response = response;
        }
    }
    public sealed class NullSchema { }
}
