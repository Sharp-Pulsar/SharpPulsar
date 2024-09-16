
using SharpPulsar.Configuration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SharpPulsar.Interfaces;
using SharpPulsar.Stats.Consumer;
using SharpPulsar.Stats.Consumer.Api;
using Akka.Actor;

namespace SharpPulsar
{
    internal class MultiTopicConsumerStatsRecorder<T> : ConsumerStatsRecorder<T> , IMultiTopicConsumerStats

    {
        private IDictionary<string, IConsumerStats> partitionStats = new ConcurrentDictionary<string, IConsumerStats>();

        private PartitionedTopicProducerStatsRecorder deadLetterStats = new PartitionedTopicProducerStatsRecorder();
        private PartitionedTopicProducerStatsRecorder retryLetterStats = new PartitionedTopicProducerStatsRecorder();

        public MultiTopicConsumerStatsRecorder() : base()
        {
        }
        
        public MultiTopicConsumerStatsRecorder(Consumer<T> consumer) //: base(consumer)
        {
        }

        public MultiTopicConsumerStatsRecorder(IActorRef pulsarClient, ConsumerConfigurationData<T> conf, Consumer<T> consumer)// : base(pulsarClient, conf, consumer)
        {
        }

        public virtual void UpdateCumulativeStats(string partition, IConsumerStats stats)
        {
            base.UpdateCumulativeStats(stats);
            partitionStats[partition] = stats;
        }

        public IDictionary<string, IConsumerStats> PartitionStats
        {
            get
            {
                return partitionStats;
            }
        }

        public  IProducerStats DeadLetterProducerStats
        {
            get
            {
                deadLetterStats.Reset();
                //partitionStats.ForEach((partition, consumerStats) => deadLetterStats.updateCumulativeStats(partition, consumerStats.getDeadLetterProducerStats()));
                return deadLetterStats;
            }
        }

        public IProducerStats RetryLetterProducerStats
        {
            get
            {
                retryLetterStats.Reset();
                //partitionStats.forEach((partition, consumerStats) => retryLetterStats.updateCumulativeStats(partition, consumerStats.getRetryLetterProducerStats()));
                return retryLetterStats;
            }
        }


    }
}
