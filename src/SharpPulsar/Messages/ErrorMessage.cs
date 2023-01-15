using System;

namespace SharpPulsar.Messages
{
    public readonly record struct ErrorMessage
    {
        public ErrorMessage(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
