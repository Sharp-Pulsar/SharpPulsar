using SharpPulsar.Common.PulsarApi;
using System;
using System.Linq;

namespace SharpPulsar.Command.Builder
{
    public class CommandAuthChallengeBuilder
    {
        private readonly CommandAuthChallenge _challenge;
        public CommandAuthChallengeBuilder()
        {
            _challenge = new CommandAuthChallenge();
        }
        
        public CommandAuthChallengeBuilder(CommandAuthChallenge challenge)
        {
            _challenge = challenge;
        }
        public  CommandAuthChallengeBuilder SetProtocolVersion(int clientProtocolVersion)
        {
            // If the broker supports a newer version of the protocol, it will anyway advertise the max version that the
            // client supports, to avoid confusing the client.
            int currentProtocolVersion = CurrentProtocolVersion;
            int versionToAdvertise = Math.Min(currentProtocolVersion, clientProtocolVersion);
            _challenge.ProtocolVersion = versionToAdvertise;
            return new CommandAuthChallengeBuilder(_challenge);
        }
        public  CommandAuthChallengeBuilder SetChallenge(string authMethod, AuthData brokerData)
        {
            var authData = new AuthData
			{
				auth_data = brokerData.auth_data,
				AuthMethodName = authMethod
			};
            _challenge.Challenge = authData;
            return new CommandAuthChallengeBuilder(_challenge);
        }
        public  CommandAuthChallenge Build()
        {
            return _challenge;
        }
        private  int CurrentProtocolVersion
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
