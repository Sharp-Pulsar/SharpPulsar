using SharpPulsar.Configuration;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Text;
using SharpPulsar.Extension;
using Xunit;
using Xunit.Abstractions;
using System.Threading;

namespace SharpPulsar.Test.Transaction
{
    [Collection(nameof(PulsarTests))]
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
			string topic = $"{_topicMessageAckTest}-{Guid.NewGuid()}";
			var subName = $"test-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(topic)
				.SubscriptionName(subName)
				.EnableBatchIndexAcknowledgment(true)
				.AcknowledgmentGroupTime(0);

			var consumer = _client.NewConsumer(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(topic)
				.EnableBatching(false)
				.SendTimeout(0);

			var producer = _client.NewProducer(producerBuilder);

			User.Transaction txn = Txn;

			int messageCnt = 10;
			for (int i = 0; i < messageCnt; i++)
			{
				producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).Send();
			}
			_output.WriteLine("produce transaction messages finished");

			// Can't receive transaction messages before commit.
			var message = consumer.Receive(TimeSpan.FromMilliseconds(5000));
			Assert.Null(message);
			_output.WriteLine("transaction messages can't be received before transaction committed");

			txn.Commit();

			int ackedMessageCount = 0;
			int receiveCnt = 0;
			for (int i = 0; i < messageCnt; i++)
			{
				message = consumer.Receive(TimeSpan.FromSeconds(10));
				Assert.NotNull(message);
				receiveCnt++;
				if (i % 2 == 0)
				{
					consumer.Acknowledge(message);
					ackedMessageCount++;
				}
			}
			Assert.Equal(messageCnt, receiveCnt);

			message = consumer.Receive(TimeSpan.FromMilliseconds(5000));
			Assert.Null(message);

			consumer.RedeliverUnacknowledgedMessages();

			Thread.Sleep(TimeSpan.FromSeconds(30));
			receiveCnt = 0;
			for (int i = 0; i < messageCnt - ackedMessageCount; i++)
			{
				message = consumer.Receive(TimeSpan.FromMilliseconds(10000));
				Assert.NotNull(message);
				consumer.Acknowledge(message);
				receiveCnt++;
			}
			Assert.Equal(messageCnt - ackedMessageCount, receiveCnt);

			message = consumer.Receive(TimeSpan.FromMilliseconds(2000));
			Assert.Null(message);
			_output.WriteLine($"receive transaction messages count: {receiveCnt}");
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
