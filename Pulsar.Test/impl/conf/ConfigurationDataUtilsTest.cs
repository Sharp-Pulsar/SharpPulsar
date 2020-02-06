using System;
using System.Collections.Generic;

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
namespace org.apache.pulsar.client.impl.conf
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.fail;


	using BatcherBuilder = api.BatcherBuilder;
	using PulsarClientException = api.PulsarClientException;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test <seealso cref="ConfigurationDataUtils"/>.
	/// </summary>
	public class ConfigurationDataUtilsTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLoadClientConfigurationData()
		public virtual void testLoadClientConfigurationData()
		{
			ClientConfigurationData confData = new ClientConfigurationData();
			confData.ServiceUrl = "pulsar://unknown:6650";
			confData.MaxLookupRequest = 600;
			confData.NumIoThreads = 33;
			IDictionary<string, object> config = new Dictionary<string, object>();
			config["serviceUrl"] = "pulsar://localhost:6650";
			config["maxLookupRequest"] = 70000;
			confData = ConfigurationDataUtils.loadData(config, confData, typeof(ClientConfigurationData));
			assertEquals("pulsar://localhost:6650", confData.ServiceUrl);
			assertEquals(70000, confData.MaxLookupRequest);
			assertEquals(33, confData.NumIoThreads);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLoadProducerConfigurationData()
		public virtual void testLoadProducerConfigurationData()
		{
			ProducerConfigurationData confData = new ProducerConfigurationData();
			confData.ProducerName = "unset";
			confData.BatchingEnabled = true;
			confData.BatchingMaxMessages = 1234;
			IDictionary<string, object> config = new Dictionary<string, object>();
			config["producerName"] = "test-producer";
			config["batchingEnabled"] = false;
			confData.BatcherBuilder = BatcherBuilder.DEFAULT;
			confData = ConfigurationDataUtils.loadData(config, confData, typeof(ProducerConfigurationData));
			assertEquals("test-producer", confData.ProducerName);
			assertFalse(confData.BatchingEnabled);
			assertEquals(1234, confData.BatchingMaxMessages);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLoadConsumerConfigurationData()
		public virtual void testLoadConsumerConfigurationData()
		{
			ConsumerConfigurationData confData = new ConsumerConfigurationData();
			confData.SubscriptionName = "unknown-subscription";
			confData.PriorityLevel = 10000;
			confData.ConsumerName = "unknown-consumer";
			IDictionary<string, object> config = new Dictionary<string, object>();
			config["subscriptionName"] = "test-subscription";
			config["priorityLevel"] = 100;
			confData = ConfigurationDataUtils.loadData(config, confData, typeof(ConsumerConfigurationData));
			assertEquals("test-subscription", confData.SubscriptionName);
			assertEquals(100, confData.PriorityLevel);
			assertEquals("unknown-consumer", confData.ConsumerName);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLoadReaderConfigurationData()
		public virtual void testLoadReaderConfigurationData()
		{
			ReaderConfigurationData confData = new ReaderConfigurationData();
			confData.TopicName = "unknown";
			confData.ReceiverQueueSize = 1000000;
			confData.ReaderName = "unknown-reader";
			IDictionary<string, object> config = new Dictionary<string, object>();
			config["topicName"] = "test-topic";
			config["receiverQueueSize"] = 100;
			confData = ConfigurationDataUtils.loadData(config, confData, typeof(ReaderConfigurationData));
			assertEquals("test-topic", confData.TopicName);
			assertEquals(100, confData.ReceiverQueueSize);
			assertEquals("unknown-reader", confData.ReaderName);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLoadConfigurationDataWithUnknownFields()
		public virtual void testLoadConfigurationDataWithUnknownFields()
		{
			ReaderConfigurationData confData = new ReaderConfigurationData();
			confData.TopicName = "unknown";
			confData.ReceiverQueueSize = 1000000;
			confData.ReaderName = "unknown-reader";
			IDictionary<string, object> config = new Dictionary<string, object>();
			config["unknown"] = "test-topic";
			config["receiverQueueSize"] = 100;
			try
			{
				ConfigurationDataUtils.loadData(config, confData, typeof(ReaderConfigurationData));
				fail("Should fail loading configuration data with unknown fields");
			}
			catch (Exception re)
			{
				assertTrue(re.InnerException is IOException);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConfigBuilder() throws org.apache.pulsar.client.api.PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testConfigBuilder()
		{
			ClientConfigurationData clientConfig = new ClientConfigurationData();
			clientConfig.ServiceUrl = "pulsar://unknown:6650";
			clientConfig.StatsIntervalSeconds = 80;

			PulsarClientImpl pulsarClient = new PulsarClientImpl(clientConfig);
			assertNotNull(pulsarClient, "Pulsar client built using config should not be null");

			assertEquals(pulsarClient.Configuration.ServiceUrl, "pulsar://unknown:6650");
			assertEquals(pulsarClient.Configuration.NumListenerThreads, 1, "builder default not set properly");
			assertEquals(pulsarClient.Configuration.StatsIntervalSeconds, 80, "builder default should overrite if set explicitly");
		}
	}

}