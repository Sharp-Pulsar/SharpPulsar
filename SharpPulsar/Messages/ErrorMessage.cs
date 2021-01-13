using System;

namespace SharpPulsar.Messages
{
    public class ErrorMessage
    {
        public ErrorMessage(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
