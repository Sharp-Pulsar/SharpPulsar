using System.Collections.Concurrent;

using Akka.Actor;

namespace SharpPulsar.Messages
{
    public record GetProducers
    {
        
        public static GetProducers Instance = new GetProducers();
    }
    public record SetProducers
    {
        public ConcurrentDictionary<int, IActorRef> Producers { get; } 
       
        public SetProducers(ConcurrentDictionary<int, IActorRef> producers)
        {
            Producers = producers;
        }
    }
}
