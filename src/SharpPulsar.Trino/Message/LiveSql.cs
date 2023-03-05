using SharpPulsar.Trino.Trino;

namespace SharpPulsar.Trino.Message
{
    public sealed class LiveSqlQuery
    {
        public LiveSqlQuery(ClientOptions options, TimeSpan frequency, DateTime startAtPublishTime, string topic)
        {
            ClientOptions = options;
            Frequency = frequency;
            StartAtPublishTime = startAtPublishTime;
            Topic = topic;
        }
        /// <summary>
        /// Frequency in Milliseconds
        /// </summary>
        public TimeSpan Frequency { get; }
        /// <summary>
        /// Represents publish time
        /// </summary>
        public DateTime StartAtPublishTime { get; }
        public string Topic { get; }
        public ClientOptions ClientOptions { get; }
    }
    public sealed class LiveSqlSession
    {
        public LiveSqlSession(ClientSession session, ClientOptions options, TimeSpan frequency, DateTime startAtPublishTime, string topic)
        {
            ClientSession = session;
            Frequency = frequency;
            StartAtPublishTime = startAtPublishTime;
            Topic = topic;
            ClientOptions = options;
        }
        /// <summary>
        /// Frequency in Milliseconds
        /// </summary>
        public TimeSpan Frequency { get; }
        /// <summary>
        /// Represents publish time
        /// </summary>
        public DateTime StartAtPublishTime { get; }
        public string Topic { get; }
        public ClientSession ClientSession { get; }
        public ClientOptions ClientOptions { get; }
    }
}
