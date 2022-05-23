using System;
using System.Collections.Generic;

using Akka.Actor;
using SharpPulsar.User;

namespace SharpPulsar.Messages
{
    public sealed class GetProducers<T>
    {
        public IList<Producer<T>> Producers { get; }
        public GetProducers(IList<Producer<T>> producers) 
        { 
            Producers = producers; 
        }
    }
    public sealed class SetProducers
    {
        public static SetProducers Instance = new SetProducers();
    }
}
