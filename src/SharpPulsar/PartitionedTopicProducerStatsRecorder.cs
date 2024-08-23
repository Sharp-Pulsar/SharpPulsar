
using System.Collections.Concurrent;
using System.Collections.Generic;
using App.Metrics.Concurrency;
using SharpPulsar.Interfaces;
using SharpPulsar.Stats.Producer;

namespace SharpPulsar
{

    internal class PartitionedTopicProducerStatsRecorder : ProducerStatsRecorder, IPartitionedTopicProducerStats
    {
        private IDictionary<string, IProducerStats> _partitionStats;
        private AtomicDouble _sendMsgsRateAggregate = new AtomicDouble();
        private AtomicDouble _sendBytesRateAggregate = new AtomicDouble();
        private int _partitions = 0;

        public PartitionedTopicProducerStatsRecorder(): base()
        {
            _partitionStats = new ConcurrentDictionary<string, IProducerStats>();
        }

        public new void Reset()
        {
            base.Reset();
            _partitions = 0;
        }
        
        internal virtual void UpdateCumulativeStats(string partition, IProducerStats stats)
        {
            
            base.UpdateCumulativeStats(stats);
            if (stats == null)
            {
                return;
            }
            _partitionStats[partition] = stats;
            // update rates_sendMsgsRateAggregate.Count() + 1] = 
            _sendMsgsRateAggregate.Add(stats.SendMsgsRate);
            _sendBytesRateAggregate.Add(stats.SendBytesRate);
            _partitions++;
        }

        IDictionary<string, IProducerStats> IPartitionedTopicProducerStats.GetPartitionStats() 
        {
            return _partitionStats;
        }

        public new double SendMsgsRate => (double)_sendMsgsRateAggregate.GetValue() / _partitions;

        public new double SendBytesRate => (double)_sendBytesRateAggregate.GetValue() / _partitions;


    }
}
