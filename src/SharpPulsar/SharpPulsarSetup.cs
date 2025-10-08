using Akka.Actor;
using System.Threading.Tasks;
using System;
using Akka.Hosting;
using Microsoft.Extensions.Hosting;
using SharpPulsar.Configuration;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages;
using SharpPulsar.TransactionImpl;
using SharpPulsar.Client;

namespace SharpPulsar
{
    public static class SharpPulsarSetup
    {
        public static void AddSharpPulsarSetup(this IHostBuilder hostBuilder, ClientConfigurationData conf, out PulsarClient pulsarClient)
        {
            PulsarClient _client = null;
            hostBuilder.ConfigureServices((ctx, services) =>
            {
                services.AddAkka("PulsarSystem", (configurationBuilder, serviceProvider) =>
                 {
                     configurationBuilder
                         .ConfigureLoggers(setup =>
                         {
                             // This sets the minimum log level
                             setup.LogLevel = LogLevel.DebugLevel;

                             // Clear all loggers (remove the default console logger)
                             setup.ClearLoggers();

                             // Add the ILoggerFactory logger
                             // NOTE:
                             //   - You can also use setup.AddLogger<LoggerFactoryLogger>();
                             //   - To use a specific ILoggerFactory instance, you can use setup.AddLoggerFactory(myILoggerFactory);
                             setup.AddLoggerFactory();
                         })
                         .WithActors((system, registry) =>
                         {
                             if (conf.ServiceUrlProvider != null)
                             {
                                 conf.ServiceUrlProvider.CreateActor(system);
                             }

                             var pool = system.ActorOf(Client.ConnectionPool.Prop(conf), $"ConnectionPool");
                             registry.TryRegister<Client.ConnectionPool>(pool);
                         })
                         .WithActors((system, registry) =>
                         {
                             var generator = system.ActorOf(IdGeneratorActor.Prop(), $"IdGenerator");
                             registry.TryRegister<IdGeneratorActor>(generator);
                         })
                         .WithActors((system, registry) =>
                         {
                             var pool = registry.Get<Client.ConnectionPool>();
                             var generator = registry.Get<IdGeneratorActor>();

                             var lookup = system.ActorOf(BinaryProtoLookupService.Prop(pool, generator, conf.ServiceUrl, conf.ListenerName,
                                 conf.UseTls, conf.MaxLookupRequest, conf.OperationTimeout, conf.ClientCnx), $"BinaryProtoLookupService");
                             registry.TryRegister<BinaryProtoLookupService>(lookup);
                         })
                         .WithActors((system, registry) =>
                         {
                             var pool = registry.Get<Client.ConnectionPool>();
                             var generator = registry.Get<IdGeneratorActor>();
                             var lookup = registry.Get<BinaryProtoLookupService>();

                             var client = system.ActorOf(Props.Create(() => new PulsarClientActor(conf, pool, lookup, generator)), $"PulsarClient");
                             registry.TryRegister<PulsarClientActor>(client);
                             lookup.Tell(new SetClient(client));
                         })
                         .WithActors((system, registry) =>
                         {
                             var pool = registry.Get<Client.ConnectionPool>();
                             var generator = registry.Get<IdGeneratorActor>();
                             var lookup = registry.Get<BinaryProtoLookupService>();
                             var client = registry.Get<PulsarClientActor>();
                             
                             _client = new PulsarClient(client, lookup, pool, generator, conf, system);
                             if (conf.ServiceUrlProvider != null)
                             {
                                 conf.ServiceUrlProvider.Initialize(_client);
                             }

                             IActorRef tcClient = ActorRefs.Nobody;
                             if (conf.EnableTransaction)
                             {
                                 try
                                 {
                                     var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                                     tcClient = system.ActorOf(TransactionCoordinatorClient.Prop(client, lookup, pool, generator, conf, tcs), $"transaction_coord_client");
                                     var count = tcs.Task.GetAwaiter().GetResult();
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
                             _client.TransactionCoordinatorClient(tcClient);
                         });
                 });
            });
            pulsarClient = _client;
        }
    }
}
