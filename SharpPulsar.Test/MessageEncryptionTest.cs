using BAMCIS.Util.Concurrent;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Text;
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
			var topic = "encrypted-message-test-long-string";
			var producer = _client.NewProducer(new ProducerConfigBuilder<sbyte[]>()
				.Topic(topic)
				.CryptoKeyReader(new RawFileKeyReader("Certs/SharpPulsar_pub.pem", "Certs/SharpPulsar_private.pem"))
				.AddEncryptionKey("Ebere"));

			for (var i = 0; i < messageCount; i++)
			{
				producer.Send(Encoding.UTF8.GetBytes($"Shhhh, a secret: my is Ebere Abanonu and am a Nigerian based in Abeokuta, Ogun (a neighbouring State to Lagos - about 2 hours drive) [{i}]").ToSBytes());
			}
			var receivedCount = 0;

			var consumer = _client.NewConsumer(new ConsumerConfigBuilder<sbyte[]>()
				.TopicsPattern(topic)
				.CryptoKeyReader(new RawFileKeyReader("Certs/SharpPulsar_pub.pem", "Certs/SharpPulsar_private.pem"))
				.SubscriptionName("encrypted-sub"));

			for (var i = 0; i < messageCount; i++)
			{
				var message = consumer.Receive(TimeSpan.FromSeconds(10));
				if(message != null)
                {
					var decrypted = Encoding.UTF8.GetString(message.Data.ToBytes());
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
