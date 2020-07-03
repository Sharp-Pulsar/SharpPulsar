﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using PulsarAdmin.Models;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Admin;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using Xunit;
using Xunit.Abstractions;

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
    public class AuthenticatedProducerConsumerTest : ProducerConsumerBase
	{
		private readonly string _tlsTrustCertFilePath = "./resources/authentication/tls/cacert.pem";
		private readonly string _tlsServerCertFilePath = ".resources/authentication/tls/broker-cert.pem";
		private readonly string _tlsServerKeyFilePath = "./resources/authentication/tls/broker-key.pem";
		private readonly string _tlsClientCertFilePath = "./resources/authentication/tls/client-cert.pem";
		private readonly string _tlsClientKeyFilePath = "./resources/authentication/tls/client-key.pem";

        private PulsarSystem _pulsarSystem;
        private TestCommon.Common _common;
        private readonly ITestOutputHelper _output;

        public AuthenticatedProducerConsumerTest(ITestOutputHelper output)
        {
            _output = output;
			_common = new TestCommon.Common(output);
        }

        private void GetPulsarSystem(IAuthentication auth, int operationTime = 0, bool useProxy = false)
		{
            var builder = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650")
                .ConnectionsPerBroker(1)
                .UseProxy(useProxy)
                .StatsInterval(0)
                .Authentication(auth)
                .AllowTlsInsecureConnection(true)
                .EnableTls(true);
            if (operationTime > 0)
                builder.OperationTimeout(operationTime);
			
            var clientConfig = builder.ClientConfigurationData;

            _pulsarSystem =  PulsarSystem.GetInstance(clientConfig);
		}


		private void TestSyncProducerAndConsumer(IAuthentication auth, int batchMessageDelayMs, int operatioTimeout = 0)
        { 
			if(_pulsarSystem == null)
                GetPulsarSystem(auth, operatioTimeout);

            var consumer = _pulsarSystem.PulsarConsumer(_common.CreateConsumer(BytesSchema.Of(), "persistent://my-property/my-ns/my-topic", "", "my-subscriber-name", forceTopic:true));

            var producer = _pulsarSystem.PulsarProducer(_common.CreateProducer(BytesSchema.Of(), consumer.Topic, "TestSyncProducerAndConsumer", batchMessageDelayMs));

			for (int i = 0; i < 10; i++)
			{
				var message = "my-message-" + i;
				_pulsarSystem.Send(new Send(Encoding.UTF8.GetBytes(message), producer.Topic, ImmutableDictionary<string, object>.Empty), producer.Producer);
            }

			ConsumedMessage msg = null;
			ISet<string> messageSet = new HashSet<string>();
            var messages =  _pulsarSystem.Messages(false, customHander: (m) =>
            {
                msg = m;
                var receivedMessage = Encoding.UTF8.GetString((byte[])(object)m.Message.Data);
                return receivedMessage;
			} );
            var y = 0;
            foreach (var message in messages)
            {
                _output.WriteLine($"Received message: [{message}]");
				var expectedMessage = "my-message-" + y;
                TestMessageOrderAndDuplicates(messageSet, message, expectedMessage);
                y++;
            }

            var msgId = (MessageId) msg.Message.MessageId;
			// Acknowledge the consumption of all messages at once
            _pulsarSystem.PulsarConsumer(new AckMessages(msgId, msg.AckSets), consumer.Consumer);
			_pulsarSystem.Stop();
            _pulsarSystem = null;
        }
		[Fact]
		public void TestTlsSyncProducerAndConsumer()
		{
			_output.WriteLine($"-- Starting 'TestTlsSyncProducerAndConsumer' test --");

			IDictionary<string, string> authParams = new Dictionary<string, string>();
			authParams["tlsCertFile"] = _tlsClientCertFilePath;
			authParams["tlsKeyFile"] = _tlsClientKeyFilePath;
			IAuthentication authTls = new AuthenticationTls();
			authTls.Configure(JsonSerializer.Serialize(authParams));

            GetPulsarSystem(authTls);

			_pulsarSystem.PulsarAdmin(new Admin( AdminCommands.CreateCluster, new object[]{"test", new ClusterData("http://localhost:8080")},
                (f) => { }, (e)=> _output.WriteLine(e.ToString()), "http://localhost:8080", l=>{_output.WriteLine(l);}));

			_pulsarSystem.PulsarAdmin(new Admin(AdminCommands.CreateTenant, new object[] { "my-property",new TenantInfo(new List<string>{ "appid1", "appid2" }, new List<string>{ "test" })},
                (f) => { }, (e) => _output.WriteLine(e.ToString()), "http://localhost:8080", l => { _output.WriteLine(l); }));

            _pulsarSystem.PulsarAdmin(new Admin(AdminCommands.CreateNamespace, new object[] { "my-property", "my-ns", new Policies(replicationClusters: new List<string>{"test"}),  },
                (f) => { }, (e) => _output.WriteLine(e.ToString()), "http://localhost:8080", l => { _output.WriteLine(l); }));
			
			TestSyncProducerAndConsumer(authTls, 5000);

			_output.WriteLine("-- Exiting 'TestTlsSyncProducerAndConsumer' test --");
		}
		
		public void TestAnonymousSyncProducerAndConsumer(int batchMessageDelayMs)
		{

            _output.WriteLine("-- Starting 'TestAnonymousSyncProducerAndConsumer' test --");

			IDictionary<string, string> authParams = new Dictionary<string, string>();
			authParams["tlsCertFile"] = _tlsClientCertFilePath;
			authParams["tlsKeyFile"] = _tlsClientKeyFilePath;
			IAuthentication authTls = new AuthenticationTls();
			authTls.Configure(JsonSerializer.Serialize(authParams));

            GetPulsarSystem(authTls, 1000);

			_pulsarSystem.PulsarAdmin(new Admin(AdminCommands.CreateCluster, new object[] { "test", new ClusterData("http://localhost:8080", "https://localhost:8080", "http://localhost:6650", "https://localhost:6650") },
                (f) => { }, (e) => _output.WriteLine(e.ToString()), "http://localhost:8080", l => { _output.WriteLine(l); }));
            
            _pulsarSystem.PulsarAdmin(new Admin(AdminCommands.CreateTenant, new object[] { "my-property", new TenantInfo(new List<string> { "anonymousUser"}, new List<string> { "test" }) },
                (f) => { }, (e) => _output.WriteLine(e.ToString()), "http://localhost:8080", l => { _output.WriteLine(l); }));

            _pulsarSystem.PulsarAdmin(new Admin(AdminCommands.CreateNamespace, new object[] { "my-property", "my-ns", new Policies(replicationClusters: new List<string> { "test" }), },
                (f) => { }, (e) => _output.WriteLine(e.ToString()), "http://localhost:8080", l => { _output.WriteLine(l); }));

			_pulsarSystem.PulsarAdmin(new Admin(AdminCommands.GrantPermissionsOnPersistentTopic, new object[] { "my-property", "my-ns", "my-topic", "anonymousUser", new List<string> { "produce", "functions", "consume" } },
                (f) => { }, (e) => _output.WriteLine(e.ToString()), "http://localhost:8080", l => { _output.WriteLine(l); }));
            

			// unauthorized topic test
			Exception pulsarClientException = null;
			try
            {
                _pulsarSystem.PulsarConsumer(_common.CreateConsumer(new AutoConsumeSchema(),
                    "persistent://my-property/my-ns/other-topic", "fail", "fail-subscriber"));
			}
			catch (Exception e)
			{
				pulsarClientException = e;
			}
			//Assert.assertTrue(pulsarClientException is PulsarClientException);

			TestSyncProducerAndConsumer(authTls, 5000);

			_output.WriteLine("-- Exiting 'TestAnonymousSyncProducerAndConsumer' test --");
		}


	}

}