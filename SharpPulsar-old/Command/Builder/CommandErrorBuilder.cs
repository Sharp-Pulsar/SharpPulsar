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
        
        public  CommandErrorBuilder SetRequestId(long requestId)
        {
            _error.RequestId = (ulong)requestId;
            return this;
        }
        public  CommandErrorBuilder SetError(ServerError error)
        {
            _error.Error = error;
            return this;
        }
        public  CommandErrorBuilder SetMessage(string message)
        {
            _error.Message = message;
            return this;
        }
        public  CommandError Build()
        {
            return _error;
        }
    }
}
