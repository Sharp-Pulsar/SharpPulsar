using BAMCIS.Util.Concurrent;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using SharpPulsar.Extension;
using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;
using System.Threading;

namespace SharpPulsar.Test.Transaction
{
    [Collection(nameof(PulsarTests))]
    public class CumulativeAck
    {
		private const string TENANT = "public";
		private static readonly string _nAMESPACE1 = TENANT + "/default";
		private static readonly string _topicOutput = _nAMESPACE1 + $"/output-{Guid.NewGuid()}";
		private static readonly string _topicMessageAckTest = _nAMESPACE1 + "/message-ack-test";

		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;
		public CumulativeAck(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}

		[Fact]
		public void TxnCumulativeAckTest()
		{

			string normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";
			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.SubscriptionType(SubType.Failover)
				.EnableBatchIndexAcknowledgment(true)
				.AcknowledgmentGroupTime(3000)
				.AckTimeout(10000, TimeUnit.MILLISECONDS);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(normalTopic);

			var producer = _client.NewProducer(producerBuilder);

			for (int retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				User.Transaction abortTxn = Txn;
				int messageCnt = 50;
				// produce normal messages
				for (int i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value(Encoding.UTF8.GetBytes("Hello")).Send();
				}
				IMessage<byte[]> message = null;
				for (int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive(TimeSpan.FromSeconds(5));
					Assert.NotNull(message);
					if (i % 3 == 0)
					{
						consumer.AcknowledgeCumulative(message.MessageId, abortTxn);
					}
					_output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
				}
				// the messages are pending ack state and can't be received
				message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
				Assert.Null(message);

				abortTxn.Abort();
				User.Transaction commitTxn = Txn;
				for (int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive(TimeSpan.FromSeconds(20));
					Assert.NotNull(message);
					consumer.AcknowledgeCumulative(message.MessageId, commitTxn);
					_output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
				}

				commitTxn.Commit();

				message = consumer.Receive(TimeSpan.FromMilliseconds(1000));
				Assert.Null(message);
			}
		}
		[Fact]
		public void TxnCumulativeAckTestBatched()
		{

			string normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";
			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.EnableBatchIndexAcknowledgment(true)
				.SubscriptionType(SubType.Failover)
				.AcknowledgmentGroupTime(3000)
				.AckTimeout(10000, TimeUnit.MILLISECONDS);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(normalTopic)
				.EnableBatching(true)
				.BatchingMaxMessages(50)
				.BatchingMaxPublishDelay(1000);

			var producer = _client.NewProducer(producerBuilder);

			for (int retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				User.Transaction abortTxn = Txn;
				int messageCnt = 100;
				// produce normal messages
				for (int i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value(Encoding.UTF8.GetBytes("Hello")).Send();
				}
				IMessage<byte[]> message = null;

				for (int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive(TimeSpan.FromSeconds(5));
					Assert.NotNull(message);
					if (i % 3 == 0)
					{
						// throws org.apache.pulsar.transaction.common.exception.TransactionCon"org.apache.pulsar.transaction.common.exception.TransactionConflictException: [persistent://public/default/normal-topic-24636acf-51a4-4309-8f68-86354383cefe][test-b1954d51-2e93-49f9-a3b7-2f76dcdedd36] Transaction:(1,42) try to cumulative batch ack position: 14960:0 within range of current currentPosition: 14960:0
						//better done outside
						consumer.AcknowledgeCumulative(message.MessageId, abortTxn);
					}
					_output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
				}
				// consumer.AcknowledgeCumulative(message.MessageId, abortTxn);
				// the messages are pending ack state and can't be received
				message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
				Assert.Null(message);

				abortTxn.Abort();
				User.Transaction commitTxn = Txn;
				for (int i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive(TimeSpan.FromSeconds(30));
					Assert.NotNull(message);
					if (i % 3 == 0)
					{
						consumer.AcknowledgeCumulative(message.MessageId, commitTxn);
					}
					_output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
				}

				commitTxn.Commit();
				message = consumer.Receive(TimeSpan.FromMilliseconds(1000));
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
