using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Threading;
using Akka.Actor;
using Nito.AsyncEx;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.EventSource.Messages;
using SharpPulsar.Akka.EventSource.Messages.Presto;
using SharpPulsar.Akka.EventSource.Pulsar;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
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
            Context.ActorOf(PrestoSourceCoordinator.Prop(network, pulsarManager), "PulsarSource");
            Receive<IEventSourceMessage>(HandleMessage);
            Receive<IEventTopics>(HandleMessage);
        }
        private void HandleMessage(IEventTopics message)
        {
            var ns = Regex.Replace($"{message.Tenant}{message.Namespace}", @"[^\w\d]", "");
            var child = Context.Child(ns);
            switch (message)
            {
                case EventTopics _:
                    if (child.IsNobody())
                        Context.ActorOf(TopicUpdater.Prop(_network, message, _pulsarManager), ns);
                    break;
                case CurrentEventTopics _:
                    var topics = TopicsHelper.Topics(message, _network);
                    _pulsarManager.Tell(new ActiveTopics(message.Namespace, topics.ToImmutableList()));
                    break;
                default:
                    Context.System.Log.Info($"{message.GetType().FullName} not supported");
                    break;
            }
        }

        private void HandleMessage(IEventSourceMessage message)
        {
            var ns = Regex.Replace($"{message.Tenant}{message.Namespace}{message.Source}", @"[^\w\d]", "");
            var child = Context.Child(ns);
            switch (message)
            {
                case CurrentEventsByTopic _:
                case EventsByTopic _:
                case CurrentEventsByTag _:
                case EventsByTag _:
                    if (message.Source == SourceType.Pulsar)
                    {
                        if (child.IsNobody())
                            child = Context.ActorOf(PrestoSourceCoordinator.Prop(_network, _pulsarManager), ns);
                        child.Forward(message);
                    }
                    else
                    {
                        if (child.IsNobody())
                            child = Context.ActorOf(PrestoSourceCoordinator.Prop(_network, _pulsarManager), ns);
                        child.Forward(message);
                        Context.Child("PrestoSource").Forward(message);
                    }
                        
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
        private readonly IEventTopics _message;
        private ICancelable _updaterCancelable;
        private readonly IAdvancedScheduler _scheduler;
        public TopicUpdater(IActorRef network, IEventTopics message, IActorRef pulsarManager)
        {
            _network = network;
            _message = message;
            _pulsarManager = pulsarManager;
            _scheduler = Context.System.Scheduler.Advanced;
            _updaterCancelable = _scheduler.ScheduleOnceCancelable(TimeSpan.FromSeconds(3), GetTopics);
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

        public static Props Prop(IActorRef network, IEventTopics message, IActorRef pulsarManager)
        {
            return Props.Create(()=> new TopicUpdater(network, message, pulsarManager));
        }
    }

    public static class TopicsHelper
    {
        public static IList<string> Topics(IEventTopics message, IActorRef network)
        {
            var nameSpace = message.Namespace;
            var requestId = Interlocked.Increment(ref IdGenerators.RequestId);
            var request = Commands.NewGetTopicsOfNamespaceRequest(nameSpace, requestId, CommandGetTopicsOfNamespace.Mode.Persistent);
            var payload = new Payload(request, requestId, "GetTopicsOfNamespace");
            var ask = network.Ask<NamespaceTopics>(payload);
            var t = SynchronizationContextSwitcher.NoContext(async () => await ask).Result;
            return t.Topics;

        }
        
    }
}
