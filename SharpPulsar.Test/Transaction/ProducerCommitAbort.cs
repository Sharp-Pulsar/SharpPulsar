using SharpPulsar.Configuration;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Text;
using System.Threading;
using SharpPulsar.Extension;
using Xunit;
using Xunit.Abstractions;
using SharpPulsar.Common;

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
namespace SharpPulsar.Test.Transaction
{
    /// <summary>
    /// End to end transaction test.
    /// </summary>
    [Collection(nameof(PulsarTests))]
	public class ProducerCommitAbort
	{

		private const int TopicPartition = 3;

		private const string TENANT = "public";
		private static readonly string _nAMESPACE1 = TENANT + "/default";
		private static readonly string _topicOutput = _nAMESPACE1 + $"/output-{Guid.NewGuid()}";
		private static readonly string _topicMessageAckTest = _nAMESPACE1 + "/message-ack-test";

		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;
		public ProducerCommitAbort(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}
		[Fact]
		public void ProduceCommitTest()
		{
			var topic = $"{_topicOutput}-{Guid.NewGuid()}";
			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(topic)
				.SubscriptionName($"test-{Guid.NewGuid()}");

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(topic)
				.SendTimeout(0);

			Producer<byte[]> producer = _client.NewProducer(producerBuilder);

			User.Transaction txn = Txn;

			int txnMessageCnt = 0;
			int messageCnt = 40;
			for(int i = 0; i < messageCnt; i++)
			{
				producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).Send();
				txnMessageCnt++;
			}

			// Can't receive transaction messages before commit.
			var message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
			Assert.Null(message);

			txn.Commit();
			// txn1 messages could be received after txn1 committed
			int receiveCnt = 0;
			for(int i = 0; i < txnMessageCnt; i++)
			{
				message = consumer.Receive(TimeSpan.FromSeconds(10));
				Assert.NotNull(message);
				receiveCnt++;
			}
			Assert.Equal(txnMessageCnt, receiveCnt);

			message = consumer.Receive(TimeSpan.FromMilliseconds(5000));
			Assert.Null(message);

			_output.WriteLine($"message commit test enableBatch {true}");
		}
		[Fact]
		public void ProduceCommitBatchedTest()
		{
			var topic = $"{_topicOutput}-{Guid.NewGuid()}";
			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(topic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.EnableBatchIndexAcknowledgment(true);
			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(topic)
				.EnableBatching(true)
				.SendTimeout(0);

			Producer<byte[]> producer = _client.NewProducer(producerBuilder);

			User.Transaction txn = Txn;

			int txnMessageCnt = 0;
			int messageCnt = 40;
			for(int i = 0; i < messageCnt; i++)
			{
				producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).Send();
				txnMessageCnt++;
			}

			// Can't receive transaction messages before commit.
			var message = consumer.Receive(TimeSpan.FromMilliseconds(5000));
			Assert.Null(message);

			txn.Commit();

			// txn1 messages could be received after txn1 committed
			int receiveCnt = 0;
			for(int i = 0; i < txnMessageCnt; i++)
			{
				message = consumer.Receive(TimeSpan.FromSeconds(10));
				Assert.NotNull(message);
				receiveCnt++;
				_output.WriteLine($"message receive count: {receiveCnt}");
			}
			Assert.Equal(txnMessageCnt, receiveCnt);

			message = consumer.Receive(TimeSpan.FromMilliseconds(5000));
			Assert.Null(message);

			_output.WriteLine($"message commit test enableBatch {true}");
		}
		[Fact]
		public void ProduceAbortTest()
		{
			User.Transaction txn = Txn;
			

			var producerBuilder = new ProducerConfigBuilder<byte[]>();
			producerBuilder.Topic(_topicOutput);
			producerBuilder.EnableBatching(true);
			producerBuilder.SendTimeout(0);

			var producer = _client.NewProducer(producerBuilder);

			int messageCnt = 10;
			for(int i = 0; i < messageCnt; i++)
			{
				producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).Send();
			}

			var consumerBuilder = new ConsumerConfigBuilder<byte[]>();
			consumerBuilder.Topic(_topicOutput);
			consumerBuilder.SubscriptionName("test");
			consumerBuilder.EnableBatchIndexAcknowledgment(true);
			consumerBuilder.SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest);
			var consumer = _client.NewConsumer(consumerBuilder);

			// Can't receive transaction messages before abort.
			var message = consumer.Receive(TimeSpan.FromMilliseconds(5000));
			Assert.Null(message);

			txn.Abort();

			// Cant't receive transaction messages after abort.
			message = consumer.Receive(TimeSpan.FromMilliseconds(5000));
			Assert.Null(message);
		}

		private User.Transaction Txn
		{
			
			get
			{
				return (User.Transaction)_client.NewTransaction().WithTransactionTimeout(2000).Build();
			}
		}

	}

}