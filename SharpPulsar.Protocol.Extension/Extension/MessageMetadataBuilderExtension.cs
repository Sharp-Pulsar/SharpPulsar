using SharpPulsar.Protocol.Proto;
using System;

namespace SharpPulsar.Protocol.Extension
{
    public static class MessageMetadataBuilderExtension
    {
        public static void SetDeliverAtTime(this MessageMetadata metadata, DateTimeOffset timestamp)
            => metadata.DeliverAtTime = timestamp.ToUnixTimeMilliseconds();


        public static void SetEventTime(this MessageMetadata metadata, DateTimeOffset timestamp)
            => metadata.EventTime = (ulong)timestamp.ToUnixTimeMilliseconds();
        
        public static void SetKey(this MessageMetadata metadata, string key)
        {
            metadata.PartitionKey = key;
            metadata.PartitionKeyB64Encoded = false;
        }

        public static void SetKey(this MessageMetadata metadata, byte[] key)
        {
            if (key is null)
                return;

            metadata.PartitionKey = Convert.ToBase64String(key);
            metadata.PartitionKeyB64Encoded = true;
        }
    }
}
