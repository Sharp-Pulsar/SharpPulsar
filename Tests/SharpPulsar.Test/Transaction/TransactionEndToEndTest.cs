using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Common;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;

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
namespace SharpPulsar.Test.Transaction
{
    /// <summary>
    /// End to end transaction test.
    /// </summary>
    //https://dev.to/damikun/the-cross-platform-build-automation-with-nuke-1kmc
    [Collection(nameof(TransactionCollection))]
	public class TransactionEndToEndTest
	{

		private const int TopicPartition = 3;

		private const string TENANT = "public";
		private static readonly string _nAMESPACE1 = TENANT + "/default";
		private static readonly string _topicOutput = _nAMESPACE1 + $"/output-txn-{DateTime.Now.Ticks}";
		private static readonly string _topicMessageAckTest = _nAMESPACE1 + "/message-ack-test";

		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;
        private readonly Admin.Public.Admin _admin;
        public TransactionEndToEndTest(ITestOutputHelper output, PulsarFixture fixture)
		{
			_output = output;
			_client = fixture.TransactionClient;
            _admin = new Admin.Public.Admin("http://localhost:8080/", new HttpClient());

            try
            {
                //var response = _admin.SetRetention("public", "default", retentionPolicies: new SharpPulsar.Admin.Models.RetentionPolicies(retentionTimeInMinutes: 3600, retentionSizeInMB: 1000));
                //var bla = response;
            }
            catch { }
        }
        [Fact]
		public async Task ProduceCommitTest()
		{
            var txn1 = await Txn();
            var txn2 = await Txn(); ;
			var topic = $"{_topicOutput}";
			var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
				.Topic(topic)
                .ForceTopicCreation(true)
				.SubscriptionName($"test-{Guid.NewGuid()}");

			var consumer = await _client.NewConsumerAsync(consumerBuilder);

			var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(topic)
				.SendTimeout(TimeSpan.Zero);

			var producer = await _client.NewProducerAsync(producerBuilder);

			var txnMessageCnt = 0;
			var messageCnt = 10;
			for(var i = 0; i < messageCnt; i++)
			{
                if(i % 5 == 0)
                    await producer.NewMessage(txn1).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).SendAsync();
                else
                    await producer.NewMessage(txn2).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).SendAsync();

                txnMessageCnt++;
			}

			// Can't receive transaction messages before commit.
			var message = await consumer.ReceiveAsync();
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
				message = await consumer.ReceiveAsync();
				Assert.NotNull(message);
                _output.WriteLine(Encoding.UTF8.GetString(message.Value));
				receiveCnt++;
			}

            for (var i = 0; i < txnMessageCnt; i++)
            {
                message = await consumer.ReceiveAsync();
                if(message != null)
                    receiveCnt++;
            }
            Assert.True(receiveCnt > 8);

			message = await consumer.ReceiveAsync();
			Assert.Null(message);

			_output.WriteLine($"message commit test enableBatch {true}");
		}
		[Fact]
		public async Task ProduceCommitBatchedTest()
		{

            var txn = await Txn();

            var topic = $"{_topicOutput}-{Guid.NewGuid()}";

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(topic)
                .ForceTopicCreation(true)
                .SubscriptionName($"test2{Guid.NewGuid()}")
                .EnableBatchIndexAcknowledgment(true)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest);

            var producerBuilder = new ProducerConfigBuilder<byte[]>()
				.Topic(topic)
				.EnableBatching(true)
                .BatchingMaxMessages(10)
				.SendTimeout(TimeSpan.Zero);

			var producer = await _client.NewProducerAsync(producerBuilder);

            var txnMessageCnt = 0;
			var messageCnt = 40;
			for(var i = 0; i < messageCnt; i++)
			{
				await producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).SendAsync();
				txnMessageCnt++;
			}


            var consumer = await _client.NewConsumerAsync(consumerBuilder);
            // Can't receive transaction messages before commit.
            var message = await consumer.ReceiveAsync();
			Assert.Null(message);

			await txn.CommitAsync();

			// txn1 messages could be received after txn1 committed
			var receiveCnt = 0;
            await Task.Delay(TimeSpan.FromSeconds(5));
            for (var i = 0; i < txnMessageCnt; i++)
			{
				message = await consumer.ReceiveAsync();
				Assert.NotNull(message);
				receiveCnt++;
				_output.WriteLine($"message receive count: {receiveCnt}");
			}
			Assert.Equal(txnMessageCnt, receiveCnt);

			message = await consumer.ReceiveAsync();
            Assert.Null(message);

			_output.WriteLine($"message commit test enableBatch {true}");
		}
		[Fact]
		public async Task ProduceAbortTest()
		{
            var topic = $"{_topicOutput}-{Guid.NewGuid()}";
            var txn = await Txn();

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(topic)
                .SubscriptionName($"test{DateTime.Now.Ticks}")
                .ForceTopicCreation(true)
                .EnableBatchIndexAcknowledgment(true)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest);

            var producerBuilder = new ProducerConfigBuilder<byte[]>();
			producerBuilder.Topic(topic);
			producerBuilder.SendTimeout(TimeSpan.Zero);

			var producer = await _client.NewProducerAsync(producerBuilder);

			var messageCnt = 10;
			for(var i = 0; i < messageCnt; i++)
			{
				await producer.NewMessage(txn).Value(Encoding.UTF8.GetBytes("Hello Txn - " + i)).SendAsync();
			}

            var consumer = await _client.NewConsumerAsync(consumerBuilder);
            // Can't receive transaction messages before abort.
            var message = await consumer.ReceiveAsync();
            Assert.Null(message);

			await txn.AbortAsync();

            // Cant't receive transaction messages after abort.
            message = await consumer.ReceiveAsync();
			Assert.Null(message);
        }

        private async Task<User.Transaction> Txn() => (User.Transaction)await _client.NewTransaction().WithTransactionTimeout(TimeSpan.FromMinutes(5)).BuildAsync();


    }

}