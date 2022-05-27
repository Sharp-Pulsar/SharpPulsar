using System;
using System.Text;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;

namespace SharpPulsar.Test.Transaction
{
    [Collection(nameof(TransactionCollection))]
    public class CumulativeAck
    {
		private const string TENANT = "public";
		private static readonly string _nAMESPACE1 = TENANT + "/default";
		private static readonly string _topicOutput = _nAMESPACE1 + $"/output-{Guid.NewGuid()}";
		private static readonly string _topicMessageAckTest = _nAMESPACE1 + "/message-ack-test";

		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;
		public CumulativeAck(ITestOutputHelper output, TransactionCollection fixture)
		{
			_output = output;
			_client = fixture.Client;
		}

		[Fact]
		public async Task TxnCumulativeAckTest()
		{

			var normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";
			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(normalTopic)
				.SubscriptionName($"test-{Guid.NewGuid()}")
                .ForceTopicCreation(true)
				//.SubscriptionType(SubType.Failover)
				.EnableBatchIndexAcknowledgment(true)
				.AcknowledgmentGroupTime(TimeSpan.FromMilliseconds(3000))
				.AckTimeout(TimeSpan.FromMilliseconds(10000));

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(normalTopic);

			var producer = await _client.NewProducerAsync(producerBuilder);

            var consumer = await _client.NewConsumerAsync(consumerBuilder);
            var receivedMessageCount = 0;

            for (var retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				var abortTxn = await Txn();
				var messageCnt = 50;
				// produce normal messages
				for (var i = 0; i < messageCnt; i++)
				{
					await producer.NewMessage().Value(Encoding.UTF8.GetBytes("Hello")).SendAsync();
				}
				IMessage<byte[]> message = null;
                await Task.Delay(TimeSpan.FromSeconds(5));
				for (var i = 0; i < messageCnt; i++)
				{
					message = await consumer.ReceiveAsync();
					if(message != null)
                    {
                        if (i % 3 == 0)
                        {
                            await consumer.AcknowledgeCumulativeAsync(message.MessageId, abortTxn);
                        }
                        _output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
                        receivedMessageCount++;
                    }
				}
				// the messages are pending ack state and can't be received
				message = await consumer.ReceiveAsync();
				Assert.Null(message);

				await abortTxn.AbortAsync();
				var commitTxn = await Txn();
                await Task.Delay(TimeSpan.FromSeconds(5));
                for (var i = 0; i < messageCnt; i++)
				{
					message = await consumer.ReceiveAsync();
					if(message != null)
                    {
                        await consumer.AcknowledgeCumulativeAsync(message.MessageId, commitTxn);
                        _output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
                        receivedMessageCount++;
                    }
				}

				await commitTxn.CommitAsync();
                await Task.Delay(TimeSpan.FromSeconds(5));

                message = await consumer.ReceiveAsync();
                Assert.Null(message);
                Assert.True(receivedMessageCount > 45);
            }
		}
		[Fact]
		public async Task TxnCumulativeAckTestBatched()
		{

			var normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";
			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(normalTopic)
                .ForceTopicCreation(true)
				.SubscriptionName($"test-{Guid.NewGuid()}")
				.EnableBatchIndexAcknowledgment(true)
				.SubscriptionType(SubType.Failover)
				.AcknowledgmentGroupTime(TimeSpan.FromMilliseconds(3000))
				.AckTimeout(TimeSpan.FromMilliseconds(10000));

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(normalTopic)
				.EnableBatching(true)
				.BatchingMaxMessages(50)
				.BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(1000));

			var producer = await _client.NewProducerAsync(producerBuilder);

            var consumer = await _client.NewConsumerAsync(consumerBuilder);
            var receivedMessageCount = 0;

            for (var retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				var abortTxn = await Txn();
				var messageCnt = 100;
				// produce normal messages
				for (var i = 0; i < messageCnt; i++)
				{
					await producer.NewMessage().Value(Encoding.UTF8.GetBytes("Hello")).SendAsync();
				}
				IMessage<byte[]> message = null;
                await Task.Delay(TimeSpan.FromSeconds(5));

                for (var i = 0; i < messageCnt; i++)
				{
					message = await consumer.ReceiveAsync();
                    if(message != null)
                    {
                        if (i % 3 == 0)
                        {
                            // throws org.apache.pulsar.transaction.common.exception.TransactionCon"org.apache.pulsar.transaction.common.exception.TransactionConflictException: [persistent://public/default/normal-topic-24636acf-51a4-4309-8f68-86354383cefe][test-b1954d51-2e93-49f9-a3b7-2f76dcdedd36] Transaction:(1,42) try to cumulative batch ack position: 14960:0 within range of current currentPosition: 14960:0
                            //better done outside
                            await consumer.AcknowledgeCumulativeAsync(message.MessageId, abortTxn);
                        }
                        _output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
                        receivedMessageCount++;
                    }
				}
				// consumer.AcknowledgeCumulative(message.MessageId, abortTxn);
				// the messages are pending ack state and can't be received
				message = await consumer.ReceiveAsync();
                Assert.Null(message);

				await abortTxn.AbortAsync();
				var commitTxn = await Txn();
                await Task.Delay(TimeSpan.FromSeconds(5));
                for (var i = 0; i < messageCnt; i++)
				{
					message = await consumer.ReceiveAsync();
                    if (message != null)
                    {
                        if (i % 3 == 0)
                        {
                            await consumer.AcknowledgeCumulativeAsync(message.MessageId, commitTxn);
                        }
                        _output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : {retryCnt}, count : {i}");
                        receivedMessageCount++;
                    }
				}

				await commitTxn.CommitAsync();
                await Task.Delay(TimeSpan.FromSeconds(5));
                message = await consumer.ReceiveAsync();
                Assert.Null(message);
                Assert.True(receivedMessageCount > 75);
			}
		}

        private async Task<User.Transaction> Txn() => (User.Transaction)await _client.NewTransaction().WithTransactionTimeout(TimeSpan.FromMinutes(5)).BuildAsync();


    }
}
