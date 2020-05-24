using System;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class Sql
    {
        public Sql(string query, Action<Exception> exceptionHandler, string destinationServer, Action<string> log)
        {
            Query = query;
            ExceptionHandler = exceptionHandler;
            DestinationServer = destinationServer;
            Log = log;
        }

        public string Query { get; }
        public Action<Exception> ExceptionHandler { get; }
        public Action<string> Log { get; }
        public string DestinationServer { get; }
    }
}
