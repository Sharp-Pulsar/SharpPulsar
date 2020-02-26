using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.InternalCommands
{
    public class Payload
    {
        public byte[] Bytes { get; }

        public Payload(byte[] bytes)
        {
            Bytes = bytes;  
        }
    }
}
