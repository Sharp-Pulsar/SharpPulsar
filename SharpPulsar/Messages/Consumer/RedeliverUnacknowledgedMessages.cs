using SharpPulsar.Exceptions;

namespace SharpPulsar.Messages.Consumer
{
    public sealed class RedeliverUnacknowledgedMessages
    {
        public static RedeliverUnacknowledgedMessages Instance = new RedeliverUnacknowledgedMessages();
    } 

    public sealed class AskResponse
    {
        public bool Failed { get; }
        private object _data { get; }
        public PulsarClientException Exception { get; }
        public AskResponse()
        {
            Failed = false;
        }
        public AskResponse(object data)
        {
            Failed = false;
            _data = data;
        }

        public AskResponse(PulsarClientException exception)
        {
            Failed = true;
            Exception = exception;
        }
        public T GetData<T>()
        {
            return (T)_data;
        }
    }
}
