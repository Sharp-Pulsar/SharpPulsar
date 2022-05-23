using System;
using System.Collections.Generic;

using Akka.Actor;

namespace SharpPulsar.Messages
{
    public sealed class GetProducers
    {
        public IList<IActorRef> Producers { get; }
        public GetProducers(IList<IActorRef> producers) 
        { 
            Producers = producers; 
        }
    }
    public sealed class SetProducers
    {
        public static SetProducers Instance = new SetProducers();
    }
}
