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
        private CommandConsumerStatsBuilder(CommandConsumerStats stats)
        {
            _stats = stats;
        }
        public CommandConsumerStatsBuilder SetConsumerId(long consumerId)
        {
            _stats.ConsumerId = (ulong)consumerId;
            return new CommandConsumerStatsBuilder(_stats);
        }
        public CommandConsumerStatsBuilder SetRequestId(long requestId)
        {
            _stats.RequestId = (ulong)requestId;
            return new CommandConsumerStatsBuilder(_stats);
        }
        public CommandConsumerStats Build()
        {
            return _stats;
        }
    }
}
