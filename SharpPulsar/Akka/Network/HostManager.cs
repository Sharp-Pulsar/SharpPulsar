using System;
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
            Receive<ConnectedServerInfo>(f =>
            {
                try
                {
                    if(!connections.ContainsKey(f.Name))
                        connections.Add(f.Name, Sender);
                    Context.Parent.Forward(f);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            Receive<Payload>(pay =>
            {
                var n = _randomNumber.Next(0, _configuration.ConnectionsPerBroker - 1);
                var actor = connections.Values.ToList()[n];
                actor.Forward(pay);
            });
        }

        protected override void Unhandled(object message)
        {
            Console.WriteLine($"Unhandled {message.GetType()} in {Self.Path}");
        }

        protected override void PreStart()
        {
            for (var i = 0; i < _configuration.ConnectionsPerBroker; i++)
            {
                Context.ActorOf(ClientConnection.Prop(_endPoint, _configuration,_manager), $"{i}");
            }
        }

        public static Props Prop(IPEndPoint endPoint, ClientConfigurationData con, IActorRef manager)
        {
            return Props.Create(()=> new HostManager(endPoint, con, manager));
        }
    }
}
