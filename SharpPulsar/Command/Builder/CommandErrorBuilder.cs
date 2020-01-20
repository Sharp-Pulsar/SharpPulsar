using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandErrorBuilder
    {
        private readonly CommandError _error;
        public CommandErrorBuilder()
        {
            _error = new CommandError();
        }
        private CommandErrorBuilder(CommandError error)
        {
            _error = error;
        }
        public  CommandErrorBuilder SetRequestId(long requestId)
        {
            _error.RequestId = (ulong)requestId;
            return new CommandErrorBuilder(_error);
        }
        public  CommandErrorBuilder SetError(ServerError error)
        {
            _error.Error = error;
            return new CommandErrorBuilder(_error);
        }
        public  CommandErrorBuilder SetMessage(string message)
        {
            _error.Message = message;
            return new CommandErrorBuilder(_error);
        }
        public  CommandError Build()
        {
            return _error;
        }
    }
}
