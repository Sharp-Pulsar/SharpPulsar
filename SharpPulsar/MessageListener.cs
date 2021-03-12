using Akka.Actor;
using SharpPulsar.Interfaces;
using System;

namespace SharpPulsar
{
    public class MessageListener<T> : IMessageListener<T>
    {
        public Action<IActorRef, IMessage<T>> Consume { get; }
        public Action<IActorRef> EndOfTopic { get; }
        /// <summary>
        /// Acknowledge(IMessageId messageId) => consumer.Tell(new AcknowledgeMessageId(messageId));
        /// Acknowledge(IMessage<T> message) => consumer.Tell(new AcknowledgeMessage<T>(message));
        /// </summary>
        /// <param name="message"></param>
        /// <param name="endOfTopic"></param>
        public MessageListener(Action<IActorRef, IMessage<T>> message, Action<IActorRef> endOfTopic)
        {
            Consume = message;
            EndOfTopic = endOfTopic;
        }
        public void ReachedEndOfTopic(IActorRef consumer)
        {
           EndOfTopic?.Invoke(consumer);
        }

        public void Received(IActorRef consumer, IMessage<T> msg)
        {
            Consume?.Invoke(consumer, msg);
        }
    }
}
