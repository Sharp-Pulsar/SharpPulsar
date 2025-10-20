namespace SharpPulsar.Messages
{
    public record GetSchemaResponse
    {
        public CommandGetSchemaResponse Response { get; }
        public GetSchemaResponse(CommandGetSchemaResponse response)
        {
            Response = response;
        }
    }
    public record GetOrCreateSchemaResponse
    {
        public CommandGetOrCreateSchemaResponse Response { get; }
        public GetOrCreateSchemaResponse(CommandGetOrCreateSchemaResponse response)
        {
            Response = response;
        }
    }
    public record NullSchema { }
}
