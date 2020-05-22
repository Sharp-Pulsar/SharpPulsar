using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Routing;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Producer
{
    public class ProducerBroadcastGroup : ReceiveActor, IWithUnboundedStash
    {
        private ProducerConfigurationData _configuration;
        private readonly IActorRef _producerManager;
        private readonly List<string> _routees;
        private int _expectedRouteeCount;
        private int _currentRouteeCount;
        private bool _canCreate;
        public ProducerBroadcastGroup(IActorRef producerManager)
        {
            _routees = new List<string>();
            _producerManager = producerManager;
            Become(Waiting);
        }

        private void Waiting()
        {
            Receive<NewProducerBroadcastGroup>(cmd =>
            {
                _canCreate = true;
                _expectedRouteeCount = cmd.ProducerConfigurations.Count;
                _configuration = cmd.ProducerConfigurations.First();
                Become(() => CreatingProducers(cmd));
            });
        }

        private void CreatingProducers(NewProducerBroadcastGroup cmd)
        {
            var m = cmd;
            Receive<RegisteredProducer>(p =>
            {
                _currentRouteeCount++;
                if (p.IsNew && _canCreate)
                {
                    _routees.Add(Sender.Path.ToString());
                    if (_currentRouteeCount == _expectedRouteeCount)
                    {
                        var router = Context.System.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(_routees)), $"Broadcast{DateTimeHelper.CurrentUnixTimeMillis()}");
                        var broadcaster = Context.ActorOf(BroadcastRouter.Prop(router));
                        _configuration.ProducerEventListener.ProducerCreated(new CreatedProducer(broadcaster, cmd.Title, _configuration.ProducerName, true));
                        _routees.Clear();
                        _currentRouteeCount = 0;
                        Become(Waiting);
                        Stash.UnstashAll();
                    }
                }
                else
                {
                    _canCreate = false;
                    if (_currentRouteeCount == _expectedRouteeCount)
                    {
                        _configuration.ProducerEventListener.ProducerCreated(new CreatedProducer(null, cmd.Title, _configuration.ProducerName, true));
                        _currentRouteeCount = 0;
                        _routees.Clear();
                        Become(Waiting);
                        Stash.UnstashAll();
                    }
                }

            });
            Receive<PulsarError>(e =>
            {
                _currentRouteeCount++;
                _configuration.ProducerEventListener.Log($"{e.Error}: {e.Message}");
                Become(Waiting);
                Stash.UnstashAll();
            });
            ReceiveAny(_=> Stash.Stash());
            foreach (var t in cmd.ProducerConfigurations)
            {
                var p = new NewProducerGroupMember(t.Schema, cmd.Configuration, t, cmd.Title);
                _producerManager.Tell(p);
            }
        }
        public static Props Prop(IActorRef producerManager)
        {
            return Props.Create(()=> new ProducerBroadcastGroup(producerManager));
        }
        public IStash Stash { get ; set ; }
    }
}
