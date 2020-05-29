using System.Linq;
using System.Threading;
using Akka.Actor;
using PulsarAdmin.Models;
using SharpPulsar.Akka.Admin;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol;

namespace SharpPulsar.Akka.EventSource
{
    public class TopicReplayActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly PulsarSystem _pulsarSystem;
        private readonly StartReplayTopic _replayTopic;
        private ReplayState _replayState;
        private readonly IActorRef _pulsarManager;
        private readonly IActorRef _network;
        private readonly IActorRef _brokerk;
        private TopicName _topicName;
        private bool _external;
        private long _from;
        private long _to;
        private long _max;
        private IActorRef _consumer;
        public TopicReplayActor(PulsarSystem pulsarSystem, StartReplayTopic replayTopic, IActorRef pulsarManager, IActorRef network)
        {
            _topicName = TopicName.Get(replayTopic.ConsumerConfigurationData.SingleTopic);
            _pulsarSystem = pulsarSystem;
            _replayTopic = replayTopic;
            _pulsarManager = pulsarManager;
            _network = network;
            Become(GetStats);
            ReceiveAny(a=> Stash.Stash());

        }
        private void GetStats()
        {
            Receive<ReplayState>(r =>
            {
                _replayState = r;

                if (_external)
                {
                    _pulsarManager.Tell(new NumberOfEntries(_topicName.ToString(), r.Max));
                    _external = false;
                }
                Become(Active);
                Stash.UnstashAll();
            });
            ReceiveAny(_=> Stash.Stash());
            _pulsarSystem.PulsarAdmin(new InternalCommands.Admin(AdminCommands.GetInternalStatsPersistent, new object[] { _tenant, _namespace, _topic, false }, e =>
            {
                if (e != null)
                {
                    var data = (PersistentTopicInternalStats)e;
                    if (_external)
                    {
                        var replayState = new ReplayState
                        {
                            LedgerId = 0,
                            EntryId = 0,
                            To = 0,
                            Max = data.NumberOfEntries
                        };
                        Self.Tell(replayState);
                    }
                    else
                    {
                        var compute = new ComputeMessageId(data, _from, _to, _max);
                        var result = compute.GetFrom();
                        var replayState = new ReplayState
                        {
                            LedgerId = result.Ledger,
                            EntryId = result.Entry,
                            To = data.NumberOfEntries,
                            Max = result.NumberOfEntries
                        };
                        Self.Tell(replayState);
                    }
                    
                   
                }
            }, e =>
            {
                var context = Context;
                context.System.Log.Error(e.ToString());
            }, _replayTopic.Server, l =>
            {
                var context = Context;
                context.System.Log.Info(l);
            }));
        }

        private void Active()
        {
            Receive<NextPlay>(n => { }); ;
            Receive<GetNumberOfEntries>(g =>
            {
                _topicName = g.TopicName;
                _external = true;

            });
        }
        
        public IStash Stash { get; set; }
    }
    
    public sealed class ReplayState
    {
        public long? LedgerId { get; set; }
        public long? EntryId { get; set; }
        public long? Max { get; set; }
        public long? To { get; set; }
    }
}
