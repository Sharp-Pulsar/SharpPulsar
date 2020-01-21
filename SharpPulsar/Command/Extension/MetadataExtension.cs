using SharpPulsar.Common.PulsarApi;
using System;

namespace SharpPulsar.Command.Extension
{
    public static class MetadataExtension
    {
        public static DateTimeOffset GetDeliverAtTimeAsDateTimeOffset(this MessageMetadata metadata)
            => DateTimeOffset.FromUnixTimeMilliseconds(metadata.DeliverAtTime);

        public static DateTimeOffset GetEventTimeAsDateTimeOffset(this MessageMetadata metadata)
            => DateTimeOffset.FromUnixTimeMilliseconds((long)metadata.EventTime);

        public static byte[]? GetKeyAsBytes(this MessageMetadata metadata)
            => metadata.PartitionKeyB64Encoded ? Convert.FromBase64String(metadata.PartitionKey) : null;
    }
}
