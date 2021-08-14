using System;

namespace SharpPulsar.Sql.Message
{
    public sealed class AskResponse
    {
        public bool Failed { get; }
        public object Data { get; }
        public Exception Exception { get; }
        public AskResponse()
        {
            Failed = false;
        }
        public AskResponse(object data)
        {
            Failed = false;
            Data = data;
        }

        public AskResponse(Exception exception)
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
