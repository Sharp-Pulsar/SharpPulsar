
using System;
using SharpPulsar.Akka.Sql.Client;

namespace SharpPulsar.Akka.InternalCommands
{
    public class LiveSql
    {
        public LiveSql(ClientOptions options, int frequency, DateTime startAtPublishTime, string topic, Action<string> log, Action<Exception> exceptionHandler)
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
}
