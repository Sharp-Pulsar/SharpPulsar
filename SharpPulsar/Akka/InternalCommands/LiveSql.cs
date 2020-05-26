
using System;
using System.Globalization;

namespace SharpPulsar.Akka.InternalCommands
{
    public class LiveSql
    {
        public LiveSql(string command, int frequency, DateTime startAtPublishTime, string topic, string server, Action<string> log, Action<Exception> exceptionHandler)
        {
            Command = command;
            Frequency = frequency;
            StartAtPublishTime = startAtPublishTime;
            Topic = topic;
            Server = server;
            Log = log;
            ExceptionHandler = exceptionHandler;
        }
        public Action<Exception> ExceptionHandler { get; }
        public Action<string> Log { get; }
        public string Server { get; }
        public string Command { get; }
        /// <summary>
        /// Frequency in Milliseconds
        /// </summary>
        public int Frequency { get; }
        /// <summary>
        /// Represents publish time
        /// </summary>
        public DateTime StartAtPublishTime { get; }
        public string Topic { get; }
    }
}
