using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Schemas;
using SharpPulsar.Test.Fixtures;
using SharpPulsar.User;
using System;
using System.Text;
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
namespace SharpPulsar.Test.Schema
{
    [Collection(nameof(PulsarTests))]
    public class ProducerInstantiation
    {
        private PulsarSystem _system;
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;

        public ProducerInstantiation(ITestOutputHelper output, PulsarSystemFixture fixture)
        {
            _system = fixture.System;
            _output = output;
            _client = _system.NewClient();
        }
        [Fact]
        public virtual void TestProducerInstantiation()
        {
            var producer = new ProducerConfigBuilder<string>();
            producer.Topic(Guid.NewGuid().ToString());
            var stringProducerBuilder = _client.NewProducer(new StringSchema(), producer);
            Assert.NotNull(stringProducerBuilder);
        }
    }
    
    [Collection(nameof(PulsarTests))]
    public class ConsumerInstantiation
    {
        private PulsarSystem _system;
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;

        public ConsumerInstantiation(ITestOutputHelper output, PulsarSystemFixture fixture)
        {
            _system = fixture.System;
            _output = output;
            _client = _system.NewClient();
        }
        [Fact]
        public virtual void TestConsumerInstantiation()
        {
            var consumer = new ConsumerConfigBuilder<string>();
            consumer.Topic(Guid.NewGuid().ToString());
            consumer.SubscriptionName("test-sub");
            var stringConsumerBuilder = _client.NewConsumer(new StringSchema(), consumer);
            Assert.NotNull(stringConsumerBuilder);
        }
    }
    
    [Collection(nameof(PulsarTests))]
    public class ReaderInstantiation
    {
        private PulsarSystem _system;
        private PulsarClient _client;
        private readonly ITestOutputHelper _output;

        public ReaderInstantiation(ITestOutputHelper output, PulsarSystemFixture fixture)
        {
            _system = fixture.System;
            _output = output;
            _client = _system.NewClient();
        }
        [Fact]
        public virtual void TestReaderInstantiation()
        {
            var reader = new ReaderConfigBuilder<string>();
            reader.Topic(Guid.NewGuid().ToString());
            reader.StartMessageId(IMessageId.Earliest);
            var stringReaderBuilder = _client.NewReader(new StringSchema(), reader);
            Assert.NotNull(stringReaderBuilder);
        }

    }

}