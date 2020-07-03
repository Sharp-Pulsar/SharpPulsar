using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Test
{
    public class TlsProducerConsumerTest
    {
        protected internal readonly string TlsTrustCertFilePath = "./src/test/resources/authentication/tls/cacert.pem";
        protected internal readonly string TlsClientCertFilePath = "./src/test/resources/authentication/tls/client-cert.pem";
        protected internal readonly string TlsClientKeyFilePath = "./src/test/resources/authentication/tls/client-key.pem";
        protected internal readonly string TlsServerCertFilePath = "./src/test/resources/authentication/tls/broker-cert.pem";
        protected internal readonly string TlsServerKeyFilePath = "./src/test/resources/authentication/tls/broker-key.pem";
        private readonly string _clusterName = "use";
        public virtual void internalSetUpForClient(bool addCertificates, string lookupUrl)
		{
			if (pulsarClient != null)
			{
				pulsarClient.Dispose();
			}

			ClientBuilder clientBuilder = PulsarClient.builder().serviceUrl(lookupUrl).tlsTrustCertsFilePath(TlsTrustCertFilePath).enableTls(true).allowTlsInsecureConnection(false).operationTimeout(1000, TimeUnit.MILLISECONDS);
			if (addCertificates)
			{
				IDictionary<string, string> authParams = new Dictionary<string, string>();
				authParams["tlsCertFile"] = TlsClientCertFilePath;
				authParams["tlsKeyFile"] = TlsClientKeyFilePath;
				//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				clientBuilder.authentication(typeof(AuthenticationTls).FullName, authParams);
			}
			pulsarClient = clientBuilder.build();
		}

		public virtual void internalSetUpForNamespace()
		{
			IDictionary<string, string> authParams = new Dictionary<string, string>();
			authParams["tlsCertFile"] = TlsClientCertFilePath;
			authParams["tlsKeyFile"] = TlsClientKeyFilePath;

			if (admin != null)
			{
				admin.Dispose();
			}
            admin = spy(PulsarAdmin.builder().serviceHttpUrl(brokerUrlTls.ToString()).tlsTrustCertsFilePath(TlsTrustCertFilePath).allowTlsInsecureConnection(false).authentication(typeof(AuthenticationTls).FullName, authParams).build());
			admin.clusters().createCluster(_clusterName, new ClusterData(brokerUrl.ToString(), brokerUrlTls.ToString(), pulsar.BrokerServiceUrl, pulsar.BrokerServiceUrlTls));
			admin.tenants().createTenant("my-property", new TenantInfo(Sets.newHashSet("appid1", "appid2"), Sets.newHashSet("use")));
			admin.namespaces().createNamespace("my-property/my-ns");
		}
		/// <summary>
		/// verifies that messages whose size is larger than 2^14 bytes (max size of single TLS chunk) can be
		/// produced/consumed
		/// </summary>
		/// <exception cref="Exception"> </exception>
		public virtual void TestTlsLargeSizeMessage()
		{
			log.info("-- Starting {} test --", methodName);

			const int messageSize = 16 * 1024 + 1;
			log.info("-- message size --", messageSize);

			internalSetUpForClient(true, pulsar.BrokerServiceUrlTls);
			internalSetUpForNamespace();

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic("persistent://my-property/use/my-ns/my-topic1").subscriptionName("my-subscriber-name").subscribe();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic("persistent://my-property/use/my-ns/my-topic1").create();
			for (int i = 0; i < 10; i++)
			{
				sbyte[] message = new sbyte[messageSize];
				Arrays.fill(message, (sbyte)i);
				producer.send(message);
			}

			Message<sbyte[]> msg = null;
			for (int i = 0; i < 10; i++)
			{
				msg = consumer.receive(5, TimeUnit.SECONDS);
				sbyte[] expected = new sbyte[messageSize];
				Arrays.fill(expected, (sbyte)i);
				Assert.assertEquals(expected, msg.Data);
			}
			// Acknowledge the consumption of all messages at once
			consumer.acknowledgeCumulative(msg);
			consumer.close();
			log.info("-- Exiting {} test --", methodName);
		}

		public virtual void TestTlsClientAuth()
		{
			log.info("-- Starting {} test --", methodName);

			const int messageSize = 16 * 1024 + 1;
			log.info("-- message size --", messageSize);
			internalSetUpForNamespace();

			// Test 1 - Using TLS on binary protocol without sending certs - expect failure
			internalSetUpForClient(false, pulsar.BrokerServiceUrlTls);
			try
			{
				pulsarClient.newConsumer().topic("persistent://my-property/use/my-ns/my-topic1").subscriptionName("my-subscriber-name").subscriptionType(SubscriptionType.Exclusive).subscribe();
				Assert.fail("Server should have failed the TLS handshake since client didn't .");
			}
			catch (Exception)
			{
				// OK
			}

			// Test 2 - Using TLS on binary protocol - sending certs
			internalSetUpForClient(true, pulsar.BrokerServiceUrlTls);
			try
			{
				pulsarClient.newConsumer().topic("persistent://my-property/use/my-ns/my-topic1").subscriptionName("my-subscriber-name").subscriptionType(SubscriptionType.Exclusive).subscribe();
			}
			catch (Exception)
			{
				Assert.fail("Should not fail since certs are sent.");
			}
		}

		public virtual void TestTlsCertsFromDynamicStream()
		{
			log.info("-- Starting {} test --", methodName);
			string topicName = "persistent://my-property/use/my-ns/my-topic1";
			ClientBuilder clientBuilder = PulsarClient.builder().serviceUrl(pulsar.BrokerServiceUrlTls).tlsTrustCertsFilePath(TlsTrustCertFilePath).enableTls(true).allowTlsInsecureConnection(false).operationTimeout(1000, TimeUnit.MILLISECONDS);
			AtomicInteger index = new AtomicInteger(0);

			MemoryStream certStream = createByteInputStream(TlsClientCertFilePath);
			MemoryStream keyStream = createByteInputStream(TlsClientKeyFilePath);

			System.Func<MemoryStream> certProvider = () => getStream(index, certStream);
			System.Func<MemoryStream> keyProvider = () => getStream(index, keyStream);
			AuthenticationTls auth = new AuthenticationTls(certProvider, keyProvider);
			clientBuilder.authentication(auth);
			//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
			//ORIGINAL LINE: @Cleanup PulsarClient pulsarClient = clientBuilder.build();
			PulsarClient pulsarClient = clientBuilder.build();
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName("my-subscriber-name").subscribe();

			// unload the topic so, new connection will be made and read the cert streams again
			PersistentTopic topicRef = (PersistentTopic)pulsar.BrokerService.getTopicReference(topicName).get();
			topicRef.close(false);

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic("persistent://my-property/use/my-ns/my-topic1").createAsync().get(30, TimeUnit.SECONDS);
			for (int i = 0; i < 10; i++)
			{
				producer.send(("test" + i).GetBytes());
			}

			Message<sbyte[]> msg = null;
			for (int i = 0; i < 10; i++)
			{
				msg = consumer.receive(5, TimeUnit.SECONDS);
				string exepctedMsg = "test" + i;
				Assert.assertEquals(exepctedMsg.GetBytes(), msg.Data);
			}
			// Acknowledge the consumption of all messages at once
			consumer.acknowledgeCumulative(msg);
			consumer.close();
			log.info("-- Exiting {} test --", methodName);
		}

		/// <summary>
		/// It verifies that AuthenticationTls provides cert refresh functionality.
		/// 
		/// <pre>
		///  a. Create Auth with invalid cert
		///  b. Consumer fails with invalid tls certs
		///  c. refresh cert in provider
		///  d. Consumer successfully gets created
		/// </pre>
		/// </summary>
		/// <exception cref="Exception"> </exception>
		public virtual void TestTlsCertsFromDynamicStreamExpiredAndRenewCert()
		{
			log.info("-- Starting {} test --", methodName);
			ClientBuilder clientBuilder = PulsarClient.builder().serviceUrl(pulsar.BrokerServiceUrlTls).tlsTrustCertsFilePath(TlsTrustCertFilePath).enableTls(true).allowTlsInsecureConnection(false).operationTimeout(1000, TimeUnit.MILLISECONDS);
			AtomicInteger certIndex = new AtomicInteger(1);
			AtomicInteger keyIndex = new AtomicInteger(0);
			MemoryStream certStream = createByteInputStream(TlsClientCertFilePath);
			MemoryStream keyStream = createByteInputStream(TlsClientKeyFilePath);
			System.Func<MemoryStream> certProvider = () => getStream(certIndex, certStream, keyStream);
			System.Func<MemoryStream> keyProvider = () => getStream(keyIndex, keyStream);
			AuthenticationTls auth = new AuthenticationTls(certProvider, keyProvider);
			clientBuilder.authentication(auth);
			//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
			//ORIGINAL LINE: @Cleanup PulsarClient pulsarClient = clientBuilder.build();
			PulsarClient pulsarClient = clientBuilder.build();
			Consumer<sbyte[]> consumer = null;
			try
			{
				consumer = pulsarClient.newConsumer().topic("persistent://my-property/use/my-ns/my-topic1").subscriptionName("my-subscriber-name").subscribe();
				Assert.fail("should have failed due to invalid tls cert");
			}
			catch (PulsarClientException)
			{
				// Ok..
			}

			certIndex.set(0);
			consumer = pulsarClient.newConsumer().topic("persistent://my-property/use/my-ns/my-topic1").subscriptionName("my-subscriber-name").subscribe();
			consumer.close();
			log.info("-- Exiting {} test --", methodName);
		}

        private MemoryStream createByteInputStream(string filePath)
		{
			Stream inStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			MemoryStream baos = new MemoryStream();
			IOUtils.copy(inStream, baos);
			return new MemoryStream(baos.toByteArray());
		}

		private MemoryStream getStream(AtomicInteger index, params MemoryStream[] streams)
		{
			return streams[index.intValue()];
		}
	}
}
