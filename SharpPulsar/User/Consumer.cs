using Akka.Actor;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Queues;
using SharpPulsar.Stats.Consumer.Api;
using System;
using System.Collections.Generic;

namespace SharpPulsar.User
{
    public class Consumer<T> : IConsumer<T>
    {
        //Either of three: Pattern, Multi, Single topic consumer
        private readonly IActorRef _consumerActor;
        private readonly ConsumerQueueCollections<T> _queue;

        public Consumer(IActorRef consumer, ConsumerQueueCollections<T> queue)
        {
            _consumerActor = consumer;
            _queue = queue;
        }
        public string Topic => _consumerActor.AskFor<string>(GetTopic.Instance);

        public string Subscription => _consumerActor.AskFor<string>(GetSubscription.Instance);

        public IConsumerStats Stats => _consumerActor.AskFor<IConsumerStats>(GetStats.Instance);

        public IMessageId LastMessageId => _consumerActor.AskFor<IMessageId>(GetLastMessageId.Instance);

        public bool Connected => _consumerActor.AskFor<bool>(IsConnected.Instance);

        public string ConsumerName => _consumerActor.AskFor<string>(GetConsumerName.Instance);

        public long LastDisconnectedTimestamp => _consumerActor.AskFor<long>(GetLastDisconnectedTimestamp.Instance);

        public void Acknowledge<T1>(IMessage<T1> message)
        {
            throw new NotImplementedException();
        }

        public void Acknowledge(IMessageId messageId)
        {
            throw new NotImplementedException();
        }

        public void Acknowledge<T1>(IMessages<T1> messages)
        {
            throw new NotImplementedException();
        }

        public void Acknowledge(IList<IMessageId> messageIdList)
        {
            throw new NotImplementedException();
        }

        public void AcknowledgeCumulative<T1>(IMessage<T1> message)
        {
            throw new NotImplementedException();
        }

        public void AcknowledgeCumulative(IMessageId messageId)
        {
            throw new NotImplementedException();
        }

        public void AcknowledgeCumulative(IMessageId messageId, IActorRef txn)
        {
            throw new NotImplementedException();
        }

        public IMessages<T> BatchReceive()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool HasReachedEndOfTopic()
        {
            throw new NotImplementedException();
        }

        public void NegativeAcknowledge<T1>(IMessage<T1> message)
        {
            throw new NotImplementedException();
        }

        public void NegativeAcknowledge(IMessageId messageId)
        {
            throw new NotImplementedException();
        }

        public void NegativeAcknowledge<T1>(IMessages<T1> messages)
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public IMessage<T> Receive()
        {
            throw new NotImplementedException();
        }

        public IMessage<T> Receive(int timeout, TimeUnit unit)
        {
            throw new NotImplementedException();
        }

        public void ReconsumeLater<T1>(IMessage<T1> message, long delayTime, TimeUnit unit)
        {
            throw new NotImplementedException();
        }

        public void ReconsumeLater<T1>(IMessages<T1> messages, long delayTime, TimeUnit unit)
        {
            throw new NotImplementedException();
        }

        public void ReconsumeLaterCumulative<T1>(IMessage<T1> message, long delayTime, TimeUnit unit)
        {
            throw new NotImplementedException();
        }

        public void RedeliverUnacknowledgedMessages()
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void Seek(IMessageId messageId)
        {
            throw new NotImplementedException();
        }

        public void Seek(long timestamp)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe()
        {
            throw new NotImplementedException();
        }
    }
}
