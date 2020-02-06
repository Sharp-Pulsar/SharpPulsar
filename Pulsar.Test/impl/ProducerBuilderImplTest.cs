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
namespace org.apache.pulsar.client.impl
{
	using org.apache.pulsar.client.api;
	using ProducerConfigurationData = conf.ProducerConfigurationData;
	using BeforeTest = org.testng.annotations.BeforeTest;
	using Test = org.testng.annotations.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNotNull;

	/// <summary>
	/// Unit tests of <seealso cref="ProducerBuilderImpl"/>.
	/// </summary>
	public class ProducerBuilderImplTest
	{

		private const string TOPIC_NAME = "testTopicName";
		private PulsarClientImpl client;
		private ProducerBuilderImpl producerBuilderImpl;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeTest public void setup()
		public virtual void setup()
		{
			Producer producer = mock(typeof(Producer));
			client = mock(typeof(PulsarClientImpl));
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			when(client.newProducer()).thenReturn(producerBuilderImpl);

			when(client.createProducerAsync(any(typeof(ProducerConfigurationData)), any(typeof(Schema)), eq(null))).thenReturn(CompletableFuture.completedFuture(producer));
		}

		public virtual void testProducerBuilderImpl()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key2"] = "Test-Value2";

			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			Producer producer = producerBuilderImpl.topic(TOPIC_NAME).producerName("Test-Producer").maxPendingMessages(2).addEncryptionKey("Test-EncryptionKey").property("Test-Key", "Test-Value").properties(properties).create();

			assertNotNull(producer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerBuilderImplWhenMessageRoutingModeAndMessageRouterAreNotSet() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenMessageRoutingModeAndMessageRouterAreNotSet()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			Producer producer = producerBuilderImpl.topic(TOPIC_NAME).create();
			assertNotNull(producer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerBuilderImplWhenMessageRoutingModeIsSinglePartition() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenMessageRoutingModeIsSinglePartition()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			Producer producer = producerBuilderImpl.topic(TOPIC_NAME).messageRoutingMode(MessageRoutingMode.SinglePartition).create();
			assertNotNull(producer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerBuilderImplWhenMessageRoutingModeIsRoundRobinPartition() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenMessageRoutingModeIsRoundRobinPartition()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			Producer producer = producerBuilderImpl.topic(TOPIC_NAME).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();
			assertNotNull(producer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerBuilderImplWhenMessageRoutingIsSetImplicitly() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenMessageRoutingIsSetImplicitly()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			Producer producer = producerBuilderImpl.topic(TOPIC_NAME).messageRouter(new CustomMessageRouter(this)).create();
			assertNotNull(producer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerBuilderImplWhenMessageRoutingIsCustomPartition() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenMessageRoutingIsCustomPartition()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			Producer producer = producerBuilderImpl.topic(TOPIC_NAME).messageRoutingMode(MessageRoutingMode.CustomPartition).messageRouter(new CustomMessageRouter(this)).create();
			assertNotNull(producer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = PulsarClientException.class) public void testProducerBuilderImplWhenMessageRoutingModeIsSinglePartitionAndMessageRouterIsSet() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenMessageRoutingModeIsSinglePartitionAndMessageRouterIsSet()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).messageRoutingMode(MessageRoutingMode.SinglePartition).messageRouter(new CustomMessageRouter(this)).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = PulsarClientException.class) public void testProducerBuilderImplWhenMessageRoutingModeIsRoundRobinPartitionAndMessageRouterIsSet() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenMessageRoutingModeIsRoundRobinPartitionAndMessageRouterIsSet()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).messageRoutingMode(MessageRoutingMode.RoundRobinPartition).messageRouter(new CustomMessageRouter(this)).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = PulsarClientException.class) public void testProducerBuilderImplWhenMessageRoutingModeIsCustomPartitionAndMessageRouterIsNotSet() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenMessageRoutingModeIsCustomPartitionAndMessageRouterIsNotSet()
		{
			ProducerBuilderImpl producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).messageRoutingMode(MessageRoutingMode.CustomPartition).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenTopicNameIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenTopicNameIsNull()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(null).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenTopicNameIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenTopicNameIsBlank()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic("   ").create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenProducerNameIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenProducerNameIsNull()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).producerName(null).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenProducerNameIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenProducerNameIsBlank()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).producerName("   ").create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenSendTimeoutIsNegative() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenSendTimeoutIsNegative()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).producerName("Test-Producer").sendTimeout(-1, TimeUnit.MILLISECONDS).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenMaxPendingMessagesIsNegative() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenMaxPendingMessagesIsNegative()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).producerName("Test-Producer").maxPendingMessages(-1).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenEncryptionKeyIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenEncryptionKeyIsNull()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).addEncryptionKey(null).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenEncryptionKeyIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenEncryptionKeyIsBlank()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).addEncryptionKey("   ").create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertyKeyIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenPropertyKeyIsNull()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).property(null, "Test-Value").create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertyKeyIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenPropertyKeyIsBlank()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).property("   ", "Test-Value").create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertyValueIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenPropertyValueIsNull()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).property("Test-Key", null).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertyValueIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenPropertyValueIsBlank()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).property("Test-Key", "   ").create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testProducerBuilderImplWhenPropertiesIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenPropertiesIsNull()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).properties(null).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertiesKeyIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenPropertiesKeyIsNull()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties[null] = "Test-Value";

			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).properties(properties).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertiesKeyIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenPropertiesKeyIsBlank()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["   "] = "Test-Value";

			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).properties(properties).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertiesValueIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenPropertiesValueIsNull()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key"] = null;

			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).properties(properties).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertiesValueIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenPropertiesValueIsBlank()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key"] = "   ";

			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).properties(properties).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertiesIsEmpty() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testProducerBuilderImplWhenPropertiesIsEmpty()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();

			producerBuilderImpl = new ProducerBuilderImpl(client, Schema.BYTES);
			producerBuilderImpl.topic(TOPIC_NAME).properties(properties).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenBatchingMaxPublishDelayPropertyIsNegative()
		public virtual void testProducerBuilderImplWhenBatchingMaxPublishDelayPropertyIsNegative()
		{
			producerBuilderImpl.batchingMaxPublishDelay(-1, TimeUnit.MILLISECONDS);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenSendTimeoutPropertyIsNegative()
		public virtual void testProducerBuilderImplWhenSendTimeoutPropertyIsNegative()
		{
			producerBuilderImpl.sendTimeout(-1, TimeUnit.SECONDS);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenMaxPendingMessagesAcrossPartitionsPropertyIsInvalid()
		public virtual void testProducerBuilderImplWhenMaxPendingMessagesAcrossPartitionsPropertyIsInvalid()
		{
			producerBuilderImpl.maxPendingMessagesAcrossPartitions(999);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerBuilderImplWhenNumericPropertiesAreValid()
		public virtual void testProducerBuilderImplWhenNumericPropertiesAreValid()
		{
			producerBuilderImpl.batchingMaxPublishDelay(1, TimeUnit.SECONDS);
			producerBuilderImpl.batchingMaxMessages(2);
			producerBuilderImpl.sendTimeout(1, TimeUnit.SECONDS);
			producerBuilderImpl.maxPendingMessagesAcrossPartitions(1000);
		}

		private class CustomMessageRouter : MessageRouter
		{
			private readonly ProducerBuilderImplTest outerInstance;

			public CustomMessageRouter(ProducerBuilderImplTest outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override int choosePartition<T1>(Message<T1> msg, TopicMetadata metadata)
			{
				int partitionIndex = int.Parse(msg.Key) % metadata.numPartitions();
				return partitionIndex;
			}
		}
	}

}