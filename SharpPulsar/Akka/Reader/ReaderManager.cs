using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using SharpPulsar.Akka.Producer;

namespace SharpPulsar.Akka.Reader
{
    public class ReaderManager:ReceiveActor
    {
        public ReaderManager()
        {
                
        }
        public static Props Prop()
        {
            return Props.Create(() => new ReaderManager());
        }
    }
}
