using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Api;
using SharpPulsar.Impl;

namespace SharpPulsar.Extension
{
    public static class ListPartition
    {
        public static IEnumerable<IList<MessageId>> PartitionMessageId(this IList<IMessageId> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = (MessageId)x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
        public static List<List<MessageId>> PartitionMessageId(this ISet<IMessageId> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = (MessageId)x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
