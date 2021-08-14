using System;

namespace SharpPulsar.Sql.Message
{
    internal sealed class AskResponse
    {
        public bool Failed { get; }
        public object Data { get; }
        public Exception Exception { get; }
        internal AskResponse()
        {
            Failed = false;
        }
        internal AskResponse(object data)
        {
            Failed = false;
            Data = data;
        }

        internal AskResponse(Exception exception)
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
