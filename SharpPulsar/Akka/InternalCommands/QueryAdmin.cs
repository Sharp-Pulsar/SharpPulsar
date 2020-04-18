using System;
using SharpPulsar.Akka.Admin;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class QueryAdmin
    {
        public QueryAdmin(AdminCommands command, object[] arguments, Action<object> handler, Action<Exception> exception)
        {
            Command = command;
            Arguments = arguments;
            Handler = handler;
            Exception = exception;
        }

        public AdminCommands Command { get; }
        public object[] Arguments { get; }
        public Action<object> Handler { get; }
        public Action<Exception> Exception { get; }
    }
}
