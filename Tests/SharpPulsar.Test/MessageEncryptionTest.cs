using SharpPulsar.Configuration;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test
{
    [Collection(nameof(PulsarTests))]
	public class MessageEncryptionTest
	{
		private readonly ITestOutputHelper _output;
		private readonly PulsarClient _client;

		public MessageEncryptionTest(ITestOutputHelper output, PulsarStandaloneClusterFixture fixture)
		{
			_output = output;
			_client = fixture.Client;
		}
		[Fact]
		public void TestEncrptedProduceConsume()
		{
			var messageCount = 10;
			var topic = "encrypted-messages";


            var consumer = _client.NewConsumer(new ConsumerConfigBuilder<byte[]>()
                .Topic(topic)
                .ForceTopicCreation(true)
                .CryptoKeyReader(new RawFileKeyReader("Certs/SharpPulsar_pub.pem", "Certs/SharpPulsar_private.pem"))
                .SubscriptionName("encrypted-sub")
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest));

            var producer = _client.NewProducer(new ProducerConfigBuilder<byte[]>()
				.Topic(topic)
				.CryptoKeyReader(new RawFileKeyReader("Certs/SharpPulsar_pub.pem", "Certs/SharpPulsar_private.pem"))
				.AddEncryptionKey("Ebere"));

            for (var i = 0; i < messageCount; i++)
			{
				producer.Send(Encoding.UTF8.GetBytes($"Shhhh, a secret: my is Ebere Abanonu and am a Nigerian based in Abeokuta, Ogun (a neighbouring State to Lagos - about 2 hours drive) [{i}]"));
			}
			var receivedCount = 0;

			for (var i = 0; i < messageCount; i++)
			{
				var message = consumer.Receive();
				if(message != null)
                {
					var decrypted = Encoding.UTF8.GetString(message.Data);
					_output.WriteLine(decrypted);
					Assert.Equal($"Shhhh, a secret: my is Ebere Abanonu and am a Nigerian based in Abeokuta, Ogun (a neighbouring State to Lagos - about 2 hours drive) [{i}]", decrypted);
					consumer.Acknowledge(message);
					receivedCount++;
				}
			}
			Assert.True(receivedCount > 6);
		}
	}
}
