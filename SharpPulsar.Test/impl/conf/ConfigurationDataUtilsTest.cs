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
namespace SharpPulsar.Test.Impl.conf
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
/// <summary>
	/// Unit test <seealso cref="ConfigurationDataUtils"/>.
	/// </summary>
	public class ConfigurationDataUtilsTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLoadClientConfigurationData()
		public virtual void TestLoadClientConfigurationData()
		{
			ClientConfigurationData ConfData = new ClientConfigurationData();
			ConfData.ServiceUrl = "pulsar://unknown:6650";
			ConfData.MaxLookupRequest = 600;
			ConfData.NumIoThreads = 33;
			IDictionary<string, object> Config = new Dictionary<string, object>();
			Config["serviceUrl"] = "pulsar://localhost:6650";
			Config["maxLookupRequest"] = 70000;
			ConfData = ConfigurationDataUtils.LoadData(Config, ConfData, typeof(ClientConfigurationData));
			assertEquals("pulsar://localhost:6650", ConfData.ServiceUrl);
			assertEquals(70000, ConfData.MaxLookupRequest);
			assertEquals(33, ConfData.NumIoThreads);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLoadProducerConfigurationData()
		public virtual void TestLoadProducerConfigurationData()
		{
			ProducerConfigurationData ConfData = new ProducerConfigurationData();
			ConfData.ProducerName = "unset";
			ConfData.BatchingEnabled = true;
			ConfData.BatchingMaxMessages = 1234;
			IDictionary<string, object> Config = new Dictionary<string, object>();
			Config["producerName"] = "test-producer";
			Config["batchingEnabled"] = false;
			ConfData.BatcherBuilder = BatcherBuilderFields.DEFAULT;
			ConfData = ConfigurationDataUtils.LoadData(Config, ConfData, typeof(ProducerConfigurationData));
			assertEquals("test-producer", ConfData.ProducerName);
			assertFalse(ConfData.BatchingEnabled);
			assertEquals(1234, ConfData.BatchingMaxMessages);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLoadConsumerConfigurationData()
		public virtual void TestLoadConsumerConfigurationData()
		{
			ConsumerConfigurationData ConfData = new ConsumerConfigurationData();
			ConfData.SubscriptionName = "unknown-subscription";
			ConfData.PriorityLevel = 10000;
			ConfData.ConsumerName = "unknown-consumer";
			IDictionary<string, object> Config = new Dictionary<string, object>();
			Config["subscriptionName"] = "test-subscription";
			Config["priorityLevel"] = 100;
			ConfData = ConfigurationDataUtils.LoadData(Config, ConfData, typeof(ConsumerConfigurationData));
			assertEquals("test-subscription", ConfData.SubscriptionName);
			assertEquals(100, ConfData.PriorityLevel);
			assertEquals("unknown-consumer", ConfData.ConsumerName);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLoadReaderConfigurationData()
		public virtual void TestLoadReaderConfigurationData()
		{
			ReaderConfigurationData ConfData = new ReaderConfigurationData();
			ConfData.TopicName = "unknown";
			ConfData.ReceiverQueueSize = 1000000;
			ConfData.ReaderName = "unknown-reader";
			IDictionary<string, object> Config = new Dictionary<string, object>();
			Config["topicName"] = "test-topic";
			Config["receiverQueueSize"] = 100;
			ConfData = ConfigurationDataUtils.LoadData(Config, ConfData, typeof(ReaderConfigurationData));
			assertEquals("test-topic", ConfData.TopicName);
			assertEquals(100, ConfData.ReceiverQueueSize);
			assertEquals("unknown-reader", ConfData.ReaderName);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLoadConfigurationDataWithUnknownFields()
		public virtual void TestLoadConfigurationDataWithUnknownFields()
		{
			ReaderConfigurationData ConfData = new ReaderConfigurationData();
			ConfData.TopicName = "unknown";
			ConfData.ReceiverQueueSize = 1000000;
			ConfData.ReaderName = "unknown-reader";
			IDictionary<string, object> Config = new Dictionary<string, object>();
			Config["unknown"] = "test-topic";
			Config["receiverQueueSize"] = 100;
			try
			{
				ConfigurationDataUtils.LoadData(Config, ConfData, typeof(ReaderConfigurationData));
				fail("Should fail loading configuration data with unknown fields");
			}
			catch (System.Exception Re)
			{
				assertTrue(Re.InnerException is IOException);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConfigBuilder() throws org.apache.pulsar.client.api.PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestConfigBuilder()
		{
			ClientConfigurationData ClientConfig = new ClientConfigurationData();
			ClientConfig.ServiceUrl = "pulsar://unknown:6650";
			ClientConfig.StatsIntervalSeconds = 80;

			PulsarClientImpl PulsarClient = new PulsarClientImpl(ClientConfig);
			assertNotNull(PulsarClient, "Pulsar client built using config should not be null");

			assertEquals(PulsarClient.Configuration.ServiceUrl, "pulsar://unknown:6650");
			assertEquals(PulsarClient.Configuration.NumListenerThreads, 1, "builder default not set properly");
			assertEquals(PulsarClient.Configuration.StatsIntervalSeconds, 80, "builder default should overrite if set explicitly");
		}
	}

}