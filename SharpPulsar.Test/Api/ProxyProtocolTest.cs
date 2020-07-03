using System.Collections.Generic;

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
		private static readonly Logger log = LoggerFactory.getLogger(typeof(ProxyProtocolTest));


		public virtual void testSniProxyProtocol()
		{

			// Client should try to connect to proxy and pass broker-url as SNI header
			string proxyUrl = org.apache.pulsar.BrokerServiceUrlTls;
			string brokerServiceUrl = "pulsar+ssl://1.1.1.1:6651";
			string topicName = "persistent://my-property/use/my-ns/my-topic1";

			ClientBuilder clientBuilder = PulsarClient.builder().serviceUrl(brokerServiceUrl).tlsTrustCertsFilePath(TLS_TRUST_CERT_FILE_PATH).enableTls(true).allowTlsInsecureConnection(false).proxyServiceUrl(proxyUrl, ProxyProtocol.SNI).operationTimeout(1000, TimeUnit.MILLISECONDS);
			IDictionary<string, string> authParams = new Dictionary<string, string>();
			authParams["tlsCertFile"] = TLS_CLIENT_CERT_FILE_PATH;
			authParams["tlsKeyFile"] = TLS_CLIENT_KEY_FILE_PATH;

			clientBuilder.authentication(typeof(AuthenticationTls).FullName, authParams);


			PulsarClient pulsarClient = clientBuilder.build();

			// should be able to create producer successfully
			pulsarClient.newProducer().topic(topicName).create();
		}


		public virtual void testSniProxyProtocolWithInvalidProxyUrl()
		{

			// Client should try to connect to proxy and pass broker-url as SNI header
			string brokerServiceUrl = "pulsar+ssl://1.1.1.1:6651";
			string proxyHost = "invalid-url";
			string proxyUrl = "pulsar+ssl://" + proxyHost + ":5555";
			string topicName = "persistent://my-property/use/my-ns/my-topic1";

			ClientBuilder clientBuilder = PulsarClient.builder().serviceUrl(brokerServiceUrl).tlsTrustCertsFilePath(TLS_TRUST_CERT_FILE_PATH).enableTls(true).allowTlsInsecureConnection(false).proxyServiceUrl(proxyUrl, ProxyProtocol.SNI).operationTimeout(1000, TimeUnit.MILLISECONDS);
			IDictionary<string, string> authParams = new Dictionary<string, string>();
			authParams["tlsCertFile"] = TLS_CLIENT_CERT_FILE_PATH;
			authParams["tlsKeyFile"] = TLS_CLIENT_KEY_FILE_PATH;

			clientBuilder.authentication(typeof(AuthenticationTls).FullName, authParams);


			PulsarClient pulsarClient = clientBuilder.build();

			try
			{
				pulsarClient.newProducer().topic(topicName).create();
				fail("should have failed due to invalid url");
			}
			catch (PulsarClientException e)
			{
				assertTrue(e.Message.contains(proxyHost));
			}
		}


		public virtual void testSniProxyProtocolWithoutTls()
		{
			// Client should try to connect to proxy and pass broker-url as SNI header
			string proxyUrl = org.apache.pulsar.BrokerServiceUrl;
			string brokerServiceUrl = "pulsar+ssl://1.1.1.1:6651";
			string topicName = "persistent://my-property/use/my-ns/my-topic1";

			ClientBuilder clientBuilder = PulsarClient.builder().serviceUrl(brokerServiceUrl).proxyServiceUrl(proxyUrl, ProxyProtocol.SNI).operationTimeout(1000, TimeUnit.MILLISECONDS);


			PulsarClient pulsarClient = clientBuilder.build();

			try
			{
				pulsarClient.newProducer().topic(topicName).create();
				fail("should have failed due to non-tls url");
			}
			catch (PulsarClientException)
			{
				// Ok
			}
		}
	}

}