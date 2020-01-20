using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandAuthResponseBuilder
    {
        private static CommandAuthResponse _response;
        public CommandAuthResponseBuilder()
        {
            _response = new CommandAuthResponse();
        }
        private CommandAuthResponseBuilder(CommandAuthResponse response)
        {
            _response = response;
        }
        public static CommandAuthResponseBuilder SetProtocolVersion(int clientProtocolVersion)
        {            
            _response.ProtocolVersion = clientProtocolVersion;
            return new CommandAuthResponseBuilder(_response);
        }
        public static CommandAuthResponseBuilder SetAuthData(string authMethod, AuthData clientData)
        {
            var authData = new AuthData
            {
                auth_data = clientData.auth_data,
                AuthMethodName = authMethod
            };
            _response.Response = authData;
            return new CommandAuthResponseBuilder(_response);
        }
        public static CommandAuthResponseBuilder SetClientVersion(string clientversion)
        {
            _response.ClientVersion = clientversion;
            return new CommandAuthResponseBuilder(_response);
        }
        public static CommandAuthResponse Build()
        {
            if (string.IsNullOrWhiteSpace(_response.ClientVersion))
                _response.ClientVersion = "Pulsar Client";
            return _response;
        }
    }
}
