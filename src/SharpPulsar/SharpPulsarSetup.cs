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
using Akka.Configuration;
using SharpPulsar.API;
using System.Collections.Generic;

namespace SharpPulsar
{
    public static class SharpPulsarSetup
    {
        //private static AkkaConfigurationBuilder _builder;
        public static void AddSharpPulsarSetup(this IHostBuilder hostBuilder, IDictionary<string, ClientConfigurationData> confs, out List<IPulsarClient> pulsarClients, Config config = null)
        {
            List<IPulsarClient> _pulsarClients = new List<IPulsarClient>();  
            var _config = config ?? ConfigurationFactory.ParseString(@"
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
            hostBuilder.ConfigureServices((ctx, services) =>
            {
                 services.AddAkka("PulsarSystem", (configurationBuilder, serviceProvider) =>
                 {

                     configurationBuilder
                         .AddHocon(_config, HoconAddMode.Append)
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
                         });
                     foreach (var conf in confs)
                     {
                         configurationBuilder.AddPulsarClient(conf.Key, conf.Value, out IPulsarClient pulsarClient);
                         _pulsarClients.Add(pulsarClient);
                     }
                 });
            });
            pulsarClients = _pulsarClients;
        }
        public static AkkaConfigurationBuilder AddPulsarClient(this AkkaConfigurationBuilder builder,  string pulsarClientName, ClientConfigurationData conf, out IPulsarClient pulsarClient)
        {
            IPulsarClient _client = null;
            builder.WithActors((system, registry) =>
            {
                if (conf.ServiceUrlProvider != null)
                {
                    conf.ServiceUrlProvider.CreateActor(system);
                }

                var pool = system.ActorOf(Client.ConnectionPool.Prop(conf), $"ConnectionPool-{pulsarClientName}");

                var generator = system.ActorOf(IdGeneratorActor.Prop(), $"IdGenerator-{pulsarClientName}");

                var lookup = system.ActorOf(BinaryProtoLookupService.Prop(pool, generator, conf.ServiceUrl, conf.ListenerName,
                    conf.UseTls, conf.MaxLookupRequest, conf.OperationTimeout, conf.ClientCnx), $"BinaryProtoLookupService-{pulsarClientName}");

                var client = system.ActorOf(Props.Create(() => new PulsarClientActor(conf, pool, lookup, generator)), pulsarClientName);
                lookup.Tell(new SetClient(client));

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
                        tcClient = system.ActorOf(TransactionCoordinatorClient.Prop(client, lookup, pool, generator, conf, tcs), $"transaction_coord_client-{pulsarClientName}");
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
                //registry.TryRegister<Client.ConnectionPool>(pool);
            });
            
            pulsarClient = _client;
            return builder;
        }        
    }
}
