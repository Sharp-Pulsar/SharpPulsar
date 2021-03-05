using BAMCIS.Util.Concurrent;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SharpPulsar.Extension;
using Xunit;
using Xunit.Abstractions;
using SharpPulsar.Common;
using SharpPulsar.Exceptions;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;
using SharpPulsar.Interfaces;
using SharpPulsar.Transaction;

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
namespace SharpPulsar.Test
{
	/// <summary>
	/// End to end transaction test.
	/// </summary>
	[Collection(nameof(PulsarTests))]
	public class TransactionEndToEndTest
	{

		private const int TopicPartition = 3;

		private const string TENANT = "public";
		private static readonly string _nAMESPACE1 = TENANT + "/default";
		private static readonly string _topicOutput = _nAMESPACE1 + $"/output-{Guid.NewGuid()}";
		private static readonly string _topicMessageAckTest = _nAMESPACE1 + "/message-ack-test";

		private readonly ITestOutputHelper _output;
		private readonly PulsarSystem _system;
		private readonly PulsarClient _client;
		public TransactionEndToEndTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_system = fixture.System;
			_client = _system.NewClient();
		}
		[Fact]
		public void ProduceCommitTest()
		{
			var topic = $"{_topicOutput}-{Guid.NewGuid()}";
			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic(topic)
				.SubscriptionName($"test-{Guid.NewGuid()}");

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>()
				.Topic(topic)
				.SendTimeout(0);

			Producer<sbyte[]> producer = _client.NewProducer(producerBuilder);

			User.Transaction txn = Txn;

			int txnMessageCnt = 0;
			int messageCnt = 40;
			for(int i = 0; i < messageCnt; i++)
			{
				producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i).ToSBytes()).Send();
				txnMessageCnt++;
			}

			// Can't receive transaction messages before commit.
			var message = consumer.Receive(5000);
			Assert.Null(message);

			txn.Commit();

			// txn1 messages could be received after txn1 committed
			int receiveCnt = 0;
			for(int i = 0; i < txnMessageCnt; i++)
			{
				message = consumer.Receive();
				Assert.NotNull(message);
				receiveCnt++;
			}
			Assert.Equal(txnMessageCnt, receiveCnt);

			message = consumer.Receive(5000);
			Assert.Null(message);

			_output.WriteLine($"message commit test enableBatch {true}");
		}
		[Fact]
		public void ProduceCommitBatchedTest()
		{
			var topic = $"{_topicOutput}-{Guid.NewGuid()}";
			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic(topic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.EnableBatchIndexAcknowledgment(true);
			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>()
				.Topic(topic)
				.EnableBatching(true)
				.SendTimeout(0);

			Producer<sbyte[]> producer = _client.NewProducer(producerBuilder);

			User.Transaction txn = Txn;

			int txnMessageCnt = 0;
			int messageCnt = 40;
			for(int i = 0; i < messageCnt; i++)
			{
				producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i).ToSBytes()).Send();
				txnMessageCnt++;
			}

			// Can't receive transaction messages before commit.
			var message = consumer.Receive(5000);
			Assert.Null(message);

			txn.Commit();

			// txn1 messages could be received after txn1 committed
			int receiveCnt = 0;
			for(int i = 0; i < txnMessageCnt; i++)
			{
				message = consumer.Receive();
				Assert.NotNull(message);
				receiveCnt++;
			}
			Assert.Equal(txnMessageCnt, receiveCnt);

			message = consumer.Receive(5000);
			Assert.Null(message);

			_output.WriteLine($"message commit test enableBatch {true}");
		}
		[Fact]
		public virtual void ProduceAbortTest()
		{
			User.Transaction txn = Txn;
			

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>();
			producerBuilder.Topic(_topicOutput);
			producerBuilder.EnableBatching(true);
			producerBuilder.SendTimeout(0);

			var producer = _client.NewProducer(producerBuilder);

			int messageCnt = 10;
			for(int i = 0; i < messageCnt; i++)
			{
				producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i).ToSBytes()).Send();
			}

			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>();
			consumerBuilder.Topic(_topicOutput);
			consumerBuilder.SubscriptionName("test");
			consumerBuilder.EnableBatchIndexAcknowledgment(true);
			consumerBuilder.SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest);
			var consumer = _client.NewConsumer(consumerBuilder);

			// Can't receive transaction messages before abort.
			var message = consumer.Receive(5000);
			Assert.Null(message);

			txn.Abort();

			// Cant't receive transaction messages after abort.
			message = consumer.Receive(5000);
			Assert.Null(message);
		}
		[Fact]
		public void TxnAckTestBatchedFailoverSub()
		{
			string normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.EnableBatchIndexAcknowledgment(true)				
				.SubscriptionType(SubType.Failover);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.EnableBatching(true)
				.BatchingMaxMessages(100);

			var producer = _client.NewProducer(producerBuilder);

			for(int retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				User.Transaction txn = Txn;

				int messageCnt = 100;
				// produce normal messages
				for(int i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value("hello".GetBytes()).Send();
				}

				// consume and ack messages with txn
				for(int i = 0; i < messageCnt; i++)
				{
					var msg = consumer.Receive();
					Assert.NotNull(msg);
					_output.WriteLine($"receive msgId: {msg.MessageId}, count : {i}");
					//consumer.Acknowledge(msg.MessageId, txn);
				}

				// the messages are pending ack state and can't be received
				var message = consumer.Receive(2, TimeUnit.SECONDS);
				Assert.Null(message);

				// 1) txn abort
				txn.Abort();

				// after transaction abort, the messages could be received
				User.Transaction commitTxn = Txn;
				//Thread.Sleep(TimeSpan.FromSeconds(30));
				for(int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive(2, TimeUnit.SECONDS);
					Assert.NotNull(message);
					consumer.Acknowledge(message.MessageId, commitTxn);
					_output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
				}

				// 2) ack committed by a new txn
				commitTxn.Commit();

				// after transaction commit, the messages can't be received
				message = consumer.Receive(2, TimeUnit.SECONDS);
				Assert.Null(message);

				try
				{
					commitTxn.Commit();
					//fail("recommit one transaction should be failed.");
				}
				catch(Exception reCommitError)
				{
					// recommit one transaction should be failed
					//log.info("expected exception for recommit one transaction.");
					Assert.NotNull(reCommitError);
					Assert.True(reCommitError.InnerException is TransactionCoordinatorClientException.InvalidTxnStatusException);
				}
			}
		}
		
		[Fact]
		public void TxnAckTestBatchedSharedSub()
		{
			string normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.AcknowledgmentGroupTime(2000)
				.EnableBatchIndexAcknowledgment(true)				
				.SubscriptionType(SubType.Shared);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.EnableBatching(true)
				.BatchingMaxMessages(100);

			var producer = _client.NewProducer(producerBuilder);

			for(int retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				User.Transaction txn = Txn;

				int messageCnt = 100;
				// produce normal messages
				for(int i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value("hello".GetBytes()).Send();
				}

				// consume and ack messages with txn
				for(int i = 0; i < messageCnt; i++)
				{
					var msg = consumer.Receive();
					Assert.NotNull(msg);
					_output.WriteLine($"receive msgId: {msg.MessageId}, count : {i}");
					//consumer.Acknowledge(msg.MessageId, txn);
				}

				// the messages are pending ack state and can't be received
				var message = consumer.Receive(2, TimeUnit.SECONDS);
				Assert.Null(message);

				// 1) txn abort
				txn.Abort();

				// after transaction abort, the messages could be received
				User.Transaction commitTxn = Txn;
				Thread.Sleep(TimeSpan.FromSeconds(30));
				for(int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive(2, TimeUnit.SECONDS);
					Assert.NotNull(message);
					consumer.Acknowledge(message.MessageId, commitTxn);
					_output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
				}

				// 2) ack committed by a new txn
				commitTxn.Commit();

				// after transaction commit, the messages can't be received
				message = consumer.Receive(2, TimeUnit.SECONDS);
				Assert.Null(message);

				try
				{
					commitTxn.Commit();
					//fail("recommit one transaction should be failed.");
				}
				catch(Exception reCommitError)
				{
					// recommit one transaction should be failed
					//log.info("expected exception for recommit one transaction.");
					Assert.NotNull(reCommitError);
					Assert.True(reCommitError.InnerException is TransactionCoordinatorClientException.InvalidTxnStatusException);
				}
			}
		}

		[Fact]
		public void TxnAckTestSharedSub()
		{
			string normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")				
				.SubscriptionType(SubType.Shared);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>();
			producerBuilder.Topic(normalTopic);;

			var producer = _client.NewProducer(producerBuilder);

			for(int retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				User.Transaction txn = Txn;

				int messageCnt = 50;
				// produce normal messages
				for(int i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value("hello".GetBytes()).Send();
				}

				// consume and ack messages with txn
				for(int i = 0; i < messageCnt; i++)
				{
					var msg = consumer.Receive();
					Assert.NotNull(msg);
					_output.WriteLine($"receive msgId: {msg.MessageId}, count : {i}");
					//consumer.Acknowledge(msg.MessageId, txn);
				}

				// the messages are pending ack state and can't be received
				var message = consumer.Receive(2, TimeUnit.SECONDS);
				Assert.Null(message);

				// 1) txn abort
				txn.Abort();

				// after transaction abort, the messages could be received
				User.Transaction commitTxn = Txn;
				//Thread.Sleep(TimeSpan.FromSeconds(30));
				for(int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive(2, TimeUnit.SECONDS);
					Assert.NotNull(message);
					consumer.Acknowledge(message.MessageId, commitTxn);
					_output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
				}

				// 2) ack committed by a new txn
				commitTxn.Commit();

				// after transaction commit, the messages can't be received
				message = consumer.Receive(2, TimeUnit.SECONDS);
				Assert.Null(message);

				try
				{
					commitTxn.Commit();
					//fail("recommit one transaction should be failed.");
				}
				catch(Exception reCommitError)
				{
					// recommit one transaction should be failed
					//log.info("expected exception for recommit one transaction.");
					Assert.NotNull(reCommitError);
					Assert.True(reCommitError.InnerException is TransactionCoordinatorClientException.InvalidTxnStatusException);
				}
			}
		}
		[Fact]
		public void TxnAckTestFailoverSub()
		{
			string normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")				
				.SubscriptionType(SubType.Failover);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>();
			producerBuilder.Topic(normalTopic);;

			var producer = _client.NewProducer(producerBuilder);

			for(int retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				User.Transaction txn = Txn;

				int messageCnt = 50;
				// produce normal messages
				for(int i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value("hello".GetBytes()).Send();
				}

				// consume and ack messages with txn
				for(int i = 0; i < messageCnt; i++)
				{
					var msg = consumer.Receive();
					Assert.NotNull(msg);
					_output.WriteLine($"receive msgId: {msg.MessageId}, count : {i}");
					//consumer.Acknowledge(msg.MessageId, txn);
				}

				// the messages are pending ack state and can't be received
				var message = consumer.Receive(2, TimeUnit.SECONDS);
				Assert.Null(message);

				// 1) txn abort
				txn.Abort();

				// after transaction abort, the messages could be received
				User.Transaction commitTxn = Txn;
				//Thread.Sleep(TimeSpan.FromSeconds(30));
				for(int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive(2, TimeUnit.SECONDS);
					Assert.NotNull(message);
					consumer.Acknowledge(message.MessageId, commitTxn);
					_output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
				}

				// 2) ack committed by a new txn
				commitTxn.Commit();

				// after transaction commit, the messages can't be received
				message = consumer.Receive(2, TimeUnit.SECONDS);
				Assert.Null(message);

				try
				{
					commitTxn.Commit();
					//fail("recommit one transaction should be failed.");
				}
				catch(Exception reCommitError)
				{
					// recommit one transaction should be failed
					//log.info("expected exception for recommit one transaction.");
					Assert.NotNull(reCommitError);
					Assert.True(reCommitError.InnerException is TransactionCoordinatorClientException.InvalidTxnStatusException);
				}
			}
		}
		
		[Fact]
		public virtual void TxnMessageAckTest()
		{
			string topic = $"{_topicMessageAckTest}-{Guid.NewGuid()}";
			var subName = $"test-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic(topic)
				.SubscriptionName(subName)
				.EnableBatchIndexAcknowledgment(true)
				.AcknowledgmentGroupTime(0);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>()
				.Topic(topic)
				.EnableBatching(false)
				.SendTimeout(0);

			var producer = _client.NewProducer(producerBuilder);

			User.Transaction txn = Txn;

			int messageCnt = 10;
			for(int i = 0; i < messageCnt; i++)
			{
				producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i).ToSBytes()).Send();
			}
			_output.WriteLine("produce transaction messages finished");

			// Can't receive transaction messages before commit.
			var message = consumer.Receive(5000);
			Assert.Null(message);
			_output.WriteLine("transaction messages can't be received before transaction committed");

			txn.Commit();

			int ackedMessageCount = 0;
			int receiveCnt = 0;
			for (int i = 0; i < messageCnt; i++)
			{
				message = consumer.Receive();
				Assert.NotNull(message);
				receiveCnt++;
				if(i % 2 == 0)
				{
					consumer.Acknowledge(message);
					ackedMessageCount++;
				}
			}
			Assert.Equal(messageCnt, receiveCnt);

			message = consumer.Receive(5000);
			Assert.Null(message);

			consumer.RedeliverUnacknowledgedMessages();

			receiveCnt = 0;
			for(int i = 0; i < messageCnt - ackedMessageCount; i++)
			{
				message = consumer.Receive(2000);
				Assert.NotNull(message);
				consumer.Acknowledge(message);
				receiveCnt++;
			}
			Assert.Equal(messageCnt - ackedMessageCount, receiveCnt);

			message = consumer.Receive(2, TimeUnit.SECONDS);
			Assert.Null(message);
			_output.WriteLine($"receive transaction messages count: {receiveCnt}");
		}
		[Fact]
		public void TxnCumulativeAckTest()
		{
			string normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";
			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.SubscriptionType(SubType.Failover)
				.AckTimeout(5000, TimeUnit.MILLISECONDS);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>()
				.Topic(normalTopic);

			var producer = _client.NewProducer(producerBuilder);

			for(int retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				User.Transaction abortTxn = Txn;
				int messageCnt = 100;
				// produce normal messages
				for(int i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value(Encoding.UTF8.GetBytes("Hello").ToSBytes()).Send();
				}
				IMessage<sbyte[]> message = null;
				for(int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive();
					Assert.NotNull(message);
					if(i % 3 == 0)
					{
						consumer.AcknowledgeCumulative(message.MessageId, abortTxn);
					}
					_output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
				}
				try
				{
					consumer.AcknowledgeCumulative(message.MessageId, abortTxn);
					//fail("not ack conflict ");
				}
				catch(Exception e)
				{
					Assert.True(e.InnerException is PulsarClientException.TransactionConflictException);
				}

				try
				{
					consumer.AcknowledgeCumulative(DefaultImplementation.NewMessageId(((MessageId) message.MessageId).LedgerId, ((MessageId) message.MessageId).EntryId - 1, -1, -1), abortTxn);
					
				}
				catch(Exception e)
				{
					Assert.True(e.InnerException is PulsarClientException.TransactionConflictException);
				}

				// the messages are pending ack state and can't be received
				message = consumer.Receive(2, TimeUnit.SECONDS);
				Assert.Null(message);

				abortTxn.Abort();
				//Thread.Sleep(TimeSpan.FromSeconds(30));
				User.Transaction commitTxn = Txn;
				for(int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive();
					Assert.NotNull(message);
					if(i % 3 == 0)
					{
						consumer.AcknowledgeCumulative(message.MessageId, commitTxn);
					}
					_output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
				}

				commitTxn.Commit();
				try
				{
					commitTxn.Commit();
					//fail("recommit one transaction should be failed.");
				}
				catch(Exception reCommitError)
				{
					// recommit one transaction should be failed
					_output.WriteLine("expected exception for recommit one transaction.");
					Assert.NotNull(reCommitError);
					Assert.True(reCommitError.InnerException is TransactionCoordinatorClientException.InvalidTxnStatusException);
				}

				message = consumer.Receive(1, TimeUnit.SECONDS);
				Assert.Null(message);
			}
		}
		[Fact]
		public void TxnCumulativeAckTestBatched()
		{
			string normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";
			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.EnableBatchIndexAcknowledgment(true)
				.SubscriptionType(SubType.Failover)
				.AckTimeout(5000, TimeUnit.MILLISECONDS);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.EnableBatching(true)
				.BatchingMaxMessages(100)
				.BatchingMaxPublishDelay(1000);

			var producer = _client.NewProducer(producerBuilder);

			for(int retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				User.Transaction abortTxn = Txn;
				int messageCnt = 100;
				// produce normal messages
				for(int i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value(Encoding.UTF8.GetBytes("Hello").ToSBytes()).Send();
				}
				IMessage<sbyte[]> message = null;

				for(int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive();
					Assert.NotNull(message);
					if(i % 3 == 0)
					{
						consumer.AcknowledgeCumulative(message.MessageId, abortTxn);
					}
					_output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
				}
				try
				{
					consumer.AcknowledgeCumulative(message.MessageId, abortTxn);
					//fail("not ack conflict ");
				}
				catch(Exception e)
				{
					Assert.True(e.InnerException is PulsarClientException.TransactionConflictException);
				}

				try
				{
					consumer.AcknowledgeCumulative(DefaultImplementation.NewMessageId(((MessageId) message.MessageId).LedgerId, ((MessageId) message.MessageId).EntryId - 1, -1, -1), abortTxn);
					
				}
				catch(Exception e)
				{
					Assert.True(e.InnerException is PulsarClientException.TransactionConflictException);
				}

				// the messages are pending ack state and can't be received
				message = consumer.Receive(2, TimeUnit.SECONDS);
				Assert.Null(message);

				abortTxn.Abort();
				//Thread.Sleep(TimeSpan.FromSeconds(30));
				User.Transaction commitTxn = Txn;
				for(int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive();
					Assert.NotNull(message);
					if(i % 3 == 0)
					{
						consumer.AcknowledgeCumulative(message.MessageId, commitTxn);
					}
					_output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
				}

				commitTxn.Commit();
				try
				{
					commitTxn.Commit();
					//fail("recommit one transaction should be failed.");
				}
				catch(Exception reCommitError)
				{
					// recommit one transaction should be failed
					_output.WriteLine("expected exception for recommit one transaction.");
					Assert.NotNull(reCommitError);
					Assert.True(reCommitError.InnerException is TransactionCoordinatorClientException.InvalidTxnStatusException);
				}

				message = consumer.Receive(1, TimeUnit.SECONDS);
				Assert.Null(message);
			}
		}

		private User.Transaction Txn
		{
			get
			{
				return (User.Transaction)_client.NewTransaction().WithTransactionTimeout(2, TimeUnit.SECONDS).Build();
			}
		}

	}

}