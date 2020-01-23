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
        
        public CommandSendErrorBuilder SetProducerId(long producerId)
        {
            _error.ProducerId = (ulong)producerId;
            return this;
        }
        public CommandSendErrorBuilder SetSequenceId(long sequenceId)
        {
            _error.SequenceId = (ulong)sequenceId;
            return this;
        }
        public CommandSendErrorBuilder SetError(ServerError error)
        {
            _error.Error = error;
            return this;
        }
        public CommandSendErrorBuilder SetMessage(string errorMsg)
        {
			_error.Message = (errorMsg);
            return this;
        }
        public CommandSendError Build()
        {
            return _error;
        }
    }
}
