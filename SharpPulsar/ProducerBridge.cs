using Akka.Actor;
using SharpPulsar.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar
{
    /// <summary>
    /// This class intended use to bridge the outside world with the Producer actor
    /// I implements all the standard pulsar producer methods, but beneath it construct a message to the actor
    /// It would hold a reference to the actor.
    /// This bridge will be constructed and returned after user calls pulsarsystem.createproducer<T>
    /// </summary>
    public sealed class ProducerBridge
    {
        private readonly IActorRef _producer;
        public ProducerBridge(IActorRef producer)
        {
            _producer = producer;
        }
        /// <summary>
        /// Example of intended use
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public void Send<T>(T message)
        {
            //construct the and tell the producer actor
            // the messageid will be placed in the queue by the actor to be picked up here
        }
    }
}
