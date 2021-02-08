using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using App.Metrics.Concurrency;
using PulsarAdmin.Models;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Auth;
using SharpPulsar.Impl.Schema;
using Xunit;
using Xunit.Abstractions;
using SharpPulsar.Admin;

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

    public class TlsProducerConsumerTest
	{
        protected internal readonly string TlsTrustCertFilePath = "./resources/authentication/tls/cacert.pem";
        protected internal readonly string TlsClientCertFilePath = "./resources/authentication/tls/client-cert.pem";
        protected internal readonly string TlsClientKeyFilePath = "./resources/authentication/tls/client-key.pem";
        private readonly string _clusterName = "use";


        private readonly TestCommon.Common _common;
        private readonly ITestOutputHelper _output;

        public TlsProducerConsumerTest(ITestOutputHelper output)
        {
            _output = output;
            _common = new TestCommon.Common(output);
        }
		/// <summary>
		/// verifies that messages whose size is larger than 2^14 bytes (max size of single TLS chunk) can be
		/// produced/consumed
		/// </summary>
		/// <exception cref="Exception"> </exception>
		///
		///
		[Fact]
		public void TestTlsLargeSizeMessage()
		{
			_output.WriteLine($"-- Starting 'TestTlsLargeSizeMessage' test --");

			const int messageSize = 16 * 1024 + 1;

			InternalSetUpForClient(true, "pulsar.BrokerServiceUrlTls");
			InternalSetUpForNamespace();

			var consumer = _common.PulsarSystem.PulsarConsumer(_common.CreateConsumer(BytesSchema.Of(), "persistent://my-property/use/my-ns/my-topic1", "TestTlsLargeSizeMessage", "my-subscriber-name"));

			var producer = _common.PulsarSystem.PulsarProducer(_common.CreateProducer(BytesSchema.Of(), "persistent://my-property/use/my-ns/my-topic1", "TestTlsLargeSizeMessage"));
			for (int i = 0; i < 10; i++)
			{
				var message = new byte[messageSize];
				Array.Fill(message, (byte) i);
				var send = new Send(message, ImmutableDictionary<string, object>.Empty);
				_common.PulsarSystem.Send(send, producer.Producer);
			}

			ConsumedMessage msg = null;
			for (int i = 0; i < 10; i++)
			{
				msg = _common.PulsarSystem.Receive("", 5000);
				var expected = new byte[messageSize];
				Array.Fill(expected, (byte) i);
                var data = (byte[])(object)msg.Message.Data;
				Assert.Equal(expected, data);
			}
			// Acknowledge the consumption of all messages at once
			_common.PulsarSystem.AcknowledgeCumulative(msg);
			_output.WriteLine($"-- Exiting 'TestTlsLargeSizeMessage' test --");
		}

		[Fact]
		public void TestTlsClientAuthOverBinaryProtocol()
		{
            _output.WriteLine($"-- Starting 'TestTlsClientAuthOverBinaryProtocol' test --");

			InternalSetUpForNamespace();

			// Test 1 - Using TLS on binary protocol without sending certs - expect failure
			InternalSetUpForClient(false, "pulsar.BrokerServiceUrlTls");
			try
            {
                _common.PulsarSystem.PulsarConsumer(_common.CreateConsumer(BytesSchema.Of(), "persistent://my-property/use/my-ns/my-topic1", "TestTlsClientAuthOverBinaryProtocol", "my-subscriber-name")); //pulsarClient.newConsumer().topic("persistent://my-property/use/my-ns/my-topic1").subscriptionName("my-subscriber-name").subscriptionType(SubscriptionType.Exclusive).subscribe();
                //Assert.fail("Server should have failed the TLS handshake since client didn't .");
            }
			catch (Exception)
			{
				// OK
			}

			// Test 2 - Using TLS on binary protocol - sending certs
			InternalSetUpForClient(true, "pulsar.BrokerServiceUrlTls");
			try
			{
                _common.PulsarSystem.PulsarConsumer(_common.CreateConsumer(BytesSchema.Of(), "persistent://my-property/use/my-ns/my-topic1", "TestTlsClientAuthOverBinaryProtocol", "my-subscriber-name")); //pulsarClient.newConsumer().topic("persistent://my-property/use/my-ns/my-topic1").subscriptionName("my-subscriber-name").subscriptionType(SubscriptionType.Exclusive).subscribe();

			}
			catch (Exception)
			{
				//Assert.fail("Should not fail since certs are sent.");
			}
		}

		[Fact]
		public void TestTlsCertsFromDynamicStream()
		{
			string topicName = "persistent://my-property/use/my-ns/my-topic1";
			
            AtomicInteger index = new AtomicInteger(0);

			MemoryStream certStream = CreateByteInputStream(TlsClientCertFilePath);
			MemoryStream keyStream = CreateByteInputStream(TlsClientKeyFilePath);

			Func<MemoryStream> certProvider = () => GetStream(index, certStream);
			Func<MemoryStream> keyProvider = () => GetStream(index, keyStream);
			AuthenticationTls auth = new AuthenticationTls(certProvider, keyProvider);
            _common.GetPulsarSystem(auth, 1000, enableTls:true, brokerService: "pulsar.BrokerServiceUrlTls");

			
            _common.PulsarSystem.PulsarConsumer(_common.CreateConsumer(BytesSchema.Of(), topicName, "TestTlsCertsFromDynamicStream", "my-subscriber-name")); 

			// unload the topic so, new connection will be made and read the cert streams again

            var producer = _common.PulsarSystem.PulsarProducer(_common.CreateProducer(BytesSchema.Of(), "persistent://my-property/use/my-ns/my-topic1", ""));
			for (int i = 0; i < 10; i++)
			{
				var send = new Send(("test" + i).GetBytes(), ImmutableDictionary<string, object>.Empty);
				_common.PulsarSystem.Send(send, producer.Producer);
			}

			ConsumedMessage msg = null;
			for (var i = 0; i < 10; i++)
			{
				msg = _common.PulsarSystem.Receive("TestTlsCertsFromDynamicStream", 5000);
				var exepctedMsg = "test" + i;
                var data = (byte[])(object)msg.Message.Data;
				Assert.Equal(exepctedMsg.GetBytes(), data);
			}
			// Acknowledge the consumption of all messages at once
			_common.PulsarSystem.AcknowledgeCumulative(msg);
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
		///
		[Fact]
		public void TestTlsCertsFromDynamicStreamExpiredAndRenewCert()
		{
            AtomicInteger certIndex = new AtomicInteger(1);
			AtomicInteger keyIndex = new AtomicInteger(0);
			MemoryStream certStream = CreateByteInputStream(TlsClientCertFilePath);
			MemoryStream keyStream = CreateByteInputStream(TlsClientKeyFilePath);
			Func<MemoryStream> certProvider = () => GetStream(certIndex, certStream, keyStream);
			Func<MemoryStream> keyProvider = () => GetStream(keyIndex, keyStream);
			AuthenticationTls auth = new AuthenticationTls(certProvider, keyProvider);
			
			_common.GetPulsarSystem(auth, 1000, enableTls:true, brokerService: "pulsar.BrokerServiceUrlTls");


            _common.PulsarSystem.PulsarConsumer(_common.CreateConsumer(BytesSchema.Of(), "persistent://my-property/use/my-ns/my-topic1", "TestTlsCertsFromDynamicStreamExpiredAndRenewCert", "my-subscriber-name"));


			certIndex.SetValue(0);
            _common.PulsarSystem.PulsarConsumer(_common.CreateConsumer(BytesSchema.Of(), "persistent://my-property/use/my-ns/my-topic1", "TestTlsCertsFromDynamicStreamExpiredAndRenewCert", "my-subscriber-name"));


		}

		private MemoryStream CreateByteInputStream(string filePath)
		{
			Stream inStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			MemoryStream baos = new MemoryStream();
			inStream.CopyTo(baos);
            return baos;
        }

		private MemoryStream GetStream(AtomicInteger index, params MemoryStream[] streams)
		{
			return streams[index.GetValue()];
		}
        private void InternalSetUpForClient(bool addCertificates, string lookupUrl)
		{
            IDictionary<string, string> authParams = new Dictionary<string, string>();
            authParams["tlsCertFile"] = TlsClientCertFilePath;
            authParams["tlsKeyFile"] = TlsClientKeyFilePath;
			_common.GetPulsarSystem(addCertificates? AuthenticationFactory.Create(typeof(AuthenticationTls).FullName, authParams): new AuthenticationDisabled(), 1000, enableTls: true, brokerService: lookupUrl);

		}


		private void InternalSetUpForNamespace()
		{
			IDictionary<string, string> authParams = new Dictionary<string, string>();
			authParams["tlsCertFile"] = TlsClientCertFilePath;
			authParams["tlsKeyFile"] = TlsClientKeyFilePath;

            _common.PulsarSystem.PulsarAdmin(new Admin(AdminCommands.CreateCluster, new object[] { "test", new ClusterData("brokerUrl.ToString(), brokerUrlTls.ToString(), pulsar.BrokerServiceUrl, pulsar.BrokerServiceUrlTls") },
                (f) => { }, (e) => _output.WriteLine(e.ToString()), "http://localhost:8080", l => { _output.WriteLine(l); }));

            _common.PulsarSystem.PulsarAdmin(new Admin(AdminCommands.CreateTenant, new object[] { "my-property", new TenantInfo(new List<string> { "appid1", "appid2" }, new List<string> { "use" }) },
                (f) => { }, (e) => _output.WriteLine(e.ToString()), "http://localhost:8080", l => { _output.WriteLine(l); }));

            _common.PulsarSystem.PulsarAdmin(new Admin(AdminCommands.CreateNamespace, new object[] { "my-property", "my-ns", new Policies(replicationClusters: new List<string> { "test" }), },
                (f) => { }, (e) => _output.WriteLine(e.ToString()), "http://localhost:8080", l => { _output.WriteLine(l); }));

		}
	}

}