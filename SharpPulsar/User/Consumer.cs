using Akka.Actor;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Configuration;
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
        private readonly ISchema<T> _schema;
        private readonly ConsumerConfigurationData<T> _conf;
        private readonly IActorRef _consumerActor;
        private readonly ConsumerQueueCollections<T> _queue;

        public Consumer(IActorRef consumer, ConsumerQueueCollections<T> queue, ISchema<T> schema, ConsumerConfigurationData<T> conf)
        {
            _consumerActor = consumer;
            _queue = queue;
            _schema = schema;
            _conf = conf;
        }
        public string Topic => _consumerActor.AskFor<string>(GetTopic.Instance);

        public string Subscription => _consumerActor.AskFor<string>(GetSubscription.Instance);

        public IConsumerStats Stats => _consumerActor.AskFor<IConsumerStats>(GetStats.Instance);

        public IMessageId LastMessageId => _consumerActor.AskFor<IMessageId>(GetLastMessageId.Instance);

        public bool Connected => _consumerActor.AskFor<bool>(IsConnected.Instance);

        public string ConsumerName => _consumerActor.AskFor<string>(GetConsumerName.Instance);

        public long LastDisconnectedTimestamp => _consumerActor.AskFor<long>(GetLastDisconnectedTimestamp.Instance);

        public void Acknowledge(IMessage<T> message)
        {
            _consumerActor.Tell(new AcknowledgeMessage<T>(message));
            if (_queue.AcknowledgeException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void Acknowledge(IMessageId messageId)
        {
            _consumerActor.Tell(new AcknowledgeMessageId(messageId));
            if (_queue.AcknowledgeException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void Acknowledge(IMessages<T> messages)
        {
            _consumerActor.Tell(new AcknowledgeMessages<T>(messages));
            if (_queue.AcknowledgeException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void Acknowledge(IList<IMessageId> messageIds)
        {
            _consumerActor.Tell(new AcknowledgeMessageIds(messageIds));
            if (_queue.AcknowledgeException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void AcknowledgeCumulative(IMessage<T> message)
        {
            _consumerActor.Tell(new AcknowledgeCumulativeMessage<T>(message));
            if (_queue.AcknowledgeCumulativeException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void AcknowledgeCumulative(IMessageId messageId)
        {
            _consumerActor.Tell(new AcknowledgeCumulativeMessageId(messageId));
            if (_queue.AcknowledgeCumulativeException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void AcknowledgeCumulative(IMessageId messageId, Transaction txn)
        {
            _consumerActor.Tell(new AcknowledgeCumulativeTxn(messageId, txn.Txn));
            if (_queue.AcknowledgeCumulativeException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public IMessages<T> BatchReceive(int timeout = 5000)
        {
            _consumerActor.Tell(Messages.Consumer.BatchReceive.Instance);
            if (_queue.BatchReceive.TryTake(out var messages, 5000))
                return messages;
            return null;
        }

        public void Close()
        {
            _consumerActor.GracefulStop(TimeSpan.FromSeconds(5));
        }

        public bool? HasReachedEndOfTopic()
        {
            _consumerActor.Tell(Messages.Consumer.HasReachedEndOfTopic.Instance);
            if (_queue.HasReachedEndOfTopic.TryTake(out var reached, 5000))
                return reached;

            return null;
        }

        public void NegativeAcknowledge(IMessage<T> message)
        {
            _consumerActor.Tell(new NegativeAcknowledgeMessage<T>(message));
            if (_queue.NegativeAcknowledgeException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void NegativeAcknowledge(IMessageId messageId)
        {
            _consumerActor.Tell(new NegativeAcknowledgeMessageId(messageId));
            if (_queue.NegativeAcknowledgeException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void NegativeAcknowledge(IMessages<T> messages)
        {
            _consumerActor.Tell(new NegativeAcknowledgeMessages<T>(messages));
            if (_queue.NegativeAcknowledgeException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void Pause()
        {
            _consumerActor.Tell(Messages.Consumer.Pause.Instance);
        }

        public IMessage<T> Receive()
        {
            _consumerActor.Tell(Messages.Consumer.Receive.Instance);
            if (_queue.Receive.TryTake(out var message, 1000))
                return message;
            return null;
        }

        public IMessage<T> Receive(int timeout, TimeUnit unit)
        {
            _consumerActor.Tell(Messages.Consumer.Receive.Instance);
            if (_queue.Receive.TryTake(out var message, (int)unit.ToMilliseconds(timeout)))
                return message;
            return null;
        }

        public void ReconsumeLater(IMessage<T> message, long delayTime, TimeUnit unit)
        {
            _consumerActor.Tell(new ReconsumeLaterMessage<T>(message, delayTime, unit));
            if (_queue.ReconsumeLaterException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void ReconsumeLater(IMessages<T> messages, long delayTime, TimeUnit unit)
        {
            _consumerActor.Tell(new ReconsumeLaterMessages<T>(messages, delayTime, unit));
            if (_queue.ReconsumeLaterException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void ReconsumeLaterCumulative(IMessage<T> message, long delayTime, TimeUnit unit)
        {
            _consumerActor.Tell(new ReconsumeLaterCumulative<T>(message, delayTime, unit));
            if (_queue.ReconsumeLaterException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void RedeliverUnacknowledgedMessages()
        {
            _consumerActor.Tell(Messages.Consumer.RedeliverUnacknowledgedMessages.Instance);
            if (_queue.RedeliverUnacknowledgedException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void Resume()
        {
            _consumerActor.Tell(Messages.Consumer.Resume.Instance);
        }

        public void Seek(IMessageId messageId)
        {
            _consumerActor.Tell(new SeekMessageId(messageId));
            if (_queue.SeekException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void Seek(long timestamp)
        {
            _consumerActor.Tell(new SeekTimestamp(timestamp));
            if (_queue.SeekException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void Unsubscribe()
        {
            _consumerActor.Tell(Messages.Consumer.Unsubscribe.Instance);
        }
    }
}
