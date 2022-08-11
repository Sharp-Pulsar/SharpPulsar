using System;
using Akka.Actor;
using Akka.Streams.Actors;

namespace Akka.Persistence.Pulsar.Tests.Kits
{
    public class ManualSubscriber : ActorSubscriber
    {
        public static Props Props(IActorRef probe)
            => Akka.Actor.Props.Create(() => new ManualSubscriber(probe));//.WithDispatcher("akka.test.stream-dispatcher");

        private readonly IActorRef _probe;

        public ManualSubscriber(IActorRef probe)
        {
            _probe = probe;
        }

        public override IRequestStrategy RequestStrategy { get; } = ZeroRequestStrategy.Instance;

        protected override bool Receive(object message)
        {
            return message.Match()
                .With<OnNext>(_probe.Tell)
                .With<OnComplete>(_probe.Tell)
                .With<OnError>(_probe.Tell)
                .With<string>(s =>
                {
                    if (s.StartsWith("ready-"))
                    {
                        var resquest = s.Split("-")[1];
                        Request(Convert.ToInt32(resquest));
                    }
                    else if (s.Equals("boom"))
                        throw new Exception(s);
                    else if (s.Equals("requestAndCancel"))
                    {
                        Request(1);
                        Cancel();
                    }
                    else if (s.Equals("cancel"))
                        Cancel();
                })
                .WasHandled;
        }
    }
}
