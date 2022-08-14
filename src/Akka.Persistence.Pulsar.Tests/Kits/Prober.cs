using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Xunit;

namespace Akka.Persistence.Pulsar.Tests.Kits
{
    public class Prober
    {
        private BlockingCollection<MessageEnvelope> _collection;
        private ActorSystem _sys;

        public Prober(ActorSystem sys)
        {
            _collection = new BlockingCollection<MessageEnvelope>();
            _sys = sys;
            Ref = _sys.ActorOf(ProberActor.Prop(_collection));
        }

        public T ExpectMessage<T>(T message, long timeoutms)
        {
            return ExpectMessage<T>(m =>
            {
                Assert.Equal(message, m);
            }, timeoutms);
        }

        public T ExpectMessage<T>(long timeoutms)
        {
            MessageEnvelope envelope = null;
            if (_collection.TryTake(out envelope, TimeSpan.FromMilliseconds(timeoutms)))
            {
                if (!(envelope.Message is T))
                {
                    const string failMessage2 = "Failed: Expected a message of type {0}, but received {{{2}}} (type {1}) instead {3} from {4}";
                    Assert.True(false, string.Format(failMessage2, typeof(T), envelope.Message.GetType(), envelope.Message, "", envelope.Sender));
                }
                
            }
            else
            {
                const string failMessage = "Failed: Timeout {0}ms while waiting for a message of type {1} {2}";
                Assert.True(false, string.Format(failMessage, timeoutms, typeof(T), ""));
            }
            return (T)envelope.Message;
        }

        public T ExpectMessage<T>(Action<T> assert, long timeoutms)
        {
            MessageEnvelope envelope = null;
            if (_collection.TryTake(out envelope, TimeSpan.FromMilliseconds(timeoutms)))
            {
                if (!(envelope.Message is T))
                {
                    const string failMessage2 = "Failed: Expected a message of type {0}, but received {{{2}}} (type {1}) instead {3} from {4}";
                    Assert.True(false, string.Format(failMessage2, typeof(T), envelope.Message.GetType(), envelope.Message, "", envelope.Sender));
                }
                
            }
            else
            {
                const string failMessage = "Failed: Timeout {0}ms while waiting for a message of type {1} {2}";
                Assert.True(false, string.Format(failMessage, timeoutms, typeof(T), ""));
            }
            var msg = (T)envelope.Message;
            assert(msg);
            return msg;
        }

        public T ExpectMessage<T>(long timeoutms, Predicate<T> isMessage)
        {
            MessageEnvelope envelope = null;
            if (_collection.TryTake(out envelope, TimeSpan.FromMilliseconds(timeoutms)))
            {
                if (envelope.Message is T m)
                {
                    if (isMessage != null)
                        Assert.True(isMessage(m), string.Format("Got a message of the expected type <{2}>. Also expected {0} but the message {{{1}}} of type <{3}> did not match",
                            "the predicate to return true", m, typeof(T).FullName, m.GetType().FullName));
                }
                else
                {
                    const string failMessage2 = "Failed: Expected a message of type {0}, but received {{{2}}} (type {1}) instead {3} from {4}";
                    Assert.True(false, string.Format(failMessage2, typeof(T), envelope.Message.GetType(), envelope.Message, "", envelope.Sender));

                }
                
            }
            else
            {
                const string failMessage = "Failed: Timeout {0}ms while waiting for a message of type {1} {2}";
                Assert.True(false, string.Format(failMessage, timeoutms, typeof(T), ""));
            }
            return (T)envelope.Message;
        }
        public IActorRef Ref { get; }
        public IActorRef PerRef { get; }
    }
    public  class MessageEnvelope
    {
        public MessageEnvelope(object message, IActorRef sender)
        {
            Message = message;
            Sender = sender;
        }
        public object Message { get; }

        public IActorRef Sender { get; }
    }
}
