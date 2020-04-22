using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class Sql
    {
        public Sql(string query, Action<Dictionary<string, string>> handler, Action<Exception> exceptionHandler, string destinationServer, Action<string> log, bool includeMetadata = false)
        {
            Query = query;
            Handler = handler;
            ExceptionHandler = exceptionHandler;
            DestinationServer = destinationServer;
            Log = log;
            IncludeMetadata = includeMetadata;
        }

        public string Query { get; }
        public Action<Dictionary<string, string>> Handler { get; }
        public Action<Exception> ExceptionHandler { get; }
        public Action<string> Log { get; }
        public bool IncludeMetadata { get; }
        public string DestinationServer { get; }
    }
}
