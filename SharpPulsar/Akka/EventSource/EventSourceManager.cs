using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Akka.Actor;
using Nito.AsyncEx;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.EventSource.Messages;
using SharpPulsar.Akka.EventSource.Pulsar;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Akka.EventSource
{
    public class EventSourceManager:ReceiveActor
    {
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        public EventSourceManager(IActorRef network, IActorRef pulsarManager)
        {
            _network = network;
            _pulsarManager = pulsarManager;
            Context.ActorOf(PulsarSourceCoordinator.Prop(network, pulsarManager), "PulsarSourceManager");
            Receive<IEventSourceMessage>(HandleMessage);
        }

        private void HandleMessage(IEventSourceMessage message)
        {
            switch (message)
            {
                case CurrentEventsByTopic _:
                case EventsByTopic _:
                case CurrentEventsByTag _:
                case EventsByTag _:
                    if (message.Source == SourceType.Pulsar)
                        Context.Child("PulsarSourceManager").Forward(message);
                    else
                        Context.Child("PrestoSourceManager").Forward(message);
                    break;
                case CurrentEventTopics _:
                    var ns = Regex.Replace(message.Namespace, @"[^\w\d]", "");
                    if(Context.Child(ns).IsNobody())
                       Context.ActorOf(TopicUpdater.Prop(_network, message, _pulsarManager), ns);
                    break;
                case EventTopics _:
                    var topics = TopicsHelper.Topics(message, _network);
                    _pulsarManager.Tell(new ActiveTopics(message.Namespace, topics.ToImmutableList()));
                    break;
                default:
                    Context.System.Log.Info($"{message.GetType().FullName} not supported");
                    break;
            }
        }
        public static Props Prop(IActorRef network, IActorRef pulsarManager)
        {
            return Props.Create(()=> new EventSourceManager(network, pulsarManager));
        }
    }

    public class TopicUpdater : ReceiveActor
    {
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        private readonly IEventSourceMessage _message;
        private ICancelable _updaterCancelable;
        private readonly IAdvancedScheduler _scheduler;
        public TopicUpdater(IActorRef network, IEventSourceMessage message, IActorRef pulsarManager)
        {
            _network = network;
            _message = message;
            _pulsarManager = pulsarManager;
            _scheduler = Context.System.Scheduler.Advanced;
            _updaterCancelable = _scheduler.ScheduleOnceCancelable(TimeSpan.FromSeconds(30), GetTopics);
        }

        private void GetTopics()
        {
            try
            {
                var topics = TopicsHelper.Topics(_message, _network);
                _pulsarManager.Tell(new ActiveTopics(_message.Namespace, topics.ToImmutableList()));
            }
            finally
            {
                _updaterCancelable = _scheduler.ScheduleOnceCancelable(TimeSpan.FromSeconds(30), GetTopics);
            }
            
        }
        protected override void PostStop()
        {
            _updaterCancelable?.Cancel();
        }

        public static Props Prop(IActorRef network, IEventSourceMessage message, IActorRef pulsarManager)
        {
            return Props.Create(()=> new TopicUpdater(network, message, pulsarManager));
        }
    }

    public static class TopicsHelper
    {
        public static IList<string> Topics(IEventSourceMessage message, IActorRef network)
        {
            var nameSpace = message.Namespace;
            var topic = new Regex($"persistent://{message.Tenant}/{nameSpace}/{message.Topic}");
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewGetTopicsOfNamespaceRequest(nameSpace, requestId, CommandGetTopicsOfNamespace.Mode.Persistent);
            var payload = new Payload(request, requestId, "GetTopicsOfNamespace");
            var ask = network.Ask<NamespaceTopics>(payload);
            var t = SynchronizationContextSwitcher.NoContext(async () => await ask).Result;
            return TopicsPatternFilter(t.Topics, topic);
            
        }
        private static IList<string> TopicsPatternFilter(IList<string> original, Regex topicsPattern)
        {
            var pattern = topicsPattern.ToString().Contains("://") ? new Regex(Regex.Split(topicsPattern.ToString(), @"\:\/\/")[1]) : topicsPattern;

            return original.Select(TopicName.Get).Select(x => x.ToString()).Where(topic => pattern.Match(Regex.Split(topic, @"\:\/\/")[1]).Success).ToList();
        }
    }
}
