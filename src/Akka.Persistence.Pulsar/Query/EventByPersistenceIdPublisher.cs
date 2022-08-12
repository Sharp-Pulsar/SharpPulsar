//-----------------------------------------------------------------------
// <copyright file="EventsByPersistenceIdPublisher.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using Akka.Actor;
using Akka.Event;
using Akka.Persistence.Query;
using Akka.Streams.Actors;

namespace Akka.Persistence.Pulsar.Query
{
    internal static class EventsByPersistenceIdPublisher
    {
        [Serializable]
        public sealed class Continue
        {
            public static readonly Continue Instance = new Continue();

            private Continue()
            {
            }
        }

        public static Props Props(string persistenceId, long fromSequenceNr, long toSequenceNr, TimeSpan? refreshDuration, int maxBufferSize, string writeJournalPluginId)
        {
            return refreshDuration.HasValue
                ? Actor.Props.Create(() => new LiveEventsByPersistenceIdPublisher(persistenceId, fromSequenceNr, toSequenceNr, maxBufferSize, writeJournalPluginId, refreshDuration.Value))
                : Actor.Props.Create(() => new CurrentEventsByPersistenceIdPublisher(persistenceId, fromSequenceNr, toSequenceNr, maxBufferSize, writeJournalPluginId));
        }
    }

    internal abstract class AbstractEventsByPersistenceIdPublisher : ActorPublisher<EventEnvelope>
    {
        private ILoggingAdapter _log;

        protected DeliveryBuffer<EventEnvelope> Buffer;
        protected readonly IActorRef JournalRef;
        protected long CurrentSequenceNr;

        protected AbstractEventsByPersistenceIdPublisher(string persistenceId, long fromSequenceNr, long toSequenceNr, int maxBufferSize, string writeJournalPluginId)
        {
            PersistenceId = persistenceId;
            CurrentSequenceNr = FromSequenceNr = fromSequenceNr;
            ToSequenceNr = toSequenceNr;
            MaxBufferSize = maxBufferSize;
            WriteJournalPluginId = writeJournalPluginId;
            Buffer = new DeliveryBuffer<EventEnvelope>(OnNext);

            JournalRef = Persistence.Instance.Apply(Context.System).JournalFor(writeJournalPluginId);
        }

        protected ILoggingAdapter Log => _log ?? (_log = Context.GetLogger());
        protected string PersistenceId { get; }
        protected long FromSequenceNr { get; }
        protected long ToSequenceNr { get; set; }
        protected int MaxBufferSize { get; }
        protected string WriteJournalPluginId { get; }

        protected bool IsTimeForReplay => (Buffer.IsEmpty || Buffer.Length <= MaxBufferSize / 2) && (CurrentSequenceNr <= ToSequenceNr);

        protected abstract void ReceiveInitialRequest();
        protected abstract void ReceiveIdleRequest();
        protected abstract void ReceiveRecoverySuccess(long highestSequenceNr);

        protected override bool Receive(object message)
        {
            return Init(message);
        }

        protected bool Init(object message)
        {
            switch (message)
            {
                case EventsByPersistenceIdPublisher.Continue _:
                    // do nothing
                    break;
                case Request _:
                    ReceiveInitialRequest();
                    break;
                case Cancel _:
                    Context.Stop(Self);
                    break;
                default:
                    return false;
            }
            return true;
        }

        protected bool Idle(object message)
        {
            switch (message)
            {
                case EventsByPersistenceIdPublisher.Continue _:
                case EventAppended _:
                    if (IsTimeForReplay) Replay();
                    break;
                case Request _:
                    ReceiveIdleRequest();
                    break;
                case Cancel _:
                    Context.Stop(Self);
                    break;
                default:
                    return false;
            }

            return true;
        }

        protected void Replay()
        {
            var limit = MaxBufferSize - Buffer.Length;
            Log.Debug("request replay for persistenceId [{0}] from [{1}] to [{2}] limit [{3}]", PersistenceId, CurrentSequenceNr, ToSequenceNr, limit);
            JournalRef.Tell(new ReplayMessages(CurrentSequenceNr, ToSequenceNr, limit, PersistenceId, Self));
            Context.Become(Replaying(limit));
        }

        protected Receive Replaying(int limit)
        {
            return message =>
            {
                switch (message)
                {
                    case ReplayedMessage replayed:
                        var seqNr = replayed.Persistent.SequenceNr;
                        Buffer.Add(new EventEnvelope(
                            offset: new Sequence(seqNr),
                            persistenceId: PersistenceId,
                            sequenceNr: seqNr,
                            @event: replayed.Persistent.Payload));
                        CurrentSequenceNr = seqNr + 1;
                        Buffer.DeliverBuffer(TotalDemand);
                        break;
                    case RecoverySuccess success:
                        Log.Debug("replay completed for persistenceId [{0}], currSeqNo [{1}]", PersistenceId, CurrentSequenceNr);
                        ReceiveRecoverySuccess(success.HighestSequenceNr);
                        break;
                    case ReplayMessagesFailure failure:
                        Log.Debug("replay failed for persistenceId [{0}], due to [{1}]", PersistenceId, failure.Cause.Message);
                        Buffer.DeliverBuffer(TotalDemand);
                        OnErrorThenStop(failure.Cause);
                        break;
                    case Request _:
                        Buffer.DeliverBuffer(TotalDemand);
                        break;
                    case EventsByPersistenceIdPublisher.Continue _:
                    case EventAppended _:
                        // Skip during replay
                        break;
                    case Cancel _:
                        Context.Stop(Self);
                        break;
                    default:
                        return false;
                }

                return true;
            };
        }
    }

    internal sealed class LiveEventsByPersistenceIdPublisher : AbstractEventsByPersistenceIdPublisher
    {
        private readonly ICancelable _tickCancelable;

        public LiveEventsByPersistenceIdPublisher(string persistenceId, long fromSequenceNr, long toSequenceNr, int maxBufferSize, string writeJournalPluginId, TimeSpan refreshInterval)
            : base(persistenceId, fromSequenceNr, toSequenceNr, maxBufferSize, writeJournalPluginId)
        {
            _tickCancelable = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(refreshInterval, refreshInterval, Self, EventsByPersistenceIdPublisher.Continue.Instance, Self);
        }

        protected override void PostStop()
        {
            _tickCancelable.Cancel();
            base.PostStop();
        }

        protected override void ReceiveInitialRequest()
        {
            JournalRef.Tell(new SubscribePersistenceId(PersistenceId));
            Replay();
        }

        protected override void ReceiveIdleRequest()
        {
            Buffer.DeliverBuffer(TotalDemand);
            if (Buffer.IsEmpty && CurrentSequenceNr > ToSequenceNr)
                OnCompleteThenStop();
        }

        protected override void ReceiveRecoverySuccess(long highestSequenceNr)
        {
            Buffer.DeliverBuffer(TotalDemand);
            if (Buffer.IsEmpty && CurrentSequenceNr > ToSequenceNr)
                OnCompleteThenStop();

            Context.Become(Idle);
        }
    }

    internal sealed class CurrentEventsByPersistenceIdPublisher : AbstractEventsByPersistenceIdPublisher
    {
        public CurrentEventsByPersistenceIdPublisher(string persistenceId, long fromSequenceNr, long toSequenceNr, int maxBufferSize, string writeJournalPluginId)
            : base(persistenceId, fromSequenceNr, toSequenceNr, maxBufferSize, writeJournalPluginId)
        {
        }

        protected override void ReceiveInitialRequest()
        {
            Replay();
        }

        protected override void ReceiveIdleRequest()
        {
            Buffer.DeliverBuffer(TotalDemand);
            if (Buffer.IsEmpty && CurrentSequenceNr > ToSequenceNr)
                OnCompleteThenStop();
            else
                Self.Tell(EventsByPersistenceIdPublisher.Continue.Instance);
        }

        protected override void ReceiveRecoverySuccess(long highestSequenceNr)
        {
            Buffer.DeliverBuffer(TotalDemand);
            if (highestSequenceNr < ToSequenceNr)
                ToSequenceNr = highestSequenceNr;
            if (Buffer.IsEmpty && (CurrentSequenceNr > ToSequenceNr || CurrentSequenceNr == FromSequenceNr))
                OnCompleteThenStop();
            else
                Self.Tell(EventsByPersistenceIdPublisher.Continue.Instance);

            Context.Become(Idle);
        }
    }
}
