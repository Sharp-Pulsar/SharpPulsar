using System;
using SharpPulsar.Akka.Sql.Client;
using SharpPulsar.Presto;

namespace SharpPulsar.Akka.InternalCommands
{
    public sealed class Sql
    {
        public Sql(ClientOptions options, Action<Exception> exceptionHandler, Action<string> log)
        {
            ExceptionHandler = exceptionHandler;
            ClientOptions = options;
            Log = log;
        }

        public Action<Exception> ExceptionHandler { get; }
        public Action<string> Log { get; }
        public ClientOptions ClientOptions { get; }
    }
    internal sealed class SqlSession
    {
        public SqlSession(ClientSession session, ClientOptions options, Action<Exception> exceptionHandler, Action<string> log)
        {
            ExceptionHandler = exceptionHandler;
            ClientSession = session;
            Log = log;
            ClientOptions = options;
        }

        public Action<Exception> ExceptionHandler { get; }
        public Action<string> Log { get; }
        public ClientSession ClientSession { get; }
        public ClientOptions ClientOptions { get; }
    }
}
