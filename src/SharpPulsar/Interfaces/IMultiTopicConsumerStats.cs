
using System.Collections.Generic;
using SharpPulsar.Stats.Consumer.Api;

namespace SharpPulsar.Interfaces
{
    internal interface IMultiTopicConsumerStats
    {
        /// <summary>
        /// Multi-topic Consumer statistics recorded by client.
        /// 
        /// <para>All the stats are relative to the last recording period. The interval of the stats refreshes is configured with
        /// <seealso cref="ClientBuilder.statsInterval(long, java.util.concurrent.TimeUnit)"/> with a default of 1 minute.
        /// </para>
        /// </summary>
        public interface MultiTopicConsumerStats : IConsumerStats
        {

            /// <returns> stats for each partition if topic is partitioned topic </returns>
            IDictionary<string, IConsumerStats> GetPartitionStats();
        }

    }
}
