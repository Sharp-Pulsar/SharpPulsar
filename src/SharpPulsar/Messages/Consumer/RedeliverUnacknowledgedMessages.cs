namespace SharpPulsar.Messages.Consumer
{
    public record RedeliverUnacknowledgedMessages
    {
        public static RedeliverUnacknowledgedMessages Instance = new RedeliverUnacknowledgedMessages();
    }
    
    public record AskResponse
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
        public (T1, T2) ConvertTo<T1,T2>()
        {
            try
            {
                return ((T1)Data, default);
            }
            catch
            {

                return (default, (T2)Data);
            }
        }
    }
}
