using System.Collections.Generic;
using SharpPulsar.Akka;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Exceptions;
using SharpPulsar.Handlers;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Schema;
using Xunit;
using Xunit.Abstractions;
using AuthenticationFactory = SharpPulsar.Impl.Auth.AuthenticationFactory;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Test.Api
{
    public class ProxyProtocolTest //: TlsProducerConsumerBase
	{
        private readonly string _tlsClientCertFilePath = "./resources/authentication/tls/client-cert.pem";
        private readonly string _tlsClientKeyFilePath = "./resources/authentication/tls/client-key.pem";
        protected internal readonly string TlsTrustCertFilePath = "./resources/authentication/tls/cacert.pem";

		private readonly TestCommon.Common _common;
        private readonly ITestOutputHelper _output;

        public ProxyProtocolTest(ITestOutputHelper output)
        {
            _output = output;
            _common = new TestCommon.Common(output);
        }
		[Fact]
		public void TestSniProxyProtocol()
		{
            // Client should try to connect to proxy and pass broker-url as SNI header
			string proxyUrl = "";
			string brokerServiceUrl = "pulsar+ssl://1.1.1.1:6651";
			string topicName = "persistent://my-property/use/my-ns/my-topic1";
			
            IDictionary<string, string> authParams = new Dictionary<string, string>();
			authParams["tlsCertFile"] = _tlsClientCertFilePath;
			authParams["tlsKeyFile"] = _tlsClientKeyFilePath;

            var builder = new PulsarClientConfigBuilder()
                .ServiceUrl(brokerServiceUrl)
                .ProxyServiceUrl(proxyUrl, ProxyProtocol.SNI)
                .ConnectionsPerBroker(1)
                .UseProxy(true)
                .AddTrustedAuthCert(null)
                .Authentication(AuthenticationFactory.Create(typeof(AuthenticationDataTls).FullName, authParams))
                .AllowTlsInsecureConnection(false)
                .EnableTls(true);
                builder.OperationTimeout(1000);

            var clientConfig = builder.ClientConfigurationData;

            var system = PulsarSystem.GetInstance(clientConfig);
            var conf = new ProducerConfigurationData {TopicName = topicName};
            // should be able to create producer successfully
            var p = system.PulsarProducer(new CreateProducer(BytesSchema.Of(), conf));
		}

		[Fact]
		public virtual void TestSniProxyProtocolWithInvalidProxyUrl()
		{
            // Client should try to connect to proxy and pass broker-url as SNI header
			string brokerServiceUrl = "pulsar+ssl://1.1.1.1:6651";
			string proxyHost = "invalid-url";
			string proxyUrl = "pulsar+ssl://" + proxyHost + ":5555";
			string topicName = "persistent://my-property/use/my-ns/my-topic1";

            IDictionary<string, string> authParams = new Dictionary<string, string>();
            authParams["tlsCertFile"] = _tlsClientCertFilePath;
            authParams["tlsKeyFile"] = _tlsClientKeyFilePath;

            var builder = new PulsarClientConfigBuilder()
                .ServiceUrl(brokerServiceUrl)
                .ProxyServiceUrl(proxyUrl, ProxyProtocol.SNI)
                .ConnectionsPerBroker(1)
                .UseProxy(true)
                .AddTrustedAuthCert(null)
                .Authentication(AuthenticationFactory.Create(typeof(AuthenticationDataTls).FullName, authParams))
                .AllowTlsInsecureConnection(false)
                .EnableTls(true);
            builder.OperationTimeout(1000);

            var clientConfig = builder.ClientConfigurationData;

            var system = PulsarSystem.GetInstance(clientConfig);
            var conf = new ProducerConfigurationData
            {
                TopicName = topicName,
				//ProducerEventListener = new DefaultProducerListener()
            };
			var p = system.PulsarProducer(new CreateProducer(BytesSchema.Of(), conf));
            //assertTrue(e.Message.Contains(proxyHost));
        }

        [Fact]
        public void TestSniProxyProtocolWithoutTls()
		{
			// Client should try to connect to proxy and pass broker-url as SNI header
			string proxyUrl = "";
			string brokerServiceUrl = "pulsar+ssl://1.1.1.1:6651";
			string topicName = "persistent://my-property/use/my-ns/my-topic1";


            IDictionary<string, string> authParams = new Dictionary<string, string>();
            authParams["tlsCertFile"] = _tlsClientCertFilePath;
            authParams["tlsKeyFile"] = _tlsClientKeyFilePath;

            var builder = new PulsarClientConfigBuilder()
                .ServiceUrl(brokerServiceUrl)
                .ProxyServiceUrl(proxyUrl, ProxyProtocol.SNI)
                .ConnectionsPerBroker(1)
                .UseProxy(true)
                .AddTrustedAuthCert(null)
                .Authentication(AuthenticationFactory.Create(typeof(AuthenticationDataTls).FullName, authParams))
                .AllowTlsInsecureConnection(false)
                .EnableTls(false);
            builder.OperationTimeout(1000);

            var clientConfig = builder.ClientConfigurationData;

            var system = PulsarSystem.GetInstance(clientConfig);
            var conf = new ProducerConfigurationData
            {
                TopicName = topicName,
                //ProducerEventListener = new DefaultProducerListener()
            };
            var p = system.PulsarProducer(new CreateProducer(BytesSchema.Of(), conf));
            //assertTrue(e.Message.Contains(proxyHost));
		}
	}

}