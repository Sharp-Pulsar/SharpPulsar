using System;
using System.Collections.Generic;
using ProducerConsumerBase = SharpPulsar.Test.Api.ProducerConsumerBase;

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
namespace SharpPulsar.Test.Impl
{
    public class KeyStoreTlsProducerConsumerTestWithAuth : ProducerConsumerBase
	{
		protected internal readonly string BROKER_KEYSTORE_FILE_PATH = "./src/test/resources/authentication/keystoretls/broker.keystore.jks";
		protected internal readonly string BROKER_TRUSTSTORE_FILE_PATH = "./src/test/resources/authentication/keystoretls/broker.truststore.jks";
		protected internal readonly string BROKER_KEYSTORE_PW = "111111";
		protected internal readonly string BROKER_TRUSTSTORE_PW = "111111";

		protected internal readonly string CLIENT_KEYSTORE_FILE_PATH = "./src/test/resources/authentication/keystoretls/client.keystore.jks";
		protected internal readonly string CLIENT_TRUSTSTORE_FILE_PATH = "./src/test/resources/authentication/keystoretls/client.truststore.jks";
		protected internal readonly string CLIENT_KEYSTORE_PW = "111111";
		protected internal readonly string CLIENT_TRUSTSTORE_PW = "111111";

		protected internal readonly string CLIENT_KEYSTORE_CN = "clientuser";
		protected internal readonly string KEYSTORE_TYPE = "JKS";

		private readonly string clusterName = "use";

		public override void setup()
		{
			// TLS configuration for Broker
			internalSetUpForBroker();

			// Start Broker

			base.init();
		}


		public override void cleanup()
		{
			base.internalCleanup();
		}


		public virtual void internalSetUpForBroker()
		{
			conf.BrokerServicePortTls = 0;
			conf.WebServicePortTls = 0;
			conf.TlsEnabledWithKeyStore = true;

			conf.TlsKeyStoreType = KEYSTORE_TYPE;
			conf.TlsKeyStore = BROKER_KEYSTORE_FILE_PATH;
			conf.TlsKeyStorePassword = BROKER_KEYSTORE_PW;

			conf.TlsTrustStoreType = KEYSTORE_TYPE;
			conf.TlsTrustStore = CLIENT_TRUSTSTORE_FILE_PATH;
			conf.TlsTrustStorePassword = CLIENT_TRUSTSTORE_PW;

			conf.ClusterName = clusterName;
			conf.TlsRequireTrustedClientCertOnConnect = true;

			// config for authentication and authorization.
			conf.SuperUserRoles = Sets.newHashSet(CLIENT_KEYSTORE_CN);
			conf.AuthenticationEnabled = true;
			conf.AuthorizationEnabled = true;
			ISet<string> providers = new HashSet<string>();

			providers.Add(typeof(AuthenticationProviderTls).FullName);
			conf.AuthenticationProviders = providers;
		}


		public virtual void internalSetUpForClient(bool addCertificates, string lookupUrl)
		{
			if (pulsarClient != null)
			{
				pulsarClient.Dispose();
			}

			ISet<string> tlsProtocols = Sets.newConcurrentHashSet();
			tlsProtocols.Add("TLSv1.2");

			ClientBuilder clientBuilder = PulsarClient.builder().serviceUrl(lookupUrl).enableTls(true).useKeyStoreTls(true).tlsTrustStorePath(BROKER_TRUSTSTORE_FILE_PATH).tlsTrustStorePassword(BROKER_TRUSTSTORE_PW).allowTlsInsecureConnection(false).tlsProtocols(tlsProtocols).operationTimeout(1000, TimeUnit.MILLISECONDS);
			if (addCertificates)
			{
				IDictionary<string, string> authParams = new Dictionary<string, string>();
				authParams[AuthenticationKeyStoreTls.KEYSTORE_TYPE] = KEYSTORE_TYPE;
				authParams[AuthenticationKeyStoreTls.KEYSTORE_PATH] = CLIENT_KEYSTORE_FILE_PATH;
				authParams[AuthenticationKeyStoreTls.KEYSTORE_PW] = CLIENT_KEYSTORE_PW;

				clientBuilder.authentication(typeof(AuthenticationKeyStoreTls).FullName, authParams);
			}
			pulsarClient = clientBuilder.build();
		}

		public virtual void internalSetUpForNamespace()
		{
			IDictionary<string, string> authParams = new Dictionary<string, string>();
			authParams[AuthenticationKeyStoreTls.KEYSTORE_PATH] = CLIENT_KEYSTORE_FILE_PATH;
			authParams[AuthenticationKeyStoreTls.KEYSTORE_PW] = CLIENT_KEYSTORE_PW;

			if (admin != null)
			{
				admin.Dispose();
			}

            admin = spy(PulsarAdmin.builder().serviceHttpUrl(brokerUrlTls.ToString()).useKeyStoreTls(true).tlsTrustStorePath(BROKER_TRUSTSTORE_FILE_PATH).tlsTrustStorePassword(BROKER_TRUSTSTORE_PW).allowTlsInsecureConnection(false).authentication(typeof(AuthenticationKeyStoreTls).FullName, authParams).build());
			admin.clusters().createCluster(clusterName, new ClusterData(brokerUrl.ToString(), brokerUrlTls.ToString(), pulsar.BrokerServiceUrl, pulsar.BrokerServiceUrlTls));
			admin.tenants().createTenant("my-property", new TenantInfo(Sets.newHashSet("appid1", "appid2"), Sets.newHashSet("use")));
			admin.namespaces().createNamespace("my-property/my-ns");
		}

		/// <summary>
		/// verifies that messages whose size is larger than 2^14 bytes (max size of single TLS chunk) can be
		/// produced/consumed
		/// </summary>
		/// <exception cref="Exception"> </exception>
		/// 
		public virtual void testTlsLargeSizeMessage()
		{
			log.info("-- Starting {} test --", methodName);

			const int MESSAGE_SIZE = 16 * 1024 + 1;
			log.info("-- message size --", MESSAGE_SIZE);
			string topicName = "persistent://my-property/use/my-ns/testTlsLargeSizeMessage" + DateTimeHelper.CurrentUnixTimeMillis();

			internalSetUpForClient(true, pulsar.BrokerServiceUrlTls);
			internalSetUpForNamespace();

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName("my-subscriber-name").subscribe();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).create();
			for (int i = 0; i < 10; i++)
			{
				sbyte[] message = new sbyte[MESSAGE_SIZE];
				Arrays.fill(message, (sbyte) i);
				producer.send(message);
			}

			Message<sbyte[]> msg = null;
			for (int i = 0; i < 10; i++)
			{
				msg = consumer.receive(5, TimeUnit.SECONDS);
				sbyte[] expected = new sbyte[MESSAGE_SIZE];
				Arrays.fill(expected, (sbyte) i);
				Assert.assertEquals(expected, msg.Data);
			}
			// Acknowledge the consumption of all messages at once
			consumer.acknowledgeCumulative(msg);
			consumer.close();
			log.info("-- Exiting {} test --", methodName);
		}


		public virtual void testTlsClientAuthOverBinaryProtocol()
		{
			log.info("-- Starting {} test --", methodName);

			const int MESSAGE_SIZE = 16 * 1024 + 1;
			log.info("-- message size --", MESSAGE_SIZE);
			string topicName = "persistent://my-property/use/my-ns/testTlsClientAuthOverBinaryProtocol" + DateTimeHelper.CurrentUnixTimeMillis();

			internalSetUpForNamespace();

			// Test 1 - Using TLS on binary protocol without sending certs - expect failure
			internalSetUpForClient(false, pulsar.BrokerServiceUrlTls);

			try
			{
				pulsarClient.newConsumer().topic(topicName).subscriptionName("my-subscriber-name").subscriptionType(SubscriptionType.Exclusive).subscribe();
				Assert.fail("Server should have failed the TLS handshake since client didn't .");
			}
			catch (Exception)
			{
				// OK
			}

			// Using TLS on binary protocol - sending certs
			internalSetUpForClient(true, pulsar.BrokerServiceUrlTls);

			try
			{
				pulsarClient.newConsumer().topic(topicName).subscriptionName("my-subscriber-name").subscriptionType(SubscriptionType.Exclusive).subscribe();
			}
			catch (Exception)
			{
				Assert.fail("Should not fail since certs are sent.");
			}
		}


		public virtual void testTlsClientAuthOverHTTPProtocol()
		{
			log.info("-- Starting {} test --", methodName);

			const int MESSAGE_SIZE = 16 * 1024 + 1;
			log.info("-- message size --", MESSAGE_SIZE);
			string topicName = "persistent://my-property/use/my-ns/testTlsClientAuthOverHTTPProtocol" + DateTimeHelper.CurrentUnixTimeMillis();

			internalSetUpForNamespace();

			// Test 1 - Using TLS on https without sending certs - expect failure
			internalSetUpForClient(false, pulsar.WebServiceAddressTls);
			try
			{
				pulsarClient.newConsumer().topic(topicName).subscriptionName("my-subscriber-name").subscriptionType(SubscriptionType.Exclusive).subscribe();
				Assert.fail("Server should have failed the TLS handshake since client didn't .");
			}
			catch (Exception)
			{
				// OK
			}

			// Test 2 - Using TLS on https - sending certs
			internalSetUpForClient(true, pulsar.WebServiceAddressTls);
			try
			{
				pulsarClient.newConsumer().topic(topicName).subscriptionName("my-subscriber-name").subscriptionType(SubscriptionType.Exclusive).subscribe();
			}
			catch (Exception)
			{
				Assert.fail("Should not fail since certs are sent.");
			}
		}

	}

}