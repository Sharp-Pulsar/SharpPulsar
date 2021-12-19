using System;
using System.Text;
using System.Threading;
using SharpPulsar.Configuration;
using SharpPulsar.Test.Transaction.Fixtures;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Transaction.Transaction
{
    [Collection(nameof(PulsarTransactionTests))]
	public class TxnMessageAck
    {
		private const string TENANT = "public";
		private static readonly string _nAMESPACE1 = TENANT + "/default";
		private static readonly string _topicOutput = _nAMESPACE1 + $"/output-{Guid.NewGuid()}";
		private static readonly string _topicMessageAckTest = _nAMESPACE1 + "/message-ack-test";

		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;
        public TxnMessageAck(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}
		[Fact]
		public void TxnMessageAckTest()
		{
			var topic = $"{_topicMessageAckTest}-{Guid.NewGuid()}";
			var subName = $"test-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(topic)
				.SubscriptionName(subName)
                .ForceTopicCreation(true)
				.EnableBatchIndexAcknowledgment(true)
				.AcknowledgmentGroupTime(TimeSpan.Zero);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(topic)
				.EnableBatching(false)
				.SendTimeout(0);

			var producer = _client.NewProducer(producerBuilder);

			var txn = Txn;

			var messageCnt = 10;
			for (var i = 0; i < messageCnt; i++)
			{
				producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).Send();
			}
			_output.WriteLine("produce transaction messages finished");

			// Can't receive transaction messages before commit.
			var message = consumer.Receive();
			Assert.Null(message);
			_output.WriteLine("transaction messages can't be received before transaction committed");

			txn.Commit();

			var ackedMessageCount = 0;
			var receiveCnt = 0;
            Thread.Sleep(TimeSpan.FromSeconds(5));
            for (var i = 0; i < messageCnt; i++)
			{
				message = consumer.Receive();
				Assert.NotNull(message);
				receiveCnt++;
				if (i % 2 == 0)
				{
					consumer.Acknowledge(message);
					ackedMessageCount++;
				}
			}
			Assert.Equal(messageCnt, receiveCnt);

			message = consumer.Receive();
			Assert.Null(message);

			consumer.RedeliverUnacknowledgedMessages();

			Thread.Sleep(TimeSpan.FromSeconds(10));
			receiveCnt = 0;
			for (var i = 0; i < messageCnt; i++)
			{
				message = consumer.Receive();
				Assert.NotNull(message);
				consumer.Acknowledge(message);
				receiveCnt++;
			}
			Assert.Equal(messageCnt - ackedMessageCount, receiveCnt);

			message = consumer.Receive();
			Assert.Null(message);
			_output.WriteLine($"receive transaction messages count: {receiveCnt}");
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
