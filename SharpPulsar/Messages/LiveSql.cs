
using System;
using SharpPulsar.Presto;
using SharpPulsar.Sql;
using SharpPulsar.Sql.Client;

namespace SharpPulsar.Messages
{
    public class LiveSqlQuery : ISqlQuery
    {
        public LiveSqlQuery(ClientOptions options, int frequency, DateTime startAtPublishTime, string topic, Action<string> log, Action<Exception> exceptionHandler)
        {
            ClientOptions = options;
            Frequency = frequency;
            StartAtPublishTime = startAtPublishTime;
            Topic = topic;
            Log = log;
            ExceptionHandler = exceptionHandler;
        }
        public Action<Exception> ExceptionHandler { get; }
        public Action<string> Log { get; }
        /// <summary>
        /// Frequency in Milliseconds
        /// </summary>
        public int Frequency { get; }
        /// <summary>
        /// Represents publish time
        /// </summary>
        public DateTime StartAtPublishTime { get; }
        public string Topic { get; }
        public ClientOptions ClientOptions { get; }
    }
    public class LiveSqlSession 
    {
        public LiveSqlSession(ClientSession session, ClientOptions options, int frequency, DateTime startAtPublishTime, string topic, Action<string> log, Action<Exception> exceptionHandler)
        {
            ClientSession = session;
            Frequency = frequency;
            StartAtPublishTime = startAtPublishTime;
            Topic = topic;
            Log = log;
            ExceptionHandler = exceptionHandler;
            ClientOptions = options;
        }
        public Action<Exception> ExceptionHandler { get; }
        public Action<string> Log { get; }
        /// <summary>
        /// Frequency in Milliseconds
        /// </summary>
        public int Frequency { get; }
        /// <summary>
        /// Represents publish time
        /// </summary>
        public DateTime StartAtPublishTime { get; }
        public string Topic { get; }
        public ClientSession ClientSession { get; }
        public ClientOptions ClientOptions { get; }
    }
}
