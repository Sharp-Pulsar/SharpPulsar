using System;
using System.Collections.Generic;
using System.Text;
using SharpPulsar.Shared;

namespace SharpPulsar.Akka.InternalCommands
{
    public class Payload
    {
        public byte[] Bytes { get; }
        public string CommandType { get; }
        public long RequestId { get; }
        public ByteBufPair Message { get; }

        public Payload(byte[] bytes, long requestId, string commandType)
        {
            Bytes = bytes;
            RequestId = requestId;
            CommandType = commandType;
        }
        public Payload(ByteBufPair pair, long requestId, string commandType)
        {
            Message = pair;
            RequestId = requestId;
            CommandType = commandType;
        }
    }
}
