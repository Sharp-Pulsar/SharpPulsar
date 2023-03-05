using SharpPulsar.Trino.Message;

namespace SharpPulsar.Trino.Live
{
    public sealed class LiveSqlData
    {
        public LiveSqlData(IQueryResponse response, string topic)
        {
            Response = response;
            Topic = topic;
        }
        public string Topic { get; }
        public IQueryResponse Response { get; }
    }
}
