using System.Text.RegularExpressions;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Reader
{
    public class ReaderManager:ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _network;
        private ClientConfigurationData _config;
        private readonly IActorRef _pulsarManager;
        public ReaderManager(ClientConfigurationData configuration, IActorRef network, IActorRef pulsarManager)
        {
            _pulsarManager = pulsarManager;
            _network = network;
            _config = configuration;
            Receive<NewReader>(NewReader);
            Receive<CloseConsumer>(c =>
            {
                Context.Stop(Sender);//stop the reader and it child 
            });
            ReceiveAny(x =>
            {
                Context.System.Log.Info($"{x.GetType().Name} not supported");
            });
        }
        public static Props Prop(ClientConfigurationData clientConfiguration, IActorRef network, IActorRef pulsarManager)
        {
            return Props.Create(() => new ReaderManager(clientConfiguration, network, pulsarManager));
        }
        
        private void NewReader(NewReader reader)
        {
            var clientConfig = reader.Configuration;
            var readerConfig = reader.ReaderConfiguration;
            var r = Regex.Replace(readerConfig.ReaderName, @"[^\w\d]", "");
            if (!Context.Child(r).IsNobody())
            {
                readerConfig.EventListener.Log($"Reader with name '{r}' already exist for topic '{readerConfig.TopicName}'");
                return;
            }
            Context.ActorOf(Reader.Prop(clientConfig, readerConfig, _network, reader.Seek, _pulsarManager), r);
        }
        
        public IStash Stash { get; set; }
    }
}
