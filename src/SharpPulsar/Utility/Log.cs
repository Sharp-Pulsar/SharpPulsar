using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SharpPulsar.Utility
{
    public class Log
    {
        public static ILoggerFactory Logger { get; set; } = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddConsole();
        });
    }
}
