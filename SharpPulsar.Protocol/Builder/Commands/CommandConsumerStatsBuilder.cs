using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandConsumerStatsBuilder
    {        
        private CommandConsumerStats _stats;
        public CommandConsumerStatsBuilder()
        {
            _stats = new CommandConsumerStats();
        }
        
        public CommandConsumerStatsBuilder SetConsumerId(long consumerId)
        {
            _stats.ConsumerId = (ulong)consumerId;
            return this;
        }
        public CommandConsumerStatsBuilder SetRequestId(long requestId)
        {
            _stats.RequestId = (ulong)requestId;
            return this;
        }
        public CommandConsumerStats Build()
        {
            return _stats;
        }
    }
}
