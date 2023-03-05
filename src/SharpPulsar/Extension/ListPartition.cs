using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SharpPulsar.Messages;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Extension
{
    internal static class ListPartition
    {
        public static IEnumerable<IList<MessageId>> PartitionMessageId(this IList<IMessageId> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = (MessageId)x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
        public static IEnumerable<IList<TopicMessageId>> Collect(this IEnumerable<TopicMessageId> source)
        {
            return source
                .GroupBy(x => x.TopicPartitionName)
                .Select(x => x.Select(v => v).ToList())
                .ToList();
        }
        public static List<List<MessageId>> PartitionMessageId(this ISet<IMessageId> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = (MessageId)x })
                .GroupBy(x => chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
        public static List<List<Unacked>> PartitionMessageId(this ImmutableHashSet<Unacked> source)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Value.PartitionIndex)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
        public static ISet<IMessageId> RemoveMessageId(this LinkedList<HashSet<IMessageId>> source, int chunkSize)
        {
            var first = source.First;
            source.RemoveFirst();
            return first.Value;
        }
    }
}
