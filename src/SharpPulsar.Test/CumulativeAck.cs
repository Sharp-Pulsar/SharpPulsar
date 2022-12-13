using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Common;
using SharpPulsar.Interfaces;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using Xunit;
using Xunit.Abstractions;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;

namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarCollection))]
    public class CumulativeAck
    {
        private const string TENANT = "public";
        private static readonly string _nAMESPACE1 = TENANT + "/default";
        private static readonly string _topicOutput = _nAMESPACE1 + $"/output-{Guid.NewGuid()}";
        private static readonly string _topicMessageAckTest = _nAMESPACE1 + "/message-ack-test";

        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;

        public CumulativeAck(ITestOutputHelper output, PulsarFixture fixture)
        {

            _output = output;
            _client = fixture.System.NewClient(fixture.ConfigBuilder).AsTask().GetAwaiter().GetResult();
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

            var abortTxn = await Txn().ConfigureAwait(false);
            var messageCnt = 50;
            // produce normal messages
            for (var i = 0; i < messageCnt; i++)
            {
                await producer.NewMessage().Value(Encoding.UTF8.GetBytes("Hello")).SendAsync().ConfigureAwait(false);
            }
            IMessage<byte[]> message = null;
            //await Task.Delay(TimeSpan.FromSeconds(5));
            for (var i = 0; i < messageCnt; i++)
            {
                message = await consumer.ReceiveAsync().ConfigureAwait(false);
                if (message != null)
                {
                    if (i % 3 == 0)
                    {
                        await consumer.AcknowledgeCumulativeAsync(message.MessageId, abortTxn).ConfigureAwait(false);
                    }
                    _output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : 1, count : {i}");
                    receivedMessageCount++;
                }
            }
            // the messages are pending ack state and can't be received
            message = await consumer.ReceiveAsync().ConfigureAwait(false);
            Assert.Null(message);

            await abortTxn.AbortAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(5));
            var commitTxn = await Txn().ConfigureAwait(false);
            for (var i = 0; i < messageCnt; i++)
            {
                message = await consumer.ReceiveAsync().ConfigureAwait(false);
                if (message != null)
                {
                    await consumer.AcknowledgeCumulativeAsync(message.MessageId, commitTxn).ConfigureAwait(false);
                    _output.WriteLine($"receive msgId abort: {message.MessageId}, retryCount : 1, count : {i}");
                    receivedMessageCount++;
                }
            }

            await commitTxn.CommitAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(5));

            message = await consumer.ReceiveAsync().ConfigureAwait(false);
            if (receivedMessageCount < 45)
                Assert.Null(message);

            Assert.True(receivedMessageCount > 45);
            _client.Dispose();
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
                var abortTxn = await Txn().ConfigureAwait(false);
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
                    if (message != null)
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
                message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
                Assert.Null(message);

                await abortTxn.AbortAsync();
                var commitTxn = await Txn().ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(5));
                for (var i = 0; i < messageCnt; i++)
                {
                    message = await consumer.ReceiveAsync(TimeSpan.FromSeconds(1));
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
                message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
                if (receivedMessageCount < 75)
                    Assert.Null(message);
                Assert.True(receivedMessageCount > 75);
            }
            _client.Dispose();
        }
        [Fact]
        public async Task ProduceCommitTest()
        {
            var txn1 = await Txn();
            var txn2 = await Txn(); ;
            var topic = $"{_topicOutput}";

            var producerBuilder = new ProducerConfigBuilder<byte[]>()
                .Topic(topic)
                .SendTimeout(TimeSpan.Zero);

            var producer = await _client.NewProducerAsync(producerBuilder);

            var txnMessageCnt = 0;
            var messageCnt = 10;
            for (var i = 0; i < messageCnt; i++)
            {
                if (i % 5 == 0)
                    await producer.NewMessage(txn1).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).SendAsync();
                else
                    await producer.NewMessage(txn2).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).SendAsync();

                txnMessageCnt++;
            }
            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
               .Topic(topic)
               .ForceTopicCreation(true)
               .SubscriptionName($"test-{Guid.NewGuid()}");

            var consumer = await _client.NewConsumerAsync(consumerBuilder);

            // Can't receive transaction messages before commit.
            var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
            Assert.Null(message);

            await txn1.CommitAsync();
            _output.WriteLine($"Committed 1");
            await txn2.CommitAsync();
            _output.WriteLine($"Committed 2");
            // txn1 messages could be received after txn1 committed
            var receiveCnt = 0;
            await Task.Delay(TimeSpan.FromSeconds(5));
            for (var i = 0; i < txnMessageCnt; i++)
            {
                message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
                Assert.NotNull(message);
                _output.WriteLine(Encoding.UTF8.GetString(message.Value));
                receiveCnt++;
            }

            for (var i = 0; i < txnMessageCnt; i++)
            {
                message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
                if (message != null)
                    receiveCnt++;
            }
            Assert.True(receiveCnt > 8);

            message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
            Assert.Null(message);

            _output.WriteLine($"message commit test enableBatch {true}");
            _client.Dispose();
        }
        [Fact]
        public async Task ProduceCommitBatchedTest()
        {

            var txn = await Txn();

            var topic = $"{_topicOutput}-{Guid.NewGuid()}";

            var producerBuilder = new ProducerConfigBuilder<byte[]>()
                .Topic(topic)
                .EnableBatching(true)
                .BatchingMaxMessages(10)
                .SendTimeout(TimeSpan.Zero);

            var producer = await _client.NewProducerAsync(producerBuilder);

            var txnMessageCnt = 0;
            var messageCnt = 40;
            for (var i = 0; i < messageCnt; i++)
            {
                await producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).SendAsync();
                txnMessageCnt++;
            }

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(topic)
                .ForceTopicCreation(true)
                .SubscriptionName($"test2{Guid.NewGuid()}")
                .EnableBatchIndexAcknowledgment(true)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest);


            var consumer = await _client.NewConsumerAsync(consumerBuilder);
            // Can't receive transaction messages before commit.
            var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
            Assert.Null(message);

            await txn.CommitAsync();

            // txn1 messages could be received after txn1 committed
            var receiveCnt = 0;
            await Task.Delay(TimeSpan.FromSeconds(5));
            for (var i = 0; i < txnMessageCnt; i++)
            {
                message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
                Assert.NotNull(message);
                receiveCnt++;
                _output.WriteLine($"message receive count: {receiveCnt}");
            }
            Assert.Equal(txnMessageCnt, receiveCnt);

            message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
            Assert.Null(message);

            _output.WriteLine($"message commit test enableBatch {true}");
            _client.Dispose();
        }
        [Fact]
        public async Task ProduceAbortTest()
        {
            var topic = $"{_topicOutput}-{Guid.NewGuid()}";
            var txn = await Txn();

            var producerBuilder = new ProducerConfigBuilder<byte[]>();
            producerBuilder.Topic(topic);
            producerBuilder.SendTimeout(TimeSpan.Zero);

            var producer = await _client.NewProducerAsync(producerBuilder);

            var messageCnt = 10;
            for (var i = 0; i < messageCnt; i++)
            {
                await producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).SendAsync();
            }

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(topic)
                .SubscriptionName($"test{DateTime.Now.Ticks}")
                .ForceTopicCreation(true)
                .EnableBatchIndexAcknowledgment(true)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest);

            var consumer = await _client.NewConsumerAsync(consumerBuilder);
            // Can't receive transaction messages before abort.
            var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
            Assert.Null(message);

            await txn.AbortAsync();

            // Cant't receive transaction messages after abort.
            message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
            Assert.Null(message);
            _client.Dispose();
        }

        [Fact]
        public async Task TxnAckTestBatchedFailoverSub()
        {
            var normalTopic = _nAMESPACE1 + $"/normal-topic-{Guid.NewGuid()}";

            var producerBuilder = new ProducerConfigBuilder<byte[]>()
                .Topic(normalTopic)
                .EnableBatching(true)
                .BatchingMaxMessages(100);

            var producer = await _client.NewProducerAsync(producerBuilder);

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(normalTopic)
                .SubscriptionName($"test-{Guid.NewGuid()}")
                .ForceTopicCreation(true)
                .EnableBatchIndexAcknowledgment(true)
                .AcknowledgmentGroupTime(TimeSpan.FromMilliseconds(5000))
                .SubscriptionType(SubType.Failover);

            var consumer = await _client.NewConsumerAsync(consumerBuilder);
            var receivedMessageCount = 0;

            for (var retryCnt = 0; retryCnt < 2; retryCnt++)
            {
                var txn = await Txn().ConfigureAwait(false);
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
                    try
                    {
                        var msg = await consumer.ReceiveAsync();
                        _output.WriteLine($"receive msgId: {msg.MessageId}, count : {i}");
                        await consumer.AcknowledgeAsync(msg.MessageId, txn);
                        receivedMessageCount++;
                    }
                    catch(Exception ex) 
                    {
                        _output.WriteLine($"count : {i}, {ex}");
                    }   
                    
                }

                // the messages are pending ack state and can't be received
                var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
                Assert.Null(message);

                // 1) txn abort
                await txn.AbortAsync();

                // after transaction abort, the messages could be received
                var commitTxn = await Txn().ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(10));
                for (var i = 0; i < messageCnt; i++)
                {
                    message = await consumer.ReceiveAsync();
                    if(message != null)
                    {
                        await consumer.AcknowledgeAsync(message.MessageId, commitTxn);
                        _output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
                        receivedMessageCount++;
                    }
                }

                // 2) ack committed by a new txn
                await commitTxn.CommitAsync();

                // after transaction commit, the messages can't be received
                message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
                if (receivedMessageCount < 75)
                    Assert.Null(message);
            }
            Assert.True(receivedMessageCount > 75);
            _client.Dispose();
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
                var txn = await Txn().ConfigureAwait(false);

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
                var commitTxn = await Txn().ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(5));
                for (var i = 0; i < messageCnt; i++)
                {
                    message = consumer.Receive();
                    if (message != null)
                    {
                        await consumer.AcknowledgeAsync(message.MessageId, commitTxn);
                        _output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
                    }
                    
                }

                // 2) ack committed by a new txn
                await commitTxn.CommitAsync();
                await Task.Delay(TimeSpan.FromSeconds(5));

                // after transaction commit, the messages can't be received
                message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
                if (receivedMessageCount < 75)
                    Assert.Null(message);
            }
            Assert.True(receivedMessageCount > 75);
            _client.Dispose();
        }

        [Fact(Skip = "TxnAckTestSharedSub")]
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
                var txn = await Txn().ConfigureAwait(false);

                var messageCnt = 50;
                // produce normal messages
                for (var i = 0; i < messageCnt; i++)
                {
                    await producer.NewMessage().Value("hello".GetBytes()).SendAsync();
                }

                // consume and ack messages with txn
                for (var i = 0; i < messageCnt; i++)
                {
                    var msg = await consumer.ReceiveAsync();
                    if(msg != null)
                    {
                        _output.WriteLine($"receive msgId: {msg.MessageId}, count : {i}");
                        await consumer.AcknowledgeAsync(msg.MessageId, txn);
                        receivedMessageCount++;
                    }
                    
                }

                // the messages are pending ack state and can't be received
                var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
                //Assert.Null(message);

                // 1) txn abort
                await txn.AbortAsync();

                // after transaction abort, the messages could be received
                var commitTxn = await Txn().ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(5));
                for (var i = 0; i < messageCnt - 2; i++)
                {
                    message = await consumer.ReceiveAsync();
                    if(message != null)
                    {
                        await consumer.AcknowledgeAsync(message.MessageId, commitTxn);
                        _output.WriteLine($"receive msgId: {message.MessageId}, count: {i}");
                        receivedMessageCount++;
                    }
                    
                }

                // 2) ack committed by a new txn
                await commitTxn.CommitAsync();

                // after transaction commit, the messages can't be received
                message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100));
                if (receivedMessageCount < 75)
                    Assert.Null(message);
            }
            Assert.True(receivedMessageCount > 75);
            _client.Dispose();
        }
        [Fact]
        public async Task TestUnAckMessageRedeliveryWithReceive()
        {
            var topic = $"persistent://public/default/async-unack-redelivery-{Guid.NewGuid()}";

            var pBuilder = new ProducerConfigBuilder<byte[]>();
            pBuilder.Topic(topic);
            var producer = await _client.NewProducerAsync(pBuilder);

            const int messageCount = 10;

            for (var i = 0; i < messageCount; i++)
            {
                var receipt = await producer.SendAsync(Encoding.UTF8.GetBytes("my-message-" + i));
                _output.WriteLine(JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
            }

            var builder = new ConsumerConfigBuilder<byte[]>();
            builder.Topic(topic);
            builder.SubscriptionName("sub-TestUnAckMessageRedeliveryWithReceive");
            builder.AckTimeout(TimeSpan.FromMilliseconds(5000));
            builder.ForceTopicCreation(true);
            builder.AcknowledgmentGroupTime(TimeSpan.Zero);
            builder.SubscriptionType(SubType.Shared);
            var consumer = await _client.NewConsumerAsync(builder);
            var messageReceived = 0;
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            for (var i = 0; i < messageCount - 2; ++i)
            {
                var m = (Message<byte[]>)await consumer.ReceiveAsync();
                if(m != null)
                {
                    _output.WriteLine($"BrokerEntryMetadata[timestamp:{m.BrokerEntryMetadata.BrokerTimestamp} index: {m.BrokerEntryMetadata?.Index.ToString()}");
                    var receivedMessage = Encoding.UTF8.GetString(m.Data);
                    _output.WriteLine($"Received message: [{receivedMessage}]");
                    messageReceived++;

                }
            }

            Assert.True(messageReceived > 0);
            await Task.Delay(TimeSpan.FromSeconds(10));
            for (var i = 0; i < messageCount - 5; i++)
            {
                var m = (Message<byte[]>)await consumer.ReceiveAsync();
                if(m != null)
                {
                    var receivedMessage = Encoding.UTF8.GetString(m.Data);
                    _output.WriteLine($"Received message: [{receivedMessage}]");
                    messageReceived++;
                }
            }
            await producer.CloseAsync();
            await consumer.CloseAsync();
            Assert.True(messageReceived > 5);
            _client.Dispose();
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

            var producer = await _client.NewProducerAsync(producerBuilder).ConfigureAwait(false);

            var txn = await Txn().ConfigureAwait(false);

            var messageCnt = 10;
            for (var i = 0; i < messageCnt; i++)
            {
                await producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i))
                    .SendAsync().ConfigureAwait(false);
            }
            _output.WriteLine("produce transaction messages finished");
            var consumer = await _client.NewConsumerAsync(consumerBuilder).ConfigureAwait(false);

            // Can't receive transaction messages before commit.
            var message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            Assert.Null(message);
            _output.WriteLine("transaction messages can't be received before transaction committed");

            await txn.CommitAsync().ConfigureAwait(false);

            var ackedMessageCount = 0;
            var receiveCnt = 0;
            await Task.Delay(TimeSpan.FromSeconds(5));
            for (var i = 0; i < messageCnt; i++)
            {
                message = await consumer.ReceiveAsync().ConfigureAwait(false);
                Assert.NotNull(message);
                receiveCnt++;
                if (i % 2 == 0)
                {
                    await consumer.AcknowledgeAsync(message).ConfigureAwait(false);
                    ackedMessageCount++;
                }
            }
            Assert.Equal(messageCnt, receiveCnt);

            message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            Assert.Null(message);

            await consumer.RedeliverUnacknowledgedMessagesAsync().ConfigureAwait(false);

            /*await Task.Delay(TimeSpan.FromSeconds(10));
            receiveCnt = 0;
            for (var i = 0; i < messageCnt; i++)
            {
                message = await consumer.ReceiveAsync().ConfigureAwait(false);
                if(message != null)
                {
                    await consumer.AcknowledgeAsync(message).ConfigureAwait(false);
                    receiveCnt++;
                }
            }
            Assert.True(receiveCnt > 5);

            message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            Assert.Null(message);
            _output.WriteLine($"receive transaction messages count: {receiveCnt}");*/
            _client.Dispose();
        }

        private async Task<TransactionImpl.Transaction> Txn() => (TransactionImpl.Transaction)await _client.NewTransaction().WithTransactionTimeout(TimeSpan.FromMinutes(5)).BuildAsync();


    }
}
