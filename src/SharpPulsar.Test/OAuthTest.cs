﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SharpPulsar.Auth.OAuth2;
using SharpPulsar.Builder;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Fixture;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test
{

    [Collection(nameof(PulsarCollection))]
    public class OAuthTest : IAsyncLifetime
    {
        private readonly ITestOutputHelper _output;

        private readonly string _topic;
        private PulsarClient _client;
        private PulsarSystem _system;
        private PulsarClientConfigBuilder _configBuilder;

        public OAuthTest(ITestOutputHelper output)
        {
            var fileUri = new Uri(GetConfigFilePath());
            var issuerUrl = new Uri("https://churchos.auth0.com/");
            var audience = "https://churchos.auth0.com/api/v2/";
            _output = output;
            var client = new PulsarClientConfigBuilder();
            var serviceUrl = "pulsar://localhost:6650";
            var webUrl = "http://localhost:8080";
            
            client.ServiceUrl(serviceUrl);
            client.WebUrl(webUrl);
            client.Authentication(AuthenticationFactoryOAuth2.ClientCredentials(issuerUrl, fileUri, audience));
            _configBuilder = client;
            _system = PulsarSystem.GetInstance(actorSystemName:"oauth");
            _topic = $"persistent://public/default/oauth-{Guid.NewGuid()}";
        }
        [Fact]
        public virtual async Task OAuth_ProducerInstantiation()
        {
            var producer = new ProducerConfigBuilder<string>();
            producer.Topic(_topic);
            var stringProducerBuilder = await _client.NewProducerAsync(new StringSchema(), producer);
            Assert.NotNull(stringProducerBuilder);
            await stringProducerBuilder.CloseAsync();
        }
        [Fact]
        public virtual async Task OAuth_ConsumerInstantiation()
        {
            var consumer = new ConsumerConfigBuilder<string>();
            consumer.Topic(_topic);
            consumer.SubscriptionName($"oauth-test-sub-{Guid.NewGuid()}");
            var stringConsumerBuilder = await _client.NewConsumerAsync(new StringSchema(), consumer);
            Assert.NotNull(stringConsumerBuilder);
            await stringConsumerBuilder.CloseAsync();
        }
        [Fact]
        public virtual async void OAuth_ReaderInstantiation()
        {
            var reader = new ReaderConfigBuilder<string>();
            reader.Topic(_topic);
            reader.StartMessageId(IMessageId.Earliest);
            var stringReaderBuilder = await _client.NewReaderAsync(new StringSchema(), reader);
            Assert.NotNull(stringReaderBuilder);
            await stringReaderBuilder.CloseAsync();
        }

        [Fact]
        public async Task OAuth_ProduceAndConsume()
        {
            var topic = $"persistent://public/default/oauth-{Guid.NewGuid}";

            var r = new Random(0);
            var byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var producerBuilder = new ProducerConfigBuilder<byte[]>();
            producerBuilder.Topic(topic);
            var producer = await _client.NewProducerAsync(producerBuilder);

            await producer.NewMessage().KeyBytes(byteKey)
               .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
               .Value(Encoding.UTF8.GetBytes("TestMessage"))
               .SendAsync();

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(topic)
                //.StartMessageId(77L, 0L, -1, 0)
                .SubscriptionInitialPosition(Common.SubscriptionInitialPosition.Earliest)
                .SubscriptionName($"ByteKeysTest-subscriber-{Guid.NewGuid()}");
            var consumer = await _client.NewConsumerAsync(consumerBuilder);

            await Task.Delay(TimeSpan.FromSeconds(1));
            var message = (Message<byte[]>)await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));

            if (message != null)
                _output.WriteLine($"BrokerEntryMetadata[timestamp:{message.BrokerEntryMetadata?.BrokerTimestamp} index: {message.BrokerEntryMetadata?.Index.ToString()}");

            Assert.Equal(byteKey, message.KeyBytes);

            Assert.True(message.HasBase64EncodedKey());
            var receivedMessage = Encoding.UTF8.GetString(message.Data);
            _output.WriteLine($"Received message: [{receivedMessage}]");
            Assert.Equal("TestMessage", receivedMessage);
            //producer.Close();
            await consumer.CloseAsync();
        }
        [Fact]
        public async Task OAuth_ProduceAndConsumeBatch()
        {

            var r = new Random(0);
            var byteKey = new byte[1000];
            r.NextBytes(byteKey);

            var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
                .Topic(_topic)
                .ForceTopicCreation(true)
                .SubscriptionName($"Batch-subscriber-{Guid.NewGuid()}");
            var consumer = await _client.NewConsumerAsync(consumerBuilder);


            var producerBuilder = new ProducerConfigBuilder<byte[]>()
                .Topic(_topic)
                .SendTimeout(TimeSpan.FromMilliseconds(10000))
                .EnableBatching(true)
                .BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(120000))
                .BatchingMaxMessages(5);

            var producer = await _client.NewProducerAsync(producerBuilder);

            for (var i = 0; i < 5; i++)
            {
                var id = await producer.NewMessage().KeyBytes(byteKey)
                   .Properties(new Dictionary<string, string> { { "KeyBytes", Encoding.UTF8.GetString(byteKey) } })
                   .Value(Encoding.UTF8.GetBytes($"TestMessage-{i}"))
                   .SendAsync();
                if (id == null)
                    _output.WriteLine($"Id is null");
                else
                    _output.WriteLine($"Id: {id}");
            }
            producer.Flush();
            await Task.Delay(TimeSpan.FromSeconds(1));
            for (var i = 0; i < 5; i++)
            {
                var message = (Message<byte[]>)await consumer.ReceiveAsync(TimeSpan.FromMicroseconds(5000));
                if (message != null)
                    _output.WriteLine($"BrokerEntryMetadata[timestamp:{message.BrokerEntryMetadata.BrokerTimestamp} index: {message.BrokerEntryMetadata?.Index.ToString()}");

                Assert.Equal(byteKey, message?.KeyBytes);
                Assert.True(message?.HasBase64EncodedKey());
                var receivedMessage = Encoding.UTF8.GetString(message.Data);
                _output.WriteLine($"Received message: [{receivedMessage}]");
                Assert.Equal($"TestMessage-{i}", receivedMessage);
            }

            await producer.CloseAsync();
            await consumer.CloseAsync();
        }

        private string GetConfigFilePath()
        {
            var configFolderName = "Oauth2Files";
            var privateKeyFileName = "o-r7y4o-eabanonu.json";
            var startup = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var indexOfConfigDir = startup.IndexOf(startup, StringComparison.Ordinal);
            var examplesFolder = startup.Substring(0, startup.Length - indexOfConfigDir);
            var configFolder = Path.Combine(examplesFolder, configFolderName);
            var ret = Path.Combine(configFolder, privateKeyFileName);
            if (!File.Exists(ret)) throw new FileNotFoundException("can't find credentials file");
            return ret;
        }
        public async Task InitializeAsync()
        {

            _client = await _system.NewClient(_configBuilder);
        }

        public async Task DisposeAsync()
        {
            await _client.ShutdownAsync();
        }
    }
}
