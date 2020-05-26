using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.Sql.Live
{
    public sealed class LiveSqlData
    {
        public LiveSqlData(Dictionary<string, object> data, Dictionary<string, object> metadata, string topic)
        {
            Data = data;
            Metadata = metadata;
            Topic = topic;
        }
        public string Topic { get; }
        public Dictionary<string, object> Data { get; }
        public Dictionary<string, object> Metadata { get; }
    }
}
