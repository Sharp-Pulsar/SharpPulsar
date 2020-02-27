using System;

namespace SharpPulsar.Akka.InternalCommands
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
