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
	using Org.Apache.Pulsar.Client.Impl.Conf;
	using BeforeTest = org.testng.annotations.BeforeTest;
	using Test = org.testng.annotations.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNotNull;

	/// <summary>
	/// Unit tests of <seealso cref="ConsumerBuilderImpl"/>.
	/// </summary>
	public class ConsumerBuilderImplTest
	{

		private const string TopicName = "testTopicName";
		private ConsumerBuilderImpl consumerBuilderImpl;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeTest public void setup()
		public virtual void Setup()
		{
			PulsarClientImpl Client = mock(typeof(PulsarClientImpl));
			ConsumerConfigurationData ConsumerConfigurationData = mock(typeof(ConsumerConfigurationData));
			when(ConsumerConfigurationData.TopicsPattern).thenReturn(Pattern.compile(@"\w+"));
			when(ConsumerConfigurationData.SubscriptionName).thenReturn("testSubscriptionName");
			consumerBuilderImpl = new ConsumerBuilderImpl(Client, ConsumerConfigurationData, SchemaFields.BYTES);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConsumerBuilderImpl() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestConsumerBuilderImpl()
		{
			Consumer Consumer = mock(typeof(Consumer));
			when(consumerBuilderImpl.subscribeAsync()).thenReturn(CompletableFuture.completedFuture(Consumer));
			assertNotNull(consumerBuilderImpl.topic(TopicName).Subscribe());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesVarargsIsNull()
		public virtual void TestConsumerBuilderImplWhenTopicNamesVarargsIsNull()
		{
			consumerBuilderImpl.topic(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesVarargsHasNullTopic()
		public virtual void TestConsumerBuilderImplWhenTopicNamesVarargsHasNullTopic()
		{
			consumerBuilderImpl.topic("my-topic", null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesVarargsHasBlankTopic()
		public virtual void TestConsumerBuilderImplWhenTopicNamesVarargsHasBlankTopic()
		{
			consumerBuilderImpl.topic("my-topic", "  ");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesIsNull()
		public virtual void TestConsumerBuilderImplWhenTopicNamesIsNull()
		{
			consumerBuilderImpl.topics(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesIsEmpty()
		public virtual void TestConsumerBuilderImplWhenTopicNamesIsEmpty()
		{
			consumerBuilderImpl.topics(Arrays.asList());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesHasBlankTopic()
		public virtual void TestConsumerBuilderImplWhenTopicNamesHasBlankTopic()
		{
			IList<string> TopicNames = Arrays.asList("my-topic", " ");
			consumerBuilderImpl.topics(TopicNames);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesHasNullTopic()
		public virtual void TestConsumerBuilderImplWhenTopicNamesHasNullTopic()
		{
			IList<string> TopicNames = Arrays.asList("my-topic", null);
			consumerBuilderImpl.topics(TopicNames);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenSubscriptionNameIsNull()
		public virtual void TestConsumerBuilderImplWhenSubscriptionNameIsNull()
		{
			consumerBuilderImpl.topic(TopicName).SubscriptionName(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenSubscriptionNameIsBlank()
		public virtual void TestConsumerBuilderImplWhenSubscriptionNameIsBlank()
		{
			consumerBuilderImpl.topic(TopicName).SubscriptionName(" ");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testConsumerBuilderImplWhenConsumerEventListenerIsNull()
		public virtual void TestConsumerBuilderImplWhenConsumerEventListenerIsNull()
		{
			consumerBuilderImpl.topic(TopicName).SubscriptionName("subscriptionName").consumerEventListener(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testConsumerBuilderImplWhenCryptoKeyReaderIsNull()
		public virtual void TestConsumerBuilderImplWhenCryptoKeyReaderIsNull()
		{
			consumerBuilderImpl.topic(TopicName).SubscriptionName("subscriptionName").cryptoKeyReader(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testConsumerBuilderImplWhenCryptoFailureActionIsNull()
		public virtual void TestConsumerBuilderImplWhenCryptoFailureActionIsNull()
		{
			consumerBuilderImpl.topic(TopicName).SubscriptionName("subscriptionName").cryptoFailureAction(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenConsumerNameIsNull()
		public virtual void TestConsumerBuilderImplWhenConsumerNameIsNull()
		{
			consumerBuilderImpl.topic(TopicName).ConsumerName(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenConsumerNameIsBlank()
		public virtual void TestConsumerBuilderImplWhenConsumerNameIsBlank()
		{
			consumerBuilderImpl.topic(TopicName).ConsumerName(" ");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertyKeyIsNull()
		public virtual void TestConsumerBuilderImplWhenPropertyKeyIsNull()
		{
			consumerBuilderImpl.topic(TopicName).Property(null, "Test-Value");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertyKeyIsBlank()
		public virtual void TestConsumerBuilderImplWhenPropertyKeyIsBlank()
		{
			consumerBuilderImpl.topic(TopicName).Property("   ", "Test-Value");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertyValueIsNull()
		public virtual void TestConsumerBuilderImplWhenPropertyValueIsNull()
		{
			consumerBuilderImpl.topic(TopicName).Property("Test-Key", null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertyValueIsBlank()
		public virtual void TestConsumerBuilderImplWhenPropertyValueIsBlank()
		{
			consumerBuilderImpl.topic(TopicName).Property("Test-Key", "   ");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConsumerBuilderImplWhenPropertiesAreCorrect()
		public virtual void TestConsumerBuilderImplWhenPropertiesAreCorrect()
		{
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			Properties["Test-Key"] = "Test-Value";
			Properties["Test-Key2"] = "Test-Value2";

			consumerBuilderImpl.topic(TopicName).Properties(Properties);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertiesKeyIsNull()
		public virtual void TestConsumerBuilderImplWhenPropertiesKeyIsNull()
		{
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			Properties[null] = "Test-Value";

			consumerBuilderImpl.topic(TopicName).Properties(Properties);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertiesKeyIsBlank()
		public virtual void TestConsumerBuilderImplWhenPropertiesKeyIsBlank()
		{
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			Properties["  "] = "Test-Value";

			consumerBuilderImpl.topic(TopicName).Properties(Properties);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertiesValueIsNull()
		public virtual void TestConsumerBuilderImplWhenPropertiesValueIsNull()
		{
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			Properties["Test-Key"] = null;

			consumerBuilderImpl.topic(TopicName).Properties(Properties);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertiesValueIsBlank()
		public virtual void TestConsumerBuilderImplWhenPropertiesValueIsBlank()
		{
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			Properties["Test-Key"] = "   ";

			consumerBuilderImpl.topic(TopicName).Properties(Properties);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertiesIsEmpty()
		public virtual void TestConsumerBuilderImplWhenPropertiesIsEmpty()
		{
			IDictionary<string, string> Properties = new Dictionary<string, string>();

			consumerBuilderImpl.topic(TopicName).Properties(Properties);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testConsumerBuilderImplWhenPropertiesIsNull()
		public virtual void TestConsumerBuilderImplWhenPropertiesIsNull()
		{
			consumerBuilderImpl.topic(TopicName).Properties(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testConsumerBuilderImplWhenSubscriptionInitialPositionIsNull()
		public virtual void TestConsumerBuilderImplWhenSubscriptionInitialPositionIsNull()
		{
			consumerBuilderImpl.topic(TopicName).SubscriptionInitialPosition(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testConsumerBuilderImplWhenSubscriptionTopicsModeIsNull()
		public virtual void TestConsumerBuilderImplWhenSubscriptionTopicsModeIsNull()
		{
			consumerBuilderImpl.topic(TopicName).SubscriptionTopicsMode(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenNegativeAckRedeliveryDelayPropertyIsNegative()
		public virtual void TestConsumerBuilderImplWhenNegativeAckRedeliveryDelayPropertyIsNegative()
		{
			consumerBuilderImpl.negativeAckRedeliveryDelay(-1, TimeUnit.MILLISECONDS);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPriorityLevelPropertyIsNegative()
		public virtual void TestConsumerBuilderImplWhenPriorityLevelPropertyIsNegative()
		{
			consumerBuilderImpl.priorityLevel(-1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenMaxTotalReceiverQueueSizeAcrossPartitionsPropertyIsNegative()
		public virtual void TestConsumerBuilderImplWhenMaxTotalReceiverQueueSizeAcrossPartitionsPropertyIsNegative()
		{
			consumerBuilderImpl.maxTotalReceiverQueueSizeAcrossPartitions(-1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPatternAutoDiscoveryPeriodPropertyIsNegative()
		public virtual void TestConsumerBuilderImplWhenPatternAutoDiscoveryPeriodPropertyIsNegative()
		{
			consumerBuilderImpl.patternAutoDiscoveryPeriod(-1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenBatchReceivePolicyIsNull()
		public virtual void TestConsumerBuilderImplWhenBatchReceivePolicyIsNull()
		{
			consumerBuilderImpl.batchReceivePolicy(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenBatchReceivePolicyIsNotValid()
		public virtual void TestConsumerBuilderImplWhenBatchReceivePolicyIsNotValid()
		{
			consumerBuilderImpl.batchReceivePolicy(BatchReceivePolicy.Builder().maxNumMessages(0).maxNumBytes(0).timeout(0, TimeUnit.MILLISECONDS).build());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConsumerBuilderImplWhenNumericPropertiesAreValid()
		public virtual void TestConsumerBuilderImplWhenNumericPropertiesAreValid()
		{
			consumerBuilderImpl.negativeAckRedeliveryDelay(1, TimeUnit.MILLISECONDS);
			consumerBuilderImpl.priorityLevel(1);
			consumerBuilderImpl.maxTotalReceiverQueueSizeAcrossPartitions(1);
			consumerBuilderImpl.patternAutoDiscoveryPeriod(1);
		}

	}

}