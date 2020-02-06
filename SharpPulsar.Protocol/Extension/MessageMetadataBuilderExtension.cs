using SharpPulsar.Protocol.Proto;
using System;

namespace SharpPulsar.Protocol.Extension
{
    public static class MessageMetadataBuilderExtension
    {
        public static void SetDeliverAtTime(this MessageMetadata.Builder metadata, DateTimeOffset timestamp)
            => metadata.SetDeliverAtTime(timestamp.ToUnixTimeMilliseconds());


        public static void SetEventTime(this MessageMetadata.Builder metadata, DateTimeOffset timestamp)
            => metadata.SetEventTime(timestamp.ToUnixTimeMilliseconds());
        
        public static void SetKey(this MessageMetadata.Builder metadata, string? key)
        {
            metadata.SetPartitionKey(key);
            metadata.SetPartitionKeyB64Encoded(false);
        }

        public static void SetKey(this MessageMetadata.Builder metadata, byte[]? key)
        {
            if (key is null)
                return;

            metadata.SetPartitionKey(Convert.ToBase64String(key));
            metadata.SetPartitionKeyB64Encoded(true);
        }
    }
}
