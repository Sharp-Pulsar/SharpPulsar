using SharpPulsar.Configuration;
using SharpPulsar.Test.Integration.Fixtures;
using SharpPulsar.User;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Integration
{
    [Collection(nameof(PulsarTests))]
	public class MessageEncryptionTest
	{
		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;

		public MessageEncryptionTest(ITestOutputHelper output, PulsarIntegrationFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}
		[Fact]
		public async Task TestEncrptedProduceConsume()
		{
			var messageCount = 10;
			var topic = $"encrypted-messages-{Guid.NewGuid()}";

            var consumer = await _client.NewConsumerAsync(new ConsumerConfigBuilder<byte[]>()
                .Topic(topic)
                .ForceTopicCreation(true)
                .CryptoKeyReader(new RawFileKeyReader("Certs/SharpPulsar_pub.pem", "Certs/SharpPulsar_private.pem"))
                .SubscriptionName("encrypted-sub")
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest));

            var producer = await _client.NewProducerAsync(new ProducerConfigBuilder<byte[]>()
				.Topic(topic)
				.CryptoKeyReader(new RawFileKeyReader("Certs/SharpPulsar_pub.pem", "Certs/SharpPulsar_private.pem"))
				.AddEncryptionKey("Ebere"));

            for (var i = 0; i < messageCount; i++)
			{
				await producer.SendAsync(Encoding.UTF8.GetBytes($"Shhhh, a secret: my is Ebere Abanonu and am a Nigerian based in Abeokuta, Ogun (a neighbouring State to Lagos - about 2 hours drive) [{i}]"));
			}
			var receivedCount = 0;

            await Task.Delay(TimeSpan.FromSeconds(5));
			for (var i = 0; i < messageCount; i++)
			{
				var message = await consumer.ReceiveAsync();
				if(message != null)
                {
					var decrypted = Encoding.UTF8.GetString(message.Data);
					_output.WriteLine(decrypted);
					Assert.Equal($"Shhhh, a secret: my is Ebere Abanonu and am a Nigerian based in Abeokuta, Ogun (a neighbouring State to Lagos - about 2 hours drive) [{i}]", decrypted);
					await consumer.AcknowledgeAsync(message);
					receivedCount++;
				}
			}
			Assert.True(receivedCount > 6);
            await producer.CloseAsync();
            await consumer.CloseAsync();
        }
	}
}
