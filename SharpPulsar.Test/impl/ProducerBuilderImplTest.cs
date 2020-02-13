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
namespace Org.Apache.Pulsar.Client.Impl
{
	using Org.Apache.Pulsar.Client.Api;
	using ProducerConfigurationData = Org.Apache.Pulsar.Client.Impl.Conf.ProducerConfigurationData;
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

		private const string TopicName = "testTopicName";
		private PulsarClientImpl client;
		private ProducerBuilderImpl producerBuilderImpl;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeTest public void setup()
		public virtual void Setup()
		{
			Producer Producer = mock(typeof(Producer));
			client = mock(typeof(PulsarClientImpl));
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			when(client.NewProducer()).thenReturn(producerBuilderImpl);

			when(client.CreateProducerAsync(any(typeof(ProducerConfigurationData)), any(typeof(Schema)), eq(null))).thenReturn(CompletableFuture.completedFuture(Producer));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerBuilderImpl() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImpl()
		{
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			Properties["Test-Key2"] = "Test-Value2";

			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			Producer Producer = producerBuilderImpl.topic(TopicName).ProducerName("Test-Producer").maxPendingMessages(2).addEncryptionKey("Test-EncryptionKey").property("Test-Key", "Test-Value").properties(Properties).create();

			assertNotNull(Producer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerBuilderImplWhenMessageRoutingModeAndMessageRouterAreNotSet() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenMessageRoutingModeAndMessageRouterAreNotSet()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			Producer Producer = producerBuilderImpl.topic(TopicName).Create();
			assertNotNull(Producer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerBuilderImplWhenMessageRoutingModeIsSinglePartition() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenMessageRoutingModeIsSinglePartition()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			Producer Producer = producerBuilderImpl.topic(TopicName).MessageRoutingMode(MessageRoutingMode.SinglePartition).create();
			assertNotNull(Producer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerBuilderImplWhenMessageRoutingModeIsRoundRobinPartition() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenMessageRoutingModeIsRoundRobinPartition()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			Producer Producer = producerBuilderImpl.topic(TopicName).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).create();
			assertNotNull(Producer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerBuilderImplWhenMessageRoutingIsSetImplicitly() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenMessageRoutingIsSetImplicitly()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			Producer Producer = producerBuilderImpl.topic(TopicName).MessageRouter(new CustomMessageRouter(this)).create();
			assertNotNull(Producer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerBuilderImplWhenMessageRoutingIsCustomPartition() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenMessageRoutingIsCustomPartition()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			Producer Producer = producerBuilderImpl.topic(TopicName).MessageRoutingMode(MessageRoutingMode.CustomPartition).messageRouter(new CustomMessageRouter(this)).create();
			assertNotNull(Producer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = PulsarClientException.class) public void testProducerBuilderImplWhenMessageRoutingModeIsSinglePartitionAndMessageRouterIsSet() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenMessageRoutingModeIsSinglePartitionAndMessageRouterIsSet()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).MessageRoutingMode(MessageRoutingMode.SinglePartition).messageRouter(new CustomMessageRouter(this)).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = PulsarClientException.class) public void testProducerBuilderImplWhenMessageRoutingModeIsRoundRobinPartitionAndMessageRouterIsSet() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenMessageRoutingModeIsRoundRobinPartitionAndMessageRouterIsSet()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).MessageRoutingMode(MessageRoutingMode.RoundRobinPartition).messageRouter(new CustomMessageRouter(this)).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = PulsarClientException.class) public void testProducerBuilderImplWhenMessageRoutingModeIsCustomPartitionAndMessageRouterIsNotSet() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenMessageRoutingModeIsCustomPartitionAndMessageRouterIsNotSet()
		{
			ProducerBuilderImpl ProducerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			ProducerBuilderImpl.topic(TopicName).messageRoutingMode(MessageRoutingMode.CustomPartition).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenTopicNameIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenTopicNameIsNull()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(null).Create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenTopicNameIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenTopicNameIsBlank()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic("   ").Create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenProducerNameIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenProducerNameIsNull()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).ProducerName(null).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenProducerNameIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenProducerNameIsBlank()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).ProducerName("   ").create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenSendTimeoutIsNegative() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenSendTimeoutIsNegative()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).ProducerName("Test-Producer").sendTimeout(-1, TimeUnit.MILLISECONDS).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenMaxPendingMessagesIsNegative() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenMaxPendingMessagesIsNegative()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).ProducerName("Test-Producer").maxPendingMessages(-1).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenEncryptionKeyIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenEncryptionKeyIsNull()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).AddEncryptionKey(null).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenEncryptionKeyIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenEncryptionKeyIsBlank()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).AddEncryptionKey("   ").create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertyKeyIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenPropertyKeyIsNull()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).Property(null, "Test-Value").create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertyKeyIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenPropertyKeyIsBlank()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).Property("   ", "Test-Value").create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertyValueIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenPropertyValueIsNull()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).Property("Test-Key", null).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertyValueIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenPropertyValueIsBlank()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).Property("Test-Key", "   ").create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testProducerBuilderImplWhenPropertiesIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenPropertiesIsNull()
		{
			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).Properties(null).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertiesKeyIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenPropertiesKeyIsNull()
		{
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			Properties[null] = "Test-Value";

			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).Properties(Properties).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertiesKeyIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenPropertiesKeyIsBlank()
		{
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			Properties["   "] = "Test-Value";

			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).Properties(Properties).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertiesValueIsNull() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenPropertiesValueIsNull()
		{
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			Properties["Test-Key"] = null;

			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).Properties(Properties).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertiesValueIsBlank() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenPropertiesValueIsBlank()
		{
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			Properties["Test-Key"] = "   ";

			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).Properties(Properties).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenPropertiesIsEmpty() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestProducerBuilderImplWhenPropertiesIsEmpty()
		{
			IDictionary<string, string> Properties = new Dictionary<string, string>();

			producerBuilderImpl = new ProducerBuilderImpl(client, SchemaFields.BYTES);
			producerBuilderImpl.topic(TopicName).Properties(Properties).create();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenBatchingMaxPublishDelayPropertyIsNegative()
		public virtual void TestProducerBuilderImplWhenBatchingMaxPublishDelayPropertyIsNegative()
		{
			producerBuilderImpl.batchingMaxPublishDelay(-1, TimeUnit.MILLISECONDS);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenSendTimeoutPropertyIsNegative()
		public virtual void TestProducerBuilderImplWhenSendTimeoutPropertyIsNegative()
		{
			producerBuilderImpl.sendTimeout(-1, TimeUnit.SECONDS);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testProducerBuilderImplWhenMaxPendingMessagesAcrossPartitionsPropertyIsInvalid()
		public virtual void TestProducerBuilderImplWhenMaxPendingMessagesAcrossPartitionsPropertyIsInvalid()
		{
			producerBuilderImpl.maxPendingMessagesAcrossPartitions(999);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProducerBuilderImplWhenNumericPropertiesAreValid()
		public virtual void TestProducerBuilderImplWhenNumericPropertiesAreValid()
		{
			producerBuilderImpl.batchingMaxPublishDelay(1, TimeUnit.SECONDS);
			producerBuilderImpl.batchingMaxMessages(2);
			producerBuilderImpl.sendTimeout(1, TimeUnit.SECONDS);
			producerBuilderImpl.maxPendingMessagesAcrossPartitions(1000);
		}

		[Serializable]
		public class CustomMessageRouter : MessageRouter
		{
			private readonly ProducerBuilderImplTest outerInstance;

			public CustomMessageRouter(ProducerBuilderImplTest outerInstance)
			{
				this.outerInstance = OuterInstance;
			}

			public override int ChoosePartition<T1>(Message<T1> Msg, TopicMetadata Metadata)
			{
				int PartitionIndex = int.Parse(Msg.Key) % Metadata.numPartitions();
				return PartitionIndex;
			}
		}
	}

}