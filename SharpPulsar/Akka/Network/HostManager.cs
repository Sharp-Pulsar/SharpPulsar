using System;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Network
{
    public class HostManager:ReceiveActor
    {
        private ClientConfigurationData _configuration;
        private Uri _endPoint;
        private IActorRef _tcpActor;
        private IActorRef _manager;
        public HostManager(Uri endPoint, ClientConfigurationData con, IActorRef manager)
        {
            _manager= manager;
            _configuration = con;
            _endPoint = endPoint;
            Receive<ConnectedServerInfo>(f =>
            {
                _tcpActor = Sender;
                Context.Parent.Tell(f);
            });
            Receive<Payload>(pay =>
            {
                _tcpActor.Forward(pay);
            });
        }

        protected override void Unhandled(object message)
        {
            Console.WriteLine($"Unhandled {message.GetType()} in {Self.Path}");
        }

        protected override void PreStart()
        {

            Context.ActorOf(ClientConnection.Prop(_endPoint, _configuration, _manager));
        }

        public static Props Prop(Uri endPoint, ClientConfigurationData con, IActorRef manager)
        {
            return Props.Create(()=> new HostManager(endPoint, con, manager));
        }

    }
}
