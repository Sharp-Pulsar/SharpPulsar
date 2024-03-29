﻿using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Messages
{
    public readonly record struct GetOrCreateSchemaServerResponse
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
