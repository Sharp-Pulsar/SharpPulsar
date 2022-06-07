using System;
using System.Text;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Test.Transaction.Fixture;
using SharpPulsar.TestContainer;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Transaction
{
    [Collection(nameof(TransactionCollection))]
	public class TxnMessageAck:IDisposable
    {
		private const string TENANT = "public";
		private static readonly string _nAMESPACE1 = TENANT + "/default";
		private static readonly string _topicOutput = _nAMESPACE1 + $"/output-{Guid.NewGuid()}";
		private static readonly string _topicMessageAckTest = _nAMESPACE1 + "/message-ack-test";

		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;
        public TxnMessageAck(ITestOutputHelper output, PulsarFixture fixture)
		{
			_output = output;
            _client = fixture.PulsarSystem.NewClient();
        }
		[Fact]
		public async Task TxnMessageAckTest()
		{
			var topic = $"{_topicMessageAckTest}-{Guid.NewGuid()}";
			var subName = $"test-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(topic)
				.SubscriptionName(subName)
                .ForceTopicCreation(true)
				.EnableBatchIndexAcknowledgment(true)
				.AcknowledgmentGroupTime(TimeSpan.Zero);

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(topic)
				.EnableBatching(false)
				.SendTimeout(TimeSpan.Zero);

			var producer = await _client.NewProducerAsync(producerBuilder);

            var consumer = await _client.NewConsumerAsync(consumerBuilder);

            var txn = await Txn();

			var messageCnt = 10;
			for (var i = 0; i < messageCnt; i++)
			{
				await producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).SendAsync();
			}
			_output.WriteLine("produce transaction messages finished");

			// Can't receive transaction messages before commit.
			var message = await consumer.ReceiveAsync();
			Assert.Null(message);
			_output.WriteLine("transaction messages can't be received before transaction committed");

			await txn.CommitAsync();

			var ackedMessageCount = 0;
			var receiveCnt = 0;
            await Task.Delay(TimeSpan.FromSeconds(5));
            for (var i = 0; i < messageCnt; i++)
			{
				message = await consumer.ReceiveAsync();
				Assert.NotNull(message);
				receiveCnt++;
				if (i % 2 == 0)
				{
					await consumer.AcknowledgeAsync(message);
					ackedMessageCount++;
				}
			}
			Assert.Equal(messageCnt, receiveCnt);

			message = await consumer.ReceiveAsync();
			Assert.Null(message);

			await consumer.RedeliverUnacknowledgedMessagesAsync();

			await Task.Delay(TimeSpan.FromSeconds(10));
			receiveCnt = 0;
			for (var i = 0; i < messageCnt; i++)
			{
				message = await consumer.ReceiveAsync();
				Assert.NotNull(message);
				await consumer.AcknowledgeAsync(message);
				receiveCnt++;
			}
			Assert.True(receiveCnt > 5);

			message = await consumer.ReceiveAsync();
			Assert.Null(message);
			_output.WriteLine($"receive transaction messages count: {receiveCnt}");
		}
        public void Dispose()
        {
            try
            {
                _client.Shutdown();
            }
            catch { }
        }
        private async Task<User.Transaction> Txn() => (User.Transaction)await _client.NewTransaction().WithTransactionTimeout(TimeSpan.FromMinutes(5)).BuildAsync();


    }
}
