﻿using System.Linq;
using System.Text.RegularExpressions;
using Akka.Actor;
using SharpPulsar.Akka.Admin;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Akka.Function;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Akka.Sql;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka
{
    public class PulsarManager:ReceiveActor, IWithUnboundedStash
    {
        private IActorRef _network;
        private IActorRef _topicManager;
        private ClientConfigurationData _config;
        private PulsarServiceNameResolver _serviceNameResolver = new PulsarServiceNameResolver();
        public PulsarManager(ClientConfigurationData conf)
        {
            _config = conf;
            _serviceNameResolver.UpdateServiceUrl(conf.ServiceUrl);
            Become(NetworkSetup);
        }

        private void Ready()
        {
            Receive<NewConsumer>(cmd =>
            {
                Context.Child("ConsumerManager").Tell(cmd);
            });
            Receive<SqlServers>(cmd =>
            {
                Context.Child("SqlManager").Tell(cmd);
            });
            Receive((InternalCommands.Sql cmd) =>
            {
                Context.Child("SqlManager").Tell(cmd);
            });
            Receive((InternalCommands.Admin cmd) =>
            {
                Context.Child("AdminManager").Tell(cmd);
            });
            Receive((InternalCommands.Function cmd) =>
            {
                Context.Child("FunctionManager").Tell(cmd);
            });
            Receive<NewProducer>(cmd =>
            {
                var t = Regex.Replace(cmd.ProducerConfiguration.TopicName, @"[^\w\d]", "");
                var child = Context.Child(t);
                if (child.IsNobody())
                    child = Context.ActorOf(TopicManager.Prop(_config, _network), t);
                child.Tell(cmd);
            });

            Receive<NewReader>(cmd =>
            {
                var t = Regex.Replace(cmd.ReaderConfiguration.TopicName, @"[^\w\d]", "");
                var child = Context.Child(t);
                if (child.IsNobody())
                    child = Context.ActorOf(TopicManager.Prop(_config, _network), t);
                child.Tell(cmd);
            });
            
        }
        
        private void NetworkSetup()
        {
            _network = Context.ActorOf(NetworkManager.Prop(Self, _config), "NetworkManager");
            Receive<ConnectedServerInfo>(s =>
            {
                Context.ActorOf(ConsumerManager.Prop(_config, _network), "ConsumerManager");
                Context.ActorOf(SqlManager.Prop(), "SqlManager");
                var serverLists = _serviceNameResolver.AddressList().Select(x => $"{_config.WebServiceScheme}://{x.Host}:{_config.WebServicePort}").ToArray();
                Context.ActorOf(AdminManager.Prop(new AdminConfiguration {BrokerWebServiceUrl = serverLists}), "AdminManager");
                Context.ActorOf(FunctionManager.Prop(new FunctionConfiguration { BrokerWebServiceUrl = serverLists}), "FunctionManager");
                Become(Ready);
                Stash.UnstashAll();
            });
            ReceiveAny(c=> Stash.Stash());
        }
        public static Props Prop(ClientConfigurationData conf)
        {
            return Props.Create(()=> new PulsarManager(conf));
        }

        public IStash Stash { get; set; }
    }
}
