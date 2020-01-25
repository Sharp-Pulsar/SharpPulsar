using SharpPulsar.Command.Builder;
using System;

namespace SharpPulsar.Protocol.Extension
{
    public static class MessageMetadataBuilderExtension
    {
        public static void SetDeliverAtTime(this MessageMetadataBuilder metadata, DateTimeOffset timestamp)
            => metadata.SetDeliveryTime(timestamp.ToUnixTimeMilliseconds());


        public static void SetEventTime(this MessageMetadataBuilder metadata, DateTimeOffset timestamp)
            => metadata.SetEventTime(timestamp.ToUnixTimeMilliseconds());
        
        public static void SetKey(this MessageMetadataBuilder metadata, string? key)
        {
            metadata.SetPartitionKey(key);
            metadata.SetPartitionKeyB64EncodedTo(false);
        }

        public static void SetKey(this MessageMetadataBuilder metadata, byte[]? key)
        {
            if (key is null)
                return;

            metadata.SetPartitionKey(Convert.ToBase64String(key));
            metadata.SetPartitionKeyB64EncodedTo(true);
        }
    }
}
