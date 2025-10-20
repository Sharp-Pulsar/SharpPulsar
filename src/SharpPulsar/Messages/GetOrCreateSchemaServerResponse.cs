namespace SharpPulsar.Messages
{
    public record GetOrCreateSchemaServerResponse
    {
        public GetOrCreateSchemaServerResponse(long requestId, string errorMessage, ServerError errorCode, byte[] schemaVersion)
        {
            RequestId = requestId;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            SchemaVersion = schemaVersion;
        }

        public long RequestId { get;}
        public string ErrorMessage { get;}
        public ServerError ErrorCode { get; }
        public byte[] SchemaVersion { get; }
    }
}
