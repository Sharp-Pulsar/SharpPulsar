using System;
using Akka.Hosting;
using Microsoft.Extensions.Hosting;
using Akka.Configuration;
using Serilog;

namespace SharpPulsar
{
    public static class SharpPulsarSetup
    {
        private static readonly Action _logSetup = () =>
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs.log", rollingInterval: RollingInterval.Hour)
                .MinimumLevel.Information()
                .CreateLogger();
        };
        //private static AkkaConfigurationBuilder _builder;
        public static void AddSharpPulsarSetup(this IHostBuilder hostBuilder, Action logSetup = null, bool runLogSetup = false, Config config = null)
        {
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
                 });
            });
            if (runLogSetup)
            {
                var logging = logSetup ?? _logSetup;
                logging();
            }
        }
    }
}
