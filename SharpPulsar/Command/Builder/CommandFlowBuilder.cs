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
        private CommandFlowBuilder(CommandFlow ack)
        {
            _flow = ack;
        }
        public CommandFlowBuilder SetConsumerId(long consumerId)
        {
            _flow.ConsumerId = (ulong)consumerId;
            return new CommandFlowBuilder(_flow);
        }
        public CommandFlowBuilder SetMessagePermits(int messagePermits)
        {
            _flow.messagePermits = (uint)messagePermits;
            return new CommandFlowBuilder(_flow);
        }
        public CommandFlow Build()
        {
            return _flow;
        }
    }
}
