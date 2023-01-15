
using SharpPulsar.Exceptions;

namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct RedeliverUnacknowledgedMessages
    {
        public static RedeliverUnacknowledgedMessages Instance = new RedeliverUnacknowledgedMessages();
    }
    
    public readonly record struct AskResponse
    {
        public bool Failed { get;  }
        public object Data { get; }
        public PulsarClientException Exception { get;  }
        public AskResponse() : this(null)
        {
            Failed= false;
        }
        public AskResponse(object data) => (Failed, Data, Exception) = (false, data, null);

        public AskResponse(PulsarClientException exception) => (Failed, Data, Exception) = (true, null, exception);
        public T ConvertTo<T>()
        {
            return (T)Data;
        }
    }
}
