using System.Collections.Concurrent;

using Akka.Actor;

namespace SharpPulsar.Messages
{
    public readonly record struct GetProducers
    {
        
        public static GetProducers Instance = new GetProducers();
    }
    public readonly record struct SetProducers
    {
        public ConcurrentDictionary<int, IActorRef> Producers { get; } 
       
        public SetProducers(ConcurrentDictionary<int, IActorRef> producers)
        {
            Producers = producers;
        }
    }
}
