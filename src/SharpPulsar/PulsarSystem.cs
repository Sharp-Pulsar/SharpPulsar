using Akka.Actor;
using Akka.Configuration;
using Serilog;
using SharpPulsar.Configuration;
using SharpPulsar.Messages.Client;
using System;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Events;
using SharpPulsar.TransactionImpl;
using SharpPulsar.Trino;
using System.Collections.Generic;
using SharpPulsar.Client;
using SharpPulsar.Messages;
using Akka.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace SharpPulsar
{
    public abstract class PulsarSystem : IDisposable
    {
        static PulsarSystem()
        {
            // Unify unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        private int _newClient = 0;
        private static PulsarSystem _instance;
        private static readonly Nito.AsyncEx.AsyncLock _lock = new Nito.AsyncEx.AsyncLock();
        private static ActorSystem _actorSystem;
        private readonly ClientConfigurationData _conf = new();
        private readonly List<IActorRef> _actorRefs= new List<IActorRef>();
        private readonly Action _logSetup = () => 
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs.log", rollingInterval: RollingInterval.Hour)
                .MinimumLevel.Information()
                .CreateLogger();
        };
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actorSystem"></param>
        /// <param name="actorSystemName"></param>
        /// <returns></returns>
        public static PulsarSystem GetInstance(ActorSystem actorSystem, string actorSystemName = "apache-pulsar")
        {
            if (_instance == null)
            {
                using (_lock.Lock())
                {
                    if (_instance == null)
                    {
                        _instance = CreateActorSystem(null, null, false, actorSystemName, actorSystem);
                    }
                }
            }
            return _instance;
        }
        public static PulsarSystem GetInstance(Action logSetup = null, Config config = null, string actorSystemName = "apache-pulsar")
        {
            if (_instance == null)
            {
                using (_lock.Lock())
                {
                    if (_instance == null)
                    {
                        _instance = CreateActorSystem(logSetup, config, true, actorSystemName);

                    }
                }
            }
            return _instance;
        }
       
        private static PulsarSystem CreateActorSystem(Action logSetup, Config config, bool runLogSetup, string actorSystemName, ActorSystem actorsystem = null)
        {
            var confg = config ?? ConfigurationFactory.ParseString(@"
            akka
            {
                log-dead-letters = off
                loglevel = INFO
			    log-config-on-start = on 
                loggers=[""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
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

            
            var actorSystem = actorsystem ?? ActorSystem.Create(actorSystemName, confg);
            return new PulsarSystem(actorSystem, logSetup, runLogSetup);
        }
        
        private PulsarSystem(ActorSystem actorSystem, Action logSetup, bool runLogSetup)
        {
            _actorSystem = actorSystem;
            if (runLogSetup)
            {
                var logging = logSetup ?? _logSetup;
                logging();
            }
           
        }
        
        public static async ValueTask<PulsarClient> Client(IServiceProvider serviceProvider, ActorRegistry registry, ClientConfigurationData conf)
        {
            var actorSystem = serviceProvider.GetRequiredService<ActorSystem>();
            var pool = registry.Get<Client.ConnectionPool>();
            var generator = registry.Get<IdGeneratorActor>();
            var lookup = registry.Get<BinaryProtoLookupService>();
            var client = registry.Get<PulsarClientActor>();
            
            var clientS = new PulsarClient(client, lookup, pool, generator, conf, actorSystem);
            if (conf.ServiceUrlProvider != null)
            {
                conf.ServiceUrlProvider.Initialize(clientS);
            }

            IActorRef tcClient = ActorRefs.Nobody;
            if (conf.EnableTransaction)
            {
                try
                {
                    var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                    tcClient = actorSystem.ActorOf(TransactionCoordinatorClient.Prop(client, lookup, pool, generator, conf, tcs), $"transaction_coord_client");
                    var count = await tcs.Task.ConfigureAwait(false);
                    if ((int)count <= 0)
                        throw new Exception($"Tranaction Coordinator has '{count}' transaction handler");
                    registry.TryRegister<TransactionCoordinatorClient>(tcClient);
                    client.Tell(new SetTcClient(tcClient));
                }
                catch
                {
                    tcClient.Tell(PoisonPill.Instance);
                    throw;
                }
            }
            clientS.TransactionCoordinatorClient(tcClient);
            return clientS;
        }
        public EventSourceBuilder EventSource(PulsarClient client, string tenant, string @namespace, string topic, long fromMessageId, long toMessageId, string brokerWebServiceUrl) 
        {
            return new EventSourceBuilder(client.ActorSystem, client.Client, client.Lookup, client.CnxPool, client.Generator, tenant, @namespace, topic, fromMessageId, toMessageId, brokerWebServiceUrl);
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
        public ClientConfigurationData ClientConfigurationData => _conf;
        public async Task Shutdown()
        {
            await _actorSystem.Terminate();
        }
        public void Dispose()
        {
            foreach(var c in _actorRefs) 
                EnsureStopped(c);

            _actorSystem.Dispose();
            _actorSystem.WhenTerminated.Wait();
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            UtilityActor.Log("UnhandledException", AkkaLogLevel.Fatal, e.ExceptionObject);
        }
        public void EnsureStopped(IActorRef actor)
        {
            using Inbox inbox = Inbox.Create(_actorSystem);
            inbox.Watch(actor);
            _actorSystem.Stop(actor);
            inbox.Receive(TimeSpan.FromMinutes(5));
        }
    }
}
