using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Messages
{
    public sealed class GetConnection
    {
        public string Topic { get; }
        public GetConnection(string topic)
        {
            Topic = topic;
        }
    }
}
