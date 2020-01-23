﻿using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandConnectBuilder
    {
        private readonly CommandConnect _connect;
        public CommandConnectBuilder()
        {
            _connect = new CommandConnect();
        }
        
        public  CommandConnectBuilder SetClientVersion(string clientversion)
        {
            _connect.ClientVersion = clientversion;
            return this;
        }
        public  CommandConnectBuilder SetProtocolVersion(int protocolVersion)
        {
            _connect.ProtocolVersion = protocolVersion;
            return this;
        }
        public  CommandConnectBuilder SetProxyToBrokerUrl(string proxyToBrokerUrl)
        {
            // When connecting through a proxy, we need to specify which broker do we want to be proxied through
            _connect.ProxyToBrokerUrl = proxyToBrokerUrl;
            return this;
        }
        public  CommandConnectBuilder SetOriginalPrincipal(string originalPrincipal)
        {
            _connect.OriginalPrincipal = originalPrincipal;
            return this;
        }
        public  CommandConnectBuilder SetAuthMethodName(string authMethodName)
        {
            _connect.AuthMethodName = authMethodName;
            return this;
        }
        public  CommandConnectBuilder SetAuthMethod(AuthMethod authMethod)
        {
            _connect.AuthMethod = authMethod;
            return this;
        }
        public  CommandConnectBuilder SetOriginalAuthData(string originalAuthData)
        {
            _connect.OriginalAuthData = originalAuthData;
            return this;
        }
        public  CommandConnectBuilder SetOriginalAuthMethod(string originalAuthMethod)
        {
            _connect.OriginalAuthMethod = originalAuthMethod;
            return this;
        }
        public  CommandConnectBuilder SetAuthData(byte[] authData)
        {
            _connect.AuthData = authData;
            return this;
        }
        public  CommandConnect Build()
        {
            if ("ycav1".Equals(_connect.AuthMethodName))
            {
                // Handle the case of a client that gets updated before the broker and starts sending the string auth method
                // name. An example would be in broker-to-broker replication. We need to make sure the clients are still
                // passing both the enum and the string until all brokers are upgraded.
                _connect.AuthMethod = AuthMethod.AuthMethodYcaV1;
            }
            if (string.IsNullOrWhiteSpace(_connect.ClientVersion))
                _connect.ClientVersion = "Pulsar Client";
            return _connect;
        }
    }
}