using System;
using System.Text;
using System.Threading;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Test.Transaction.Fixtures;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;

namespace SharpPulsar.Test.Transaction.Transaction
{
    [Collection(nameof(PulsarTransactionTests))]
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

			var normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";
			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
                .ForceTopicCreation(true)
				//.SubscriptionType(SubType.Failover)
				.EnableBatchIndexAcknowledgment(true)
				.AcknowledgmentGroupTime(3000)
				.AckTimeout(TimeSpan.FromMilliseconds(10000));

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(normalTopic);

			var producer = _client.NewProducer(producerBuilder);

			for (var retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				var abortTxn = Txn;
				var messageCnt = 50;
				// produce normal messages
				for (var i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value(Encoding.UTF8.GetBytes("Hello")).Send();
				}
				IMessage<byte[]> message = null;
                Thread.Sleep(TimeSpan.FromSeconds(5));
				for (var i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive();
					Assert.NotNull(message);
					if (i % 3 == 0)
					{
						consumer.AcknowledgeCumulative(message.MessageId, abortTxn);
					}
					_output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
				}
				// the messages are pending ack state and can't be received
				message = consumer.Receive();
				Assert.Null(message);

				abortTxn.Abort();
				var commitTxn = Txn;
                Thread.Sleep(TimeSpan.FromSeconds(5));
                for (var i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive();
					Assert.NotNull(message);
					consumer.AcknowledgeCumulative(message.MessageId, commitTxn);
					_output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
				}

				commitTxn.Commit();
                Thread.Sleep(TimeSpan.FromSeconds(5));

                message = consumer.Receive();
				Assert.Null(message);
			}
		}
		[Fact]
		public void TxnCumulativeAckTestBatched()
		{

			var normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";
			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(normalTopic)
                .ForceTopicCreation(true)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.EnableBatchIndexAcknowledgment(true)
				.SubscriptionType(SubType.Failover)
				.AcknowledgmentGroupTime(3000)
				.AckTimeout(TimeSpan.FromMilliseconds(10000));

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(normalTopic)
				.EnableBatching(true)
				.BatchingMaxMessages(50)
				.BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(1000));

			var producer = _client.NewProducer(producerBuilder);

			for (var retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				var abortTxn = Txn;
				var messageCnt = 100;
				// produce normal messages
				for (var i = 0; i < messageCnt; i++)
				{
					producer.NewMessage().Value(Encoding.UTF8.GetBytes("Hello")).Send();
				}
				IMessage<byte[]> message = null;
                Thread.Sleep(TimeSpan.FromSeconds(5));

                for (var i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive();
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
				message = consumer.Receive();
				Assert.Null(message);

				abortTxn.Abort();
				var commitTxn = Txn;
                Thread.Sleep(TimeSpan.FromSeconds(5));
                for (var i = 0; i < messageCnt; i++)
				{
					message = consumer.Receive();
					Assert.NotNull(message);
					if (i % 3 == 0)
					{
						consumer.AcknowledgeCumulative(message.MessageId, commitTxn);
					}
					_output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
				}

				commitTxn.Commit();
                Thread.Sleep(TimeSpan.FromSeconds(5));
                message = consumer.Receive();
				Assert.Null(message);
			}
		}


        private User.Transaction Txn
		{

			get
			{
				return (User.Transaction)_client.NewTransaction().WithTransactionTimeout(TimeSpan.FromMinutes(5)).Build();
			}
		}
	}
}
