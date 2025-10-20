using System;

namespace SharpPulsar.Messages
{
    public record ErrorMessage
    {
        public ErrorMessage(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
