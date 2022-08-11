using System;
using System.Collections.Concurrent;
using Akka.Actor;
using Akka.Persistence.Query;
using Akka.Streams.Actors;

namespace Akka.Persistence.Pulsar.Tests.Kits
{
    public class ProberActor : ActorBase
    {
        private readonly BlockingCollection<MessageEnvelope> _queue;

        public ProberActor(BlockingCollection<MessageEnvelope> queue)
        {
            _queue = queue;
        }
        protected override bool Receive(object message)
        {

            global::System.Diagnostics.Debug.WriteLine("ProberActor received " + message);
            switch (message)
            {
                case ReplayedMessage a:
                case RecoverySuccess b:
                case WriteMessagesSuccessful c:
                case WriteMessageSuccess d:
                case EventEnvelope e:
                case LoadSnapshotResult f:
                case SaveSnapshotSuccess g:
                case ReplayMessagesFailure y:
                case OnComplete oc:
                case string s:
                    _queue.Add(new MessageEnvelope(message, Sender));
                    break;
                case OnNext n:
                    _queue.Add(new MessageEnvelope(n.Element, Sender));
                    break;

            }
            return true;
        }

        public static Props Prop(BlockingCollection<MessageEnvelope> queue)
        {
            return Props.Create(()=> new ProberActor(queue));
        }
    }
}
