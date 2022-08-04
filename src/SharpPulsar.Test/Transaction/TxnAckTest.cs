using System;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Common;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;

namespace SharpPulsar.Test.Transaction
{
    [Collection(nameof(PulsarCollection))]
	public class TxnAckTest
    {
		private const string TENANT = "public";
		private static readonly string _nAMESPACE1 = TENANT + "/default";
		private static readonly string _topicOutput = _nAMESPACE1 + $"/output-{Guid.NewGuid()}";
		private static readonly string _topicMessageAckTest = _nAMESPACE1 + "/message-ack-test";

		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;
        public TxnAckTest(ITestOutputHelper output, PulsarFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}

		[Fact]
		public async Task TxnAckTestBatchedFailoverSub()
		{
			var normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
                .ForceTopicCreation(true)
				.EnableBatchIndexAcknowledgment(true)
				.AcknowledgmentGroupTime(TimeSpan.FromMilliseconds(5000))
				.SubscriptionType(SubType.Failover);

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(normalTopic)
				.EnableBatching(true)
				.BatchingMaxMessages(100);

			var producer = await _client.NewProducerAsync(producerBuilder);

            var consumer = await _client.NewConsumerAsync(consumerBuilder);
            var receivedMessageCount = 0;   

            for (var retryCnt = 0; retryCnt < 1; retryCnt++)
			{
				var txn = await Txn();
				//await Task.Delay(TimeSpan.FromSeconds(30));
				var messageCnt = 100;
				// produce normal messages
				for (var i = 0; i < messageCnt; i++)
				{
					await producer.NewMessage().Value("hello".GetBytes()).SendAsync();
				}

                // consume and ack messages with txn
				await Task.Delay(TimeSpan.FromSeconds(5));
                for (var i = 0; i < messageCnt; i++)
				{
					var msg = await consumer.ReceiveAsync();
                    _output.WriteLine($"receive msgId: {msg.MessageId}, count : {i}");
                    await consumer.AcknowledgeAsync(msg.MessageId, txn);
                    receivedMessageCount++;
                }

				// the messages are pending ack state and can't be received
				var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
                Assert.Null(message);

				// 1) txn abort
				await txn.AbortAsync();

				// after transaction abort, the messages could be received
				var commitTxn = await Txn();
                await Task.Delay(TimeSpan.FromSeconds(30));
                for (var i = 0; i < messageCnt - 1; i++)
				{
					message = await consumer.ReceiveAsync();
                    await consumer.AcknowledgeAsync(message.MessageId, commitTxn);
                    _output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
                    receivedMessageCount++;
                }

				// 2) ack committed by a new txn
				await commitTxn.CommitAsync();

				// after transaction commit, the messages can't be received
				message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
				Assert.Null(message);
			}
            Assert.True(receivedMessageCount > 75);
		}

		[Fact]
		public async Task TxnAckTestBatchedSharedSub()
		{
			var normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(normalTopic)
                .ForceTopicCreation(true)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.AcknowledgmentGroupTime(TimeSpan.FromMilliseconds(5000))
				.EnableBatchIndexAcknowledgment(true)
				.SubscriptionType(SubType.Shared);

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(normalTopic)
				.EnableBatching(true)
				.BatchingMaxMessages(100);

			var producer = await _client.NewProducerAsync(producerBuilder);

            var consumer = await _client.NewConsumerAsync(consumerBuilder);
            var receivedMessageCount = 0;

            for (var retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				var txn = await Txn();

				var messageCnt = 100;
				// produce normal messages
				for (var i = 0; i < messageCnt; i++)
				{
					await producer.NewMessage().Value("hello".GetBytes()).SendAsync();
				}

                // consume and ack messages with txn
                await Task.Delay(TimeSpan.FromSeconds(5));
                for (var i = 0; i < messageCnt - 2; i++)
				{
					var msg = consumer.Receive();
                    _output.WriteLine($"receive msgId: {msg.MessageId}, count : {i}");
                    await consumer.AcknowledgeAsync(msg.MessageId, txn);
                    receivedMessageCount++;
                }

				// the messages are pending ack state and can't be received
				var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
				Assert.Null(message);

				// 1) txn abort
				await txn.AbortAsync();

				// after transaction abort, the messages could be received
				var commitTxn = await Txn();
                await Task.Delay(TimeSpan.FromSeconds(5));
                for (var i = 0; i < messageCnt - 2; i++)
				{
					message = consumer.Receive();
                    await consumer.AcknowledgeAsync(message.MessageId, commitTxn);
                    _output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
                }

				// 2) ack committed by a new txn
				await commitTxn.CommitAsync();
                await Task.Delay(TimeSpan.FromSeconds(5));

                // after transaction commit, the messages can't be received
                message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
				Assert.Null(message);
            }
            Assert.True(receivedMessageCount > 75);
        }

		[Fact]
		public async Task TxnAckTestSharedSub()
		{
			var normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";

			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(normalTopic)
                .ForceTopicCreation(true)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.AcknowledgmentGroupTime(TimeSpan.FromMilliseconds(5000))
				.SubscriptionType(SubType.Shared);

			var producerBuilder = new ProducerConfigBuilder<byte[]>();
			producerBuilder.Topic(normalTopic); ;

			var producer = await _client.NewProducerAsync(producerBuilder);

            var consumer = await _client.NewConsumerAsync(consumerBuilder);
            var receivedMessageCount = 0;

            for (var retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				var txn = await Txn();

				var messageCnt = 50;
				// produce normal messages
				for (var i = 0; i < messageCnt; i++)
				{
					await producer.NewMessage().Value("hello".GetBytes()).SendAsync();
				}

				// consume and ack messages with txn
				for (var i = 0; i < messageCnt - 2; i++)
				{
					var msg = await consumer.ReceiveAsync(); 
                    _output.WriteLine($"receive msgId: {msg.MessageId}, count : {i}");
                    await consumer.AcknowledgeAsync(msg.MessageId, txn);
                    receivedMessageCount++;
                }

				// the messages are pending ack state and can't be received
				var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
				Assert.Null(message);

				// 1) txn abort
				await txn.AbortAsync();

				// after transaction abort, the messages could be received
				var commitTxn = await Txn();
				await Task.Delay(TimeSpan.FromSeconds(30));
				for (var i = 0; i < messageCnt - 2; i++)
				{
					message = await consumer.ReceiveAsync(); 
                    await consumer.AcknowledgeAsync(message.MessageId, commitTxn);
                    _output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
                    receivedMessageCount++;
                }

				// 2) ack committed by a new txn
				await commitTxn.CommitAsync();

				// after transaction commit, the messages can't be received
				message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
				Assert.Null(message);
			}
            Assert.True(receivedMessageCount > 75);
		}

        private async Task<User.Transaction> Txn() => (User.Transaction)await _client.NewTransaction().WithTransactionTimeout(TimeSpan.FromMinutes(5)).BuildAsync();

    }
}
