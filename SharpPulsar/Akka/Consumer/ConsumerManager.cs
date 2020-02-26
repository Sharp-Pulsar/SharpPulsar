using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Consumer
{
    public class ConsumerManager<T>:ReceiveActor
    {
        public ConsumerManager()
        {
            Receive<Subscribe<T>>(subscribe => { });
        }

        public static Props Prop()
        {
            return Props.Empty;
        }
    }
}
