using System;
using SharpPulsar.Function;

namespace SharpPulsar.Messages
{
    public sealed class Function
    {
        public Function(FunctionCommand command, object[] arguments, Action<object> handler, Action<Exception> exception, string brokerDestinationUrl, Action<string> log)
        {
            Command = command;
            Arguments = arguments;
            Handler = handler;
            Exception = exception;
            BrokerDestinationUrl = brokerDestinationUrl;
            Log = log;
        }

        public FunctionCommand Command { get; }
        public object[] Arguments { get; }
        public Action<object> Handler { get; }
        public Action<Exception> Exception { get; }
        public Action<string> Log { get; }
        public string BrokerDestinationUrl { get; }
    }
}
