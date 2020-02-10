using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpPulsar.Api;
using SharpPulsar.Impl;

namespace SharpPulsar.Extension
{
    public static class ListPartition
    {
        public static IEnumerable<IList<MessageIdImpl>> PartitionMessageId(this IList<IMessageId> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = (MessageIdImpl)x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
        public static List<List<MessageIdImpl>> PartitionMessageId(this ISet<IMessageId> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = (MessageIdImpl)x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
