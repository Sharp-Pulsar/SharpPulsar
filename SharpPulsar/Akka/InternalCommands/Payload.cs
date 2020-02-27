using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.InternalCommands
{
    public class Payload
    {
        public byte[] Bytes { get; }
        public string CommandType { get; }
        public long RequestId { get; }

        public Payload(byte[] bytes, long requestId, string commandType)
        {
            Bytes = bytes;
            RequestId = requestId;
            CommandType = commandType;
        }
    }
}
