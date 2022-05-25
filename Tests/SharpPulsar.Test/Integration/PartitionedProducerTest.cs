﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SharpPulsar.Builder;
using SharpPulsar.Common;
using SharpPulsar.Configuration;
using SharpPulsar.Test.Fixture;
using SharpPulsar.TestContainer;
using SharpPulsar.User;
using Xunit;
using Xunit.Abstractions;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;

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
namespace SharpPulsar.Test.Integration
{
    [Collection(nameof(IntegrationCollection))]
    public class PartitionedProducerTest
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarClient _client;
        private readonly Admin.Public.Admin _admin;
        public PartitionedProducerTest(ITestOutputHelper output, PulsarFixture fixture)
        {
            _admin = new Admin.Public.Admin("http://localhost:8080/", new HttpClient()); ;
            _output = output;
            _client = fixture.Client;
        }
        [Fact]
        public virtual async Task TestGetNumOfPartitions()
        {
            var topicName = "one-partitioned-topic-" + Guid.NewGuid();

            // create partitioned topic
            try 
            {
                var asf = await _admin.CreatePartitionedTopicAsync("public", "default", topicName, 5);                
            }
            catch(Exception ex)
            {
                var ss = ex.Message ;
                Assert.Equal("Operation returned an invalid status code 'NoContent'", ss);
            }
            var partitions = await _admin.GetPartitionedMetadataAsync("public", "default", topicName);
            var s = partitions.Body.Partitions;
            Assert.Equal(5, s);

            // 2. create producer
            var messagePredicate = "partitioned-producer" + Guid.NewGuid() + "-";
            var partitioProducer = await _client.NewPartitionedProducerAsync(new ProducerConfigBuilder<byte[]>().Topic(topicName)
                .EnableLazyStartPartitionedProducers(true));

            var producers = await partitioProducer.Producers();
            
            // 5. produce data
            foreach (var producer in producers)
            {
              await producer.SendAsync(Encoding.UTF8.GetBytes(messagePredicate + "producer1-"));
            }
            foreach (var producer in producers)
            {
                await producer.CloseAsync();
            }
            
            Assert.True(producers.Count > 0);
        }
    }
}
