using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandSuccessBuilder
    {
        private static CommandSuccess _success;
        public CommandSuccessBuilder()
        {
            _success = new CommandSuccess();
        }
        private CommandSuccessBuilder(CommandSuccess success)
        {
            _success = success;
        }
        public static CommandSuccessBuilder SetRequestId(long requestId)
        {
            _success.RequestId = (ulong)requestId;
            return new CommandSuccessBuilder(_success);
        }
        public static CommandSuccess Build()
        {
            return _success;
        }
    }
}
