using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Akka.Actor;
using SharpPulsar.User;

namespace SharpPulsar.Messages
{
    public sealed class GetProducers
    {
        
        public static GetProducers Instance = new GetProducers();
    }
    public sealed class SetProducers
    {
        public ConcurrentDictionary<int, IActorRef> Producers { get; private set; } 
       
        public SetProducers(ConcurrentDictionary<int, IActorRef> producers)
        {
            Producers = producers;
        }
    }
}
