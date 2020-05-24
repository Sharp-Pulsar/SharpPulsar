using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly IActorRef _pulsarManager;
        public ProducerBroadcastGroup(IActorRef producerManager, IActorRef pulsarManager)
        {
            _pulsarManager = pulsarManager;
            _routees = new List<string>();
            _producerManager = producerManager;
            Become(Waiting);
        }

        private void Waiting()
        {
            Receive<NewProducerBroadcastGroup>(cmd =>
            {
                _expectedRouteeCount = cmd.ProducerConfigurations.Count;
                _configuration = cmd.ProducerConfigurations.First();
                var name = $"broadcaster{Regex.Replace(cmd.Title, @"[^\w\d]", "")}".ToLower();
                var child = Context.Child(name);
                Become(() => CreatingProducers(cmd, child, name));
            });
        }

        private void CreatingProducers(NewProducerBroadcastGroup cmd, IActorRef child, string name)
        {
            Receive<RegisteredProducer>(p =>
            {
                
                _currentRouteeCount++;
                if (p.IsNew)
                {
                    if (child.IsNobody())
                    {
                        _routees.Add(Sender.Path.ToString());
                        if (_currentRouteeCount == _expectedRouteeCount)
                        {
                            var router = Context.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(_routees)), $"BroadcastRouter{cmd.Title}".ToLower());
                            var broadcaster = Context.ActorOf(BroadcastRouter.Prop(router), name);
                            _pulsarManager.Tell(new CreatedProducer(broadcaster, cmd.Title, _configuration.ProducerName, true));
                            _routees.Clear();
                            _currentRouteeCount = 0;
                            Become(Waiting);
                            Stash.UnstashAll();
                        }
                    }
                    else
                    {
                        var router = Context.Child($"BroadcastRouter{cmd.Title}".ToLower());
                        var routee = Routee.FromActorRef(Sender);
                        router.Tell(new AddRoutee(routee));
                        if (_currentRouteeCount == _expectedRouteeCount)
                        {
                            _pulsarManager.Tell(new CreatedProducer(child, cmd.Title, _configuration.ProducerName, true));
                            _currentRouteeCount = 0;
                            Become(Waiting);
                            Stash.UnstashAll();
                        }
                    }
                    
                }
                else
                {
                    if (_currentRouteeCount == _expectedRouteeCount)
                    {
                        _pulsarManager.Tell(new CreatedProducer(child, cmd.Title, _configuration.ProducerName, true));
                        _currentRouteeCount = 0;
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
        public static Props Prop(IActorRef producerManager, IActorRef pulsarManager)
        {
            return Props.Create(()=> new ProducerBroadcastGroup(producerManager, pulsarManager));
        }
        public IStash Stash { get ; set ; }
    }
}
