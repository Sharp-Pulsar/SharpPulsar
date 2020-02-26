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
        private readonly int _connections;
        private ClientConfigurationData _configuration;
        private EndPoint _endPoint;
        private Dictionary<string, IActorRef> connections = new Dictionary<string, IActorRef>();
        private readonly Random _randomNumber;
        private int _protocolVersion;
        private IActorRef _outerActorRef;
        public HostManager(EndPoint endPoint, int numberofconnection, ClientConfigurationData con, int version, IActorRef outerActorRef)
        {
            _outerActorRef = outerActorRef;
            _protocolVersion = version;
            _configuration = con;
            _connections = numberofconnection;
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
                var n = _randomNumber.Next(0, _connections - 1);
                var actor = connections.Values.ToList()[n];
                actor.Tell(pay);
            });
        }

        protected override void PreStart()
        {
            for (var i = 0; i < _connections; i++)
            {
                Context.ActorOf(ClientConnection.Prop(_endPoint, _configuration, _protocolVersion, _outerActorRef), $"{Context.Parent.Path.Name}-tcp-connection-{i}");
            }
        }

        public static Props Prop(IPEndPoint endPoint, int numberofconnection, ClientConfigurationData con, int version, IActorRef outerActorRef)
        {
            return Props.Create(()=> new HostManager(endPoint, numberofconnection, con, version, outerActorRef));
        }
    }
}
