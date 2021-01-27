using Akka.Actor;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.Transaction;
using SharpPulsar.Stats.Consumer.Api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SharpPulsar.User
{
    public class Consumer<T> : IConsumer<T>
    {
        public string Topic => throw new NotImplementedException();

        public string Subscription => throw new NotImplementedException();

        public IConsumerStats Stats => throw new NotImplementedException();

        public IMessageId LastMessageId => throw new NotImplementedException();

        public bool Connected => throw new NotImplementedException();

        public string ConsumerName => throw new NotImplementedException();

        public long LastDisconnectedTimestamp => throw new NotImplementedException();

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
