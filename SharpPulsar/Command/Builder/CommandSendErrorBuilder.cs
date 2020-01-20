using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandSendErrorBuilder
    {
        private readonly CommandSendError _error;
        public CommandSendErrorBuilder()
        {
            _error = new CommandSendError();
        }
        private CommandSendErrorBuilder(CommandSendError error)
        {
            _error = error;
        }
        public CommandSendErrorBuilder SetProducerId(long producerId)
        {
            _error.ProducerId = (ulong)producerId;
            return new CommandSendErrorBuilder(_error);
        }
        public CommandSendErrorBuilder SetSequenceId(long sequenceId)
        {
            _error.SequenceId = (ulong)sequenceId;
            return new CommandSendErrorBuilder(_error);
        }
        public CommandSendErrorBuilder SetError(ServerError error)
        {
            _error.Error = error;
            return new CommandSendErrorBuilder(_error);
        }
        public CommandSendErrorBuilder SetMessage(string errorMsg)
        {
			_error.Message = (errorMsg);
            return new CommandSendErrorBuilder(_error);
        }
        public CommandSendError Build()
        {
            return _error;
        }
    }
}
