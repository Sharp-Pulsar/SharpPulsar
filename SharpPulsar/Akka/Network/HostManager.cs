using System;
using System.Net;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Network
{
    public class HostManager:ReceiveActor, IWithUnboundedStash
    {
        private ClientConfigurationData _configuration;
        private EndPoint _endPoint;
        private IActorRef _tcpActor;
        private IActorRef _manager;
        public HostManager(EndPoint endPoint, ClientConfigurationData con, IActorRef manager)
        {
            _manager= manager;
            _configuration = con;
            _endPoint = endPoint;
            ReceiveAny(_=> Stash.Stash());
            Become(Awaiting);
        }

        private void Awaiting()
        {
            Context.ActorOf(ClientConnection.Prop(_endPoint, _configuration, _manager), "hostConnection");
            Receive<ConnectedServerInfo>(f =>
            {
                try
                {
                    _tcpActor = Sender;
                    Context.Parent.Tell(f);
                    Become(Open);
                    Stash.UnstashAll();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
        }

        private void Open()
        {
            Receive<Payload>(pay =>
            {
                _tcpActor.Forward(pay);
            });
        }
        protected override void Unhandled(object message)
        {
            Console.WriteLine($"Unhandled {message.GetType()} in {Self.Path}");
        }


        public static Props Prop(IPEndPoint endPoint, ClientConfigurationData con, IActorRef manager)
        {
            return Props.Create(()=> new HostManager(endPoint, con, manager));
        }

        public IStash Stash { get; set; }
    }
}
