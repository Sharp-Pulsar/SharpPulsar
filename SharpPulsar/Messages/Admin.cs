using System;
using SharpPulsar.Admin;

namespace SharpPulsar.Messages
{
    public sealed class Admin
    {
        public Admin(AdminCommands command, object[] arguments, Action<object> handler, Action<Exception> exception, string brokerDestinationUrl, Action<string> log)
        {
            Command = command;
            Arguments = arguments;
            Handler = handler;
            Exception = exception;
            BrokerDestinationUrl = brokerDestinationUrl;
            Log = log;
        }

        public AdminCommands Command { get; }
        public object[] Arguments { get; }
        public Action<object> Handler { get; }
        public Action<Exception> Exception { get; }
        public Action<string> Log { get; }
        public string BrokerDestinationUrl { get; }
    }
}
