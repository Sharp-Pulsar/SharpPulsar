using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SharpPulsar.Util
{
    public class Log
    {
        private static ILogger _logger = new NullLogger<ILogger>();
        public static ILogger Logger
        {
            get
            {
                return _logger;
            }
            set
            {
                _logger = value;
            }
        }

    }
}
