using Akka.Actor;
using Akka.Configuration;
using NLog;
using SharpPulsar.Configuration;
using SharpPulsar.Messages.Client;
using SharpPulsar.Transaction;
using SharpPulsar.User;
using SharpPulsar.User.Events;
using System;
using System.Threading.Tasks;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Public;
using AskResponse = SharpPulsar.Messages.Consumer.AskResponse;
using SharpPulsar.Builder;

namespace SharpPulsar
{
    public sealed class PulsarSystem
    {
        private static PulsarSystem _instance;
        private static readonly Nito.AsyncEx.AsyncLock _lock = new Nito.AsyncEx.AsyncLock();
        private static ActorSystem _actorSystem;
        private readonly ClientConfigurationData _conf;
        private readonly IActorRef _cnxPool;
        private readonly IActorRef _client;
        private readonly IActorRef _tcClient;
        private readonly IActorRef _lookup;
        private readonly IActorRef _generator;
        private readonly Action _logSetup = () => 
        {
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
            LogManager.Configuration = nlog;
        };
        public static PulsarSystem GetInstance(ActorSystem actorSystem, PulsarClientConfigBuilder conf, string actorSystemName = "apache-pulsar")
        {
            return GetInstanceAsync(actorSystem, conf, actorSystemName).GetAwaiter().GetResult();
        }
        public static async Task<PulsarSystem> GetInstanceAsync(ActorSystem actorSystem, PulsarClientConfigBuilder conf, string actorSystemName = "apache-pulsar")
        {
            if (_instance == null)
            {
                using (await _lock.LockAsync().ConfigureAwait(false))
                {
                    if (_instance == null)
                    {
                        _instance = await CreateActorSystem(conf, null, null, false, actorSystemName, actorSystem).ConfigureAwait(false);
                    }
                }
            }
            return _instance;
        }
        public static PulsarSystem GetInstance(PulsarClientConfigBuilder conf, Action logSetup = null, Config config = null, string actorSystemName = "apache-pulsar")
        {
            return GetInstanceAsync(conf, logSetup, config, actorSystemName).GetAwaiter().GetResult();
        }
        public static async Task<PulsarSystem> GetInstanceAsync(PulsarClientConfigBuilder conf, Action logSetup = null, Config config = null, string actorSystemName = "apache-pulsar")
        {
            if (_instance == null)
            {
                using (await _lock.LockAsync().ConfigureAwait(false))
                {
                    if (_instance == null)
                    {
                        _instance = await CreateActorSystem(conf, logSetup, config, true, actorSystemName).ConfigureAwait(false);
                        
                    }
                }
            }
            return _instance;
        }
        private static async Task<PulsarSystem> CreateActorSystem(PulsarClientConfigBuilder conf, Action logSetup, Config config, bool runLogSetup, string actorSystemName, ActorSystem actorsystem = null)
        {
            var confg = config ?? ConfigurationFactory.ParseString(@"
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
            }");
            var clientConf = conf.ClientConfigurationData;
            var actorSystem = actorsystem ?? ActorSystem.Create(actorSystemName, confg);

            var cnxPool = actorSystem.ActorOf(ConnectionPool.Prop(clientConf), "ConnectionPool");
            var generator = actorSystem.ActorOf(IdGeneratorActor.Prop(), "IdGenerator");
            var lookup = actorSystem.ActorOf(BinaryProtoLookupService.Prop(cnxPool, generator, clientConf.ServiceUrl, clientConf.ListenerName, clientConf.UseTls, clientConf.MaxLookupRequest, clientConf.OperationTimeout), "BinaryProtoLookupService");
            IActorRef tcClient = ActorRefs.Nobody;
            if (clientConf.EnableTransaction)
            {
                try
                {
                    var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                    tcClient = actorSystem.ActorOf(TransactionCoordinatorClient.Prop(lookup, cnxPool, generator, clientConf, tcs));
                    var count = await tcs.Task.ConfigureAwait(false);
                    if ((int)count <= 0)
                        throw new Exception($"Tranaction Coordinator has '{count}' transaction handler");
                }
                catch
                {
                    tcClient.Tell(PoisonPill.Instance);
                    throw;
                }
            }
            return  new PulsarSystem(actorSystem, clientConf, logSetup, cnxPool, generator, lookup, tcClient, runLogSetup);
        }
        private PulsarSystem(ActorSystem actorSystem, ClientConfigurationData conf, Action logSetup, IActorRef cnxPool, IActorRef generator, IActorRef lookup, IActorRef tcClient, bool runLogSetup)
        {
            _actorSystem = actorSystem;
            _cnxPool = cnxPool;
            _generator = generator; 
            _lookup = lookup;
            _tcClient = tcClient;
            _conf = conf;
            if(runLogSetup)
            {
                var logging = logSetup ?? _logSetup;
                logging();
            }
            _client = _actorSystem.ActorOf(Props.Create(()=> new PulsarClientActor(_conf,  _cnxPool, _tcClient, _lookup, _generator)), "PulsarClient");
            _lookup.Tell(new SetClient(_client));

        }
        public PulsarClient NewClient() 
        {
            var client = new PulsarClient(_client, _lookup, _cnxPool, _generator, _conf, _actorSystem, _tcClient);
            if (_conf.ServiceUrlProvider != null)
            {
                _conf.ServiceUrlProvider.Initialize(client);
            }
            return client;
        }
        public EventSourceBuilder EventSource(string tenant, string @namespace, string topic, long fromMessageId, long toMessageId, string brokerWebServiceUrl) 
        {
            return new EventSourceBuilder(_actorSystem, _client, _lookup, _cnxPool, _generator, tenant, @namespace, topic, fromMessageId, toMessageId, brokerWebServiceUrl);
        }

        public static SqlInstance Sql(ClientOptions options) 
        {
            return new SqlInstance(_actorSystem, options);
        }
        public static SqlInstance Sql(ActorSystem actorSystem, ClientOptions options)
        {
            if (actorSystem == null)
                throw new Exception("ActorSystem can not be null");

            return new SqlInstance(actorSystem, options);
        }
        public static LiveSqlInstance LiveSql(ClientOptions options, string topic, TimeSpan interval, DateTime startAtPublishTime) 
        {
            return new LiveSqlInstance(_actorSystem, options, topic, interval, startAtPublishTime);
        }
        public static LiveSqlInstance LiveSql(ActorSystem actorSystem, ClientOptions options, string topic, TimeSpan interval, DateTime startAtPublishTime)
        {
            if (actorSystem == null)
                throw new Exception("ActorSystem can not be null");
            return new LiveSqlInstance(_actorSystem, options, topic, interval, startAtPublishTime);
        }

        public ActorSystem System => _actorSystem;
        public async Task Shutdown()
        {
            await _actorSystem.Terminate();
        }
    }
}
