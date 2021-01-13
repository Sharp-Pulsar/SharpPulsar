using System;
using Akka.Actor;
using SharpPulsar.Messages;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Network
{
    public class HostManager:ReceiveActor, IWithUnboundedStash
    {
        private readonly ClientConfigurationData _configuration;
        private readonly Uri _endPoint;
        private IActorRef _tcpActor;
        private readonly IActorRef _manager;
        public HostManager(Uri endPoint, ClientConfigurationData con, IActorRef manager)
        {
            _manager= manager;
            _configuration = con;
            _endPoint = endPoint;
            var child = Context.ActorOf(ClientConnection.Prop(_endPoint, _configuration, manager));
            Context.Watch(child);
            Become(Online);
        }

        private void IsTerminated()
        {
            ReceiveAny(_=> Stash.UnstashAll());
            var child = Context.ActorOf(ClientConnection.Prop(_endPoint, _configuration, _manager));
            Context.Watch(child);
            Become(Online);
        }
        protected override void Unhandled(object message)
        {
            Console.WriteLine($"Unhandled {message.GetType()} in {Self.Path}");
        }

        private void Online()
        {
            Receive<ConnectedServerInfo>(f =>
            {
                Stash.UnstashAll();
                _tcpActor = Sender;
                Context.Parent.Tell(f);
            });
            Receive<Payload>(pay =>
            {
                _tcpActor.Forward(pay);
            });
            Receive<Terminated>(_=> Become(IsTerminated));
        }

        public static Props Prop(Uri endPoint, ClientConfigurationData con, IActorRef manager)
        {
            return Props.Create(()=> new HostManager(endPoint, con, manager));
        }

        public IStash Stash { get; set; }
    }
}
