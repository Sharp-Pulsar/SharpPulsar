using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class QueryData
    {
        public QueryData(string query, Action<Dictionary<string, object>> handler, Action<Exception> exceptionHandler, string destinationServer, bool includeMetadata = false)
        {
            Query = query;
            Handler = handler;
            ExceptionHandler = exceptionHandler;
            DestinationServer = destinationServer;
            IncludeMetadata = includeMetadata;
        }

        public string Query { get; }
        public Action<Dictionary<string, object>> Handler { get; }
        public Action<Exception> ExceptionHandler { get; }
        public bool IncludeMetadata { get; }
        public string DestinationServer { get; }
    }
}
