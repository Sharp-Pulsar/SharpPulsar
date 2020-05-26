using System;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class Sql
    {
        public Sql(string query, Action<Exception> exceptionHandler, string server, Action<string> log)
        {
            Query = query;
            ExceptionHandler = exceptionHandler;
            Server = server;
            Log = log;
        }

        public string Query { get; }
        public Action<Exception> ExceptionHandler { get; }
        public Action<string> Log { get; }
        public string Server { get; }
    }
}
