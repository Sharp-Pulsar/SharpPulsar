using Akka.Actor;
using Akka.Configuration;
using NLog;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Client;
using SharpPulsar.Sql;
using SharpPulsar.Sql.Live;
using SharpPulsar.Transaction;
using SharpPulsar.User;
using SharpPulsar.User.Events;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SharpPulsar
{
    public sealed class PulsarSystem
    {
        private static PulsarSystem _instance;
        private static readonly object Lock = new object();
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
        public static PulsarSystem GetInstance(PulsarClientConfigBuilder conf, Action logSetup = null, Config config = null)
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new PulsarSystem(conf, logSetup, config);
                    }
                }
            }
            return _instance;
        }
        private PulsarSystem(PulsarClientConfigBuilder confBuilder, Action logSetup, Config confg)
        {

            _conf = confBuilder.ClientConfigurationData;
            var conf = _conf;
            var logging = logSetup ?? _logSetup;
            logging();
            _conf = conf;
            var config = confg ?? ConfigurationFactory.ParseString(@"
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
            {
                _tcClient = _actorSystem.ActorOf(TransactionCoordinatorClient.Prop(_lookup, _cnxPool, _generator, conf));
                var cos = _tcClient.Ask<int>("Start").GetAwaiter().GetResult();
                if (cos <= 0)
                    throw new Exception($"Tranaction Coordinator has '{cos}' transaction handler");
            } 
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
        public static User.Admin Admin(string brokeWebServiceUrl, HttpClient httpClient, bool disposeHttpClient) 
        {
            return new User.Admin(brokeWebServiceUrl, httpClient, disposeHttpClient);
        }
        public static User.Admin Admin(string brokerWebServiceUrl, params DelegatingHandler[] handlers) 
        {
            return new User.Admin(brokerWebServiceUrl, handlers);
        }
        public static User.Admin Admin(string brokerwebserviceurl, HttpClientHandler rootHandler, params DelegatingHandler[] handlers) 
        {
            return new User.Admin(brokerwebserviceurl, rootHandler, handlers);
        }
        public static User.Function Function(HttpClient httpClient) 
        {
            return new User.Function(httpClient);
        }
        public EventSourceBuilder EventSource(string tenant, string @namespace, string topic, long fromSequenceId, long toSequenceId, string brokerWebServiceUrl) 
        {
            return new EventSourceBuilder(_actorSystem, _client, _lookup, _cnxPool, _generator, tenant, @namespace, topic, fromSequenceId, toSequenceId, brokerWebServiceUrl);
        }
        public static Sql<SqlData> NewSql(ActorSystem actorSystem = null) 
        {
            if(actorSystem != null)
                return Sql<SqlData>.NewSql(actorSystem);

            return Sql<SqlData>.NewSql(_actorSystem);
        }
        public static Sql<LiveSqlData> NewLiveSql(LiveSqlQuery data) 
        {
            var hasQuery = !string.IsNullOrWhiteSpace(data.ClientOptions.Execute);

            if (string.IsNullOrWhiteSpace(data.ClientOptions.Server) || data.ExceptionHandler == null || string.IsNullOrWhiteSpace(data.ClientOptions.Execute) || data.Log == null || string.IsNullOrWhiteSpace(data.Topic))
                throw new ArgumentException("'Sql' is in an invalid state: null field not allowed");

            if (hasQuery)
            {
                data.ClientOptions.Execute.TrimEnd(';');
            }
            else
            {
                data.ClientOptions.Execute = File.ReadAllText(data.ClientOptions.File);
            }

            if (!data.ClientOptions.Execute.Contains("__publish_time__ > {time}"))
            {
                if (data.ClientOptions.Execute.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("add '__publish_time__ > {time}' to where clause");
                }
                throw new ArgumentException("add 'where __publish_time__ > {time}' to '" + data.ClientOptions.Execute + "'");
            }
            if (!TopicName.IsValid(data.Topic))
                throw new ArgumentException($"Topic '{data.Topic}' failed validation");

            return Sql<LiveSqlData>.NewLiveSql(_actorSystem, new LiveSqlSession(data.ClientOptions.ToClientSession(), data.ClientOptions, data.Frequency, data.StartAtPublishTime, TopicName.Get(data.Topic).ToString(), data.Log, data.ExceptionHandler));
        }
        public async Task Shutdown()
        {
            await _actorSystem.Terminate();
        }
    }
}
