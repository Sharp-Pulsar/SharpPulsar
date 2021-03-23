using SharpPulsar.User;
using System;
using Xunit.Abstractions;
using SharpPulsar.Common;
using SharpPulsar.Test.Fixtures;
using Xunit;
using SharpPulsar.Configuration;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;

namespace SharpPulsar.Test.Transaction
{
    [Collection(nameof(PulsarTests))]
	public class TxnAckTest
    {
		private const string TENANT = "public";
		private static readonly string _nAMESPACE1 = TENANT + "/default";
		private static readonly string _topicOutput = _nAMESPACE1 + $"/output-{Guid.NewGuid()}";
		private static readonly string _topicMessageAckTest = _nAMESPACE1 + "/message-ack-test";

		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;
        public TxnAckTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}

		[Fact]
		public void TxnAckTestBatchedFailoverSub()
		{
			string normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.EnableBatchIndexAcknowledgment(true)
				.AcknowledgmentGroupTime(5000)
				.SubscriptionType(SubType.Failover);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.EnableBatching(true)
				.BatchingMaxMessages(100);

			var producer = _client.NewProducer(producerBuilder);

			for (int retryCnt = 0; retryCnt < 1; retryCnt++)
			{
				User.Transaction txn = Txn;
				//Thread.Sleep(TimeSpan.FromSeconds(30));
				int messageCnt = 100;
				// produce normal messages
				for (int i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value("hello".GetBytes()).Send();
				}

				// consume and ack messages with txn
				for (int i = 0; i < messageCnt; i++)
				{
					var msg = consumer.Receive(TimeSpan.FromSeconds(5));
					Assert.NotNull(msg);
					_output.WriteLine($"receive msgId: {msg.MessageId}, count : {i}");
					consumer.Acknowledge(msg.MessageId, txn);
				}

				// the messages are pending ack state and can't be received
				var message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
				Assert.Null(message);

				// 1) txn abort
				txn.Abort();

				// after transaction abort, the messages could be received
				User.Transaction commitTxn = Txn;
				//Thread.Sleep(TimeSpan.FromSeconds(30));
				for (int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
					Assert.NotNull(message);
					consumer.Acknowledge(message.MessageId, commitTxn);
					_output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
				}

				// 2) ack committed by a new txn
				commitTxn.Commit();

				// after transaction commit, the messages can't be received
				message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
				Assert.Null(message);
			}
		}

		[Fact]
		public void TxnAckTestBatchedSharedSub()
		{
			string normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.AcknowledgmentGroupTime(5000)
				.EnableBatchIndexAcknowledgment(true)
				.SubscriptionType(SubType.Shared);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.EnableBatching(true)
				.BatchingMaxMessages(100);

			var producer = _client.NewProducer(producerBuilder);

			for (int retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				User.Transaction txn = Txn;

				int messageCnt = 100;
				// produce normal messages
				for (int i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value("hello".GetBytes()).Send();
				}

				// consume and ack messages with txn
				for (int i = 0; i < messageCnt; i++)
				{
					var msg = consumer.Receive(TimeSpan.FromSeconds(5));
					Assert.NotNull(msg);
					_output.WriteLine($"receive msgId: {msg.MessageId}, count : {i}");
					consumer.Acknowledge(msg.MessageId, txn);
				}

				// the messages are pending ack state and can't be received
				var message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
				Assert.Null(message);

				// 1) txn abort
				txn.Abort();

				// after transaction abort, the messages could be received
				User.Transaction commitTxn = Txn;
				for (int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
					Assert.NotNull(message);
					consumer.Acknowledge(message.MessageId, commitTxn);
					_output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
				}

				// 2) ack committed by a new txn
				commitTxn.Commit();

				// after transaction commit, the messages can't be received
				message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
				Assert.Null(message);
			}
		}

		[Fact]
		public void TxnAckTestSharedSub()
		{
			string normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.AcknowledgmentGroupTime(5000)
				.SubscriptionType(SubType.Shared);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>();
			producerBuilder.Topic(normalTopic); ;

			var producer = _client.NewProducer(producerBuilder);

			for (int retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				User.Transaction txn = Txn;

				int messageCnt = 50;
				// produce normal messages
				for (int i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value("hello".GetBytes()).Send();
				}

				// consume and ack messages with txn
				for (int i = 0; i < messageCnt; i++)
				{
					var msg = consumer.Receive(TimeSpan.FromSeconds(5));
					Assert.NotNull(msg);
					_output.WriteLine($"receive msgId: {msg.MessageId}, count : {i}");
					consumer.Acknowledge(msg.MessageId, txn);
				}

				// the messages are pending ack state and can't be received
				var message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
				Assert.Null(message);

				// 1) txn abort
				txn.Abort();

				// after transaction abort, the messages could be received
				User.Transaction commitTxn = Txn;
				//Thread.Sleep(TimeSpan.FromSeconds(30));
				for (int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
					Assert.NotNull(message);
					consumer.Acknowledge(message.MessageId, commitTxn);
					_output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
				}

				// 2) ack committed by a new txn
				commitTxn.Commit();

				// after transaction commit, the messages can't be received
				message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
				Assert.Null(message);
			}
		}
		[Fact]
		public void TxnAckTestFailoverSub()
		{
			string normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<sbyte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.AcknowledgmentGroupTime(5000)
				.SubscriptionType(SubType.Failover);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<sbyte[]>();
			producerBuilder.Topic(normalTopic); 

			var producer = _client.NewProducer(producerBuilder);

			for (int retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				User.Transaction txn = Txn;

				int messageCnt = 50;
				// produce normal messages
				for (int i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value("hello".GetBytes()).Send();
				}

				// consume and ack messages with txn
				for (int i = 0; i < messageCnt; i++)
				{
					var msg = consumer.Receive(TimeSpan.FromSeconds(5));
					Assert.NotNull(msg);
					_output.WriteLine($"receive msgId: {msg.MessageId}, count : {i}");
					consumer.Acknowledge(msg.MessageId, txn);
				}

				// the messages are pending ack state and can't be received
				var message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
				Assert.Null(message);

				// 1) txn abort
				txn.Abort();

				// after transaction abort, the messages could be received
				User.Transaction commitTxn = Txn;
				//Thread.Sleep(TimeSpan.FromSeconds(30));
				for (int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
					Assert.NotNull(message);
					consumer.Acknowledge(message.MessageId, commitTxn);
					_output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
				}

				// 2) ack committed by a new txn
				commitTxn.Commit();

				// after transaction commit, the messages can't be received
				message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
				Assert.Null(message);
			}
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
