using SharpPulsar.Common.PulsarApi;
using System;
using System.Linq;

namespace SharpPulsar.Command.Builder
{
    public class CommandConnectedBuilder
    {
        private static CommandConnected _connected;
        public const int INVALID_MAX_MESSAGE_SIZE = -1;
        public CommandConnectedBuilder()
        {
            _connected = new CommandConnected();
        }
        private CommandConnectedBuilder(CommandConnected connected)
        {
            _connected = connected;
        }       
        public static CommandConnectedBuilder SetServerVersion(string serverVersion)
        {
            _connected.ServerVersion = serverVersion;
            return new CommandConnectedBuilder(_connected);
        }
        public static CommandConnectedBuilder SetMaxMessageSize(int maxMessageSize)
        {
            if (INVALID_MAX_MESSAGE_SIZE != maxMessageSize)
            {
                _connected.MaxMessageSize = maxMessageSize;
            }
            return new CommandConnectedBuilder(_connected);
        }
        public static CommandConnectedBuilder SetProtocolVersion(int clientProtocolVersion)
        {
            int currentProtocolVersion = CurrentProtocolVersion;
            int versionToAdvertise = Math.Min(currentProtocolVersion, clientProtocolVersion);
            _connected.ProtocolVersion = versionToAdvertise;
            return new CommandConnectedBuilder(_connected);
        }
        private static int CurrentProtocolVersion
        {
            get
            {
                // Return the last ProtocolVersion enum value
                var version = System.Enum.GetValues(typeof(ProtocolVersion)).Cast<int>().Last();
                //return ProtocolVersion.values()[PulsarApi.ProtocolVersion.values().length - 1].Number;
                return version;
            }
        }
    }
}
