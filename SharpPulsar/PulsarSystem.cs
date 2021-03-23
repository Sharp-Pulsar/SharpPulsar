using Akka.Actor;
using Akka.Configuration;
using NLog;
using SharpPulsar.Configuration;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Transaction;
using SharpPulsar.User;
using System.Threading.Tasks;

namespace SharpPulsar
{
    public sealed class PulsarSystem
    {
        private static PulsarSystem _instance;
        private static readonly object Lock = new object();
        private readonly ActorSystem _actorSystem;
        private readonly ClientConfigurationData _conf;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _tcClient;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        public static PulsarSystem GetInstance(ActorSystem actorSystem, PulsarClientConfigBuilder conf)
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new PulsarSystem(actorSystem, conf);
                    }
                }
            }
            return _instance;
        }
        public static PulsarSystem GetInstance(PulsarClientConfigBuilder conf, NLog.Config.LoggingConfiguration loggingConfiguration = null)
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new PulsarSystem(conf, loggingConfiguration);
                    }
                }
            }
            return _instance;
        }
        private PulsarSystem(PulsarClientConfigBuilder confBuilder, NLog.Config.LoggingConfiguration loggingConfiguration)
        {

            _conf = confBuilder.ClientConfigurationData;
            var conf = _conf;
            var nlog = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets
                .FileTarget("logFile")
            {
                FileName = "logs.log",
                Layout = "[${longdate}] [${logger}] ${level:uppercase=true}] : ${event-properties:actorPath} ${message} ${exception:format=tostring}",
                ArchiveEvery = NLog.Targets.FileArchivePeriod.Hour,
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.DateAndSequence
            };
            nlog.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            LogManager.Configuration = loggingConfiguration ?? nlog;
            _conf = conf;
            var config = ConfigurationFactory.ParseString(@"
            akka
            {
                loglevel = DEBUG
			    log-config-on-start = on 
                loggers=[""Akka.Logger.NLog.NLogLogger, Akka.Logger.NLog""]
			    actor 
                {              
				      debug 
				      {
					      receive = on
					      autoreceive = on
					      lifecycle = on
					      event-stream = on
					      unhandled = on
				      }  
			    }
                coordinated-shutdown
                {
                    exit-clr = on
                }
            }"
            );
            _actorSystem = ActorSystem.Create("Pulsar", config);
            
            _cnxPool = _actorSystem.ActorOf(ConnectionPool.Prop(conf), "ConnectionPool");
            _generator = _actorSystem.ActorOf(IdGeneratorActor.Prop(), "IdGenerator");
            _lookup = _actorSystem.ActorOf(BinaryProtoLookupService.Prop(_cnxPool, _generator, conf.ServiceUrl, conf.ListenerName, conf.UseTls, conf.MaxLookupRequest, conf.OperationTimeoutMs), "BinaryProtoLookupService");

            if (conf.EnableTransaction)
                _tcClient = _actorSystem.ActorOf(TransactionCoordinatorClient.Prop(_lookup, _cnxPool, _generator, conf));

            _client = _actorSystem.ActorOf(Props.Create(()=> new PulsarClientActor(conf,  _cnxPool, _tcClient, _lookup, _generator)), "PulsarClient");
            _lookup.Tell(new SetClient(_client));

        }
        private PulsarSystem(ActorSystem actorSystem, PulsarClientConfigBuilder confBuilder)
        {
            _conf = confBuilder.ClientConfigurationData;
            var conf = _conf;
            _actorSystem = actorSystem;
            _conf = conf;
            _cnxPool = _actorSystem.ActorOf(ConnectionPool.Prop(conf), "ConnectionPool");
            _generator = _actorSystem.ActorOf(IdGeneratorActor.Prop(), "IdGenerator");
            _lookup = _actorSystem.ActorOf(BinaryProtoLookupService.Prop(_cnxPool, _generator, conf.ServiceUrl, conf.ListenerName, conf.UseTls, conf.MaxLookupRequest, conf.OperationTimeoutMs), "BinaryProtoLookupService");

            if (conf.EnableTransaction)
                _tcClient = _actorSystem.ActorOf(TransactionCoordinatorClient.Prop(_lookup, _cnxPool, _generator, conf));

            _client = _actorSystem.ActorOf(Props.Create<PulsarClientActor>(conf, _cnxPool, _tcClient, _lookup, _generator), "PulsarClient");
            _lookup.Tell(new SetClient(_client));
        }
        public PulsarClient NewClient() 
        {
            return new PulsarClient(_client, _lookup, _cnxPool, _generator, _conf, _actorSystem, _tcClient);
        }
        public User.Admin Admin() 
        {
            return null;
        }
        public User.Function Function() 
        {
            return null;
        }
        public EventSource NewEventSource() 
        {
            return null;
        }
        public User.Sql NewSql() 
        {
            return null;
        }
        public async Task Shutdown()
        {
            await _actorSystem.Terminate();
        }
    }
}
