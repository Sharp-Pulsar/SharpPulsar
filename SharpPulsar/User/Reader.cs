using Akka.Actor;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Reader;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Queues;
using System;
using System.Threading.Tasks;

namespace SharpPulsar.User
{
    public class Reader<T> : IReader<T>
    {
        private readonly ISchema<T> _schema;
        private readonly ReaderConfigurationData<T> _conf;
        private readonly IActorRef _readerActor;
        private readonly ConsumerQueueCollections<T> _queue;

        public Reader(IActorRef consumer, ConsumerQueueCollections<T> queue, ISchema<T> schema, ReaderConfigurationData<T> conf)
        {
            _readerActor = consumer;
            _queue = queue;
            _schema = schema;
            _conf = conf;
        }
        public string Topic => _readerActor.AskFor<string>(GetTopic.Instance);

        public bool Connected => _readerActor.AskFor<bool>(IsConnected.Instance);


        public void Stop()
        {
            _readerActor.GracefulStop(TimeSpan.FromSeconds(5));
        }

        public bool HasMessageAvailable()
        {
            _readerActor.Tell(Messages.Consumer.HasMessageAvailable.Instance);
            return _queue.HasMessageAvailable.Take();
        }

        public bool HasReachedEndOfTopic()
        {
            _readerActor.Tell(Messages.Consumer.HasReachedEndOfTopic.Instance);
            return _queue.HasReachedEndOfTopic.Take();
        }

        public IMessage<T> ReadNext()
        {
            _readerActor.Tell(Messages.Reader.ReadNext.Instance);
            if (_queue.Receive.TryTake(out var message, 1000))
            {
                _readerActor.Tell(new AcknowledgeCumulativeMessage<T>(message));
                return message;
            }
            return null;
        }

        public IMessage<T> ReadNext(int timeout, TimeUnit unit)
        {
            _readerActor.Tell(new ReadNextTimeout(timeout, unit));
            if (_queue.Receive.TryTake(out var message, (int)unit.ToMilliseconds(timeout)))
            {
                _readerActor.Tell(new AcknowledgeCumulativeMessage<T>(message));
                return message;
            }                
            return null;
        }
        public void Seek(IMessageId messageId)
        {
            _readerActor.Tell(new SeekMessageId(messageId));
            if (_queue.SeekException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

        public void Seek(long timestamp)
        {
            _readerActor.Tell(new SeekTimestamp(timestamp));
            if (_queue.SeekException.TryTake(out var msg, 1000))
                if (msg.Exception != null)
                    throw msg.Exception;
        }

    }
}
