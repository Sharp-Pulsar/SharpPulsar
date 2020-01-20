using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandErrorBuilder
    {
        private static CommandError _error;
        public CommandErrorBuilder()
        {
            _error = new CommandError();
        }
        private CommandErrorBuilder(CommandError error)
        {
            _error = error;
        }
        public static CommandErrorBuilder SetRequestId(long requestId)
        {
            _error.RequestId = (ulong)requestId;
            return new CommandErrorBuilder(_error);
        }
        public static CommandErrorBuilder SetError(ServerError error)
        {
            _error.Error = error;
            return new CommandErrorBuilder(_error);
        }
        public static CommandErrorBuilder SetMessage(string message)
        {
            _error.Message = message;
            return new CommandErrorBuilder(_error);
        }
        public static CommandError Build()
        {
            return _error;
        }
    }
}
