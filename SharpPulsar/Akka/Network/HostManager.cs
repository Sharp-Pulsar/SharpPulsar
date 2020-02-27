﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Network
{
    public class HostManager:ReceiveActor
    {
        private ClientConfigurationData _configuration;
        private EndPoint _endPoint;
        private Dictionary<string, IActorRef> connections = new Dictionary<string, IActorRef>();
        private readonly Random _randomNumber;
        private IActorRef _manager;
        public HostManager(EndPoint endPoint, ClientConfigurationData con, IActorRef manager)
        {
            _manager= manager;
            _configuration = con;
            _endPoint = endPoint;
            _randomNumber = new Random();
            Receive<TcpFailed>(f =>
            {
                connections.Remove(f.Name);
                Console.WriteLine($"TCP connection failure from {f.Name}");
            });
            Receive<TcpSuccess>(f =>
            {
                connections.Add(f.Name, Sender);
                Console.WriteLine($"TCP connection success from {f.Name}");
            });
            Receive<Payload>(pay =>
            {
                var n = _randomNumber.Next(0, _configuration.ConnectionsPerBroker - 1);
                var actor = connections.Values.ToList()[n];
                actor.Tell(pay);
            });
        }

        protected override void PreStart()
        {
            for (var i = 0; i < _configuration.ConnectionsPerBroker; i++)
            {
                Context.ActorOf(ClientConnection.Prop(_endPoint, _configuration,_manager), $"{Context.Parent.Path.Name}-tcp-connection-{i}");
            }
        }

        public static Props Prop(IPEndPoint endPoint, ClientConfigurationData con, IActorRef manager)
        {
            return Props.Create(()=> new HostManager(endPoint, con, manager));
        }
    }
}