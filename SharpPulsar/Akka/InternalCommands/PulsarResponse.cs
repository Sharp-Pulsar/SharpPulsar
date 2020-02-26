using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.InternalCommands
{
    public class PulsarResponse
    {
        public object Message { get; }

        public PulsarResponse(object message)
        {
            Message = message;
        }
    }
}
