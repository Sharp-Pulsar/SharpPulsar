using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Text.RegularExpressions;
using Akka.Actor;
using PulsarAdmin;
using SharpPulsar.Akka.EventSource.Messages;
using SharpPulsar.Akka.EventSource.Messages.Presto;
using SharpPulsar.Akka.EventSource.Pulsar;
using SharpPulsar.Akka.InternalCommands.Consumer;

namespace SharpPulsar.Akka.EventSource
{
    public class EventSourceManager:ReceiveActor
    {
        private readonly IActorRef _network;
        private readonly IActorRef _pulsarManager;
        private readonly HttpClient _httpClient;
        public EventSourceManager(IActorRef network, IActorRef pulsarManager)
        {
            _httpClient = new HttpClient();
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
                    var topics = TopicsHelper.Topics(message, new PulsarAdminRESTAPI(message.AdminUri, _httpClient, true));
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
        private readonly IActorRef _pulsarManager;
        private readonly IEventTopics _message;
        private ICancelable _updaterCancelable;
        private readonly IAdvancedScheduler _scheduler;
        private readonly PulsarAdminRESTAPI _adminRestapi;
        public TopicUpdater(IActorRef network, IEventTopics message, IActorRef pulsarManager)
        {
            _adminRestapi = new PulsarAdminRESTAPI(message.AdminUri, new HttpClient(), true);
            _message = message;
            _pulsarManager = pulsarManager;
            _scheduler = Context.System.Scheduler.Advanced;
            _updaterCancelable = _scheduler.ScheduleOnceCancelable(TimeSpan.FromSeconds(3), GetTopics);
        }

        private void GetTopics()
        {
            try
            {
                var topics = TopicsHelper.Topics(_message, _adminRestapi);
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
        public static IList<string> Topics(IEventTopics message, PulsarAdminRESTAPI adminRestapi)
        {
            var response = adminRestapi.GetTopics(message.Tenant, message.Namespace, "ALL");
            if(response == null)
                return new List<string>();
            return response;
        }
        
    }
}
