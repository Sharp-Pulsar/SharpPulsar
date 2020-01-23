using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandSuccessBuilder
    {
        private readonly CommandSuccess _success;
        public CommandSuccessBuilder()
        {
            _success = new CommandSuccess();
        }
        
        public  CommandSuccessBuilder SetRequestId(long requestId)
        {
            _success.RequestId = (ulong)requestId;
            return this;
        }
        public  CommandSuccess Build()
        {
            return _success;
        }
    }
}
