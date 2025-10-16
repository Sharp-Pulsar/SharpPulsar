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
using SharpPulsar.API;

namespace SharpPulsar
{
    public class PulsarSystem : IDisposable
    {
        static PulsarSystem()
        {
            // Unify unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        private static readonly Nito.AsyncEx.AsyncLock _lock = new Nito.AsyncEx.AsyncLock();
        private static ActorSystem _actorSystem;
        private readonly ClientConfigurationData _conf = new();
        private readonly List<IActorRef> _actorRefs= new List<IActorRef>();
        
        public PulsarSystem(IServiceProvider serviceProvider)
        {
            _actorSystem = serviceProvider.GetRequiredService<ActorSystem>();
        }
        public static async ValueTask<IPulsarClient> NewClient(string pulsarClientName,  ClientConfigurationData conf)
        {
            var pool = _actorSystem.ActorOf(Client.ConnectionPool.Prop(conf), $"ConnectionPool-{pulsarClientName}");

            var generator = _actorSystem.ActorOf(IdGeneratorActor.Prop(), $"IdGenerator-{pulsarClientName}");

            var lookup = _actorSystem.ActorOf(BinaryProtoLookupService.Prop(pool, generator, conf.ServiceUrl, conf.ListenerName,
                conf.UseTls, conf.MaxLookupRequest, conf.OperationTimeout, conf.ClientCnx), $"BinaryProtoLookupService-{pulsarClientName}");

            var client = _actorSystem.ActorOf(Props.Create(() => new PulsarClientActor(conf, pool, lookup, generator)), pulsarClientName);
            lookup.Tell(new SetClient(client));


            var clientS = new PulsarClient(client, lookup, pool, generator, conf, _actorSystem);
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
                    tcClient = _actorSystem.ActorOf(TransactionCoordinatorClient.Prop(client, lookup, pool, generator, conf, tcs), $"transaction_coord_client-{pulsarClientName}");
                    var count = await tcs.Task.ConfigureAwait(false);
                    if ((int)count <= 0)
                        throw new Exception($"Tranaction Coordinator has '{count}' transaction handler");
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
