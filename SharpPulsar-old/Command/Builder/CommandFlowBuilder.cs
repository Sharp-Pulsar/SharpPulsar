using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandFlowBuilder
    {
        private readonly CommandFlow _flow;
        public CommandFlowBuilder()
        {
            _flow = new CommandFlow();
        }
        
        public CommandFlowBuilder SetConsumerId(long consumerId)
        {
            _flow.ConsumerId = (ulong)consumerId;
            return this;
        }
        public CommandFlowBuilder SetMessagePermits(int messagePermits)
        {
            _flow.messagePermits = (uint)messagePermits;
            return this;
        }
        public CommandFlow Build()
        {
            return _flow;
        }
    }
}
