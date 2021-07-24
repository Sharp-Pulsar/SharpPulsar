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
        public object Data { get; }
        public PulsarClientException Exception { get; }
        public AskResponse()
        {
            Failed = false;
        }
        public AskResponse(object data)
        {
            Failed = false;
            Data = data;
        }

        public AskResponse(PulsarClientException exception)
        {
            if (exception == null)
                Failed = false;
            else
                Failed = true;

            Exception = exception;
        }
        public T ConvertTo<T>()
        {
            return (T)Data;
        }
    }
}
