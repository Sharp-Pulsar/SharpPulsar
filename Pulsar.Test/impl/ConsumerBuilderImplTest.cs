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
	using org.apache.pulsar.client.impl.conf;
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

		private const string TOPIC_NAME = "testTopicName";
		private ConsumerBuilderImpl consumerBuilderImpl;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeTest public void setup()
		public virtual void setup()
		{
			PulsarClientImpl client = mock(typeof(PulsarClientImpl));
			ConsumerConfigurationData consumerConfigurationData = mock(typeof(ConsumerConfigurationData));
			when(consumerConfigurationData.TopicsPattern).thenReturn(Pattern.compile("\\w+"));
			when(consumerConfigurationData.SubscriptionName).thenReturn("testSubscriptionName");
			consumerBuilderImpl = new ConsumerBuilderImpl(client, consumerConfigurationData, Schema.BYTES);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConsumerBuilderImpl() throws PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testConsumerBuilderImpl()
		{
			Consumer consumer = mock(typeof(Consumer));
			when(consumerBuilderImpl.subscribeAsync()).thenReturn(CompletableFuture.completedFuture(consumer));
			assertNotNull(consumerBuilderImpl.topic(TOPIC_NAME).subscribe());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesVarargsIsNull()
		public virtual void testConsumerBuilderImplWhenTopicNamesVarargsIsNull()
		{
			consumerBuilderImpl.topic(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesVarargsHasNullTopic()
		public virtual void testConsumerBuilderImplWhenTopicNamesVarargsHasNullTopic()
		{
			consumerBuilderImpl.topic("my-topic", null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesVarargsHasBlankTopic()
		public virtual void testConsumerBuilderImplWhenTopicNamesVarargsHasBlankTopic()
		{
			consumerBuilderImpl.topic("my-topic", "  ");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesIsNull()
		public virtual void testConsumerBuilderImplWhenTopicNamesIsNull()
		{
			consumerBuilderImpl.topics(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesIsEmpty()
		public virtual void testConsumerBuilderImplWhenTopicNamesIsEmpty()
		{
			consumerBuilderImpl.topics(Arrays.asList());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesHasBlankTopic()
		public virtual void testConsumerBuilderImplWhenTopicNamesHasBlankTopic()
		{
			IList<string> topicNames = Arrays.asList("my-topic", " ");
			consumerBuilderImpl.topics(topicNames);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenTopicNamesHasNullTopic()
		public virtual void testConsumerBuilderImplWhenTopicNamesHasNullTopic()
		{
			IList<string> topicNames = Arrays.asList("my-topic", null);
			consumerBuilderImpl.topics(topicNames);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenSubscriptionNameIsNull()
		public virtual void testConsumerBuilderImplWhenSubscriptionNameIsNull()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).subscriptionName(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenSubscriptionNameIsBlank()
		public virtual void testConsumerBuilderImplWhenSubscriptionNameIsBlank()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).subscriptionName(" ");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testConsumerBuilderImplWhenConsumerEventListenerIsNull()
		public virtual void testConsumerBuilderImplWhenConsumerEventListenerIsNull()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).subscriptionName("subscriptionName").consumerEventListener(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testConsumerBuilderImplWhenCryptoKeyReaderIsNull()
		public virtual void testConsumerBuilderImplWhenCryptoKeyReaderIsNull()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).subscriptionName("subscriptionName").cryptoKeyReader(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testConsumerBuilderImplWhenCryptoFailureActionIsNull()
		public virtual void testConsumerBuilderImplWhenCryptoFailureActionIsNull()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).subscriptionName("subscriptionName").cryptoFailureAction(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenConsumerNameIsNull()
		public virtual void testConsumerBuilderImplWhenConsumerNameIsNull()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).consumerName(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenConsumerNameIsBlank()
		public virtual void testConsumerBuilderImplWhenConsumerNameIsBlank()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).consumerName(" ");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertyKeyIsNull()
		public virtual void testConsumerBuilderImplWhenPropertyKeyIsNull()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).property(null, "Test-Value");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertyKeyIsBlank()
		public virtual void testConsumerBuilderImplWhenPropertyKeyIsBlank()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).property("   ", "Test-Value");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertyValueIsNull()
		public virtual void testConsumerBuilderImplWhenPropertyValueIsNull()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).property("Test-Key", null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertyValueIsBlank()
		public virtual void testConsumerBuilderImplWhenPropertyValueIsBlank()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).property("Test-Key", "   ");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConsumerBuilderImplWhenPropertiesAreCorrect()
		public virtual void testConsumerBuilderImplWhenPropertiesAreCorrect()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key"] = "Test-Value";
			properties["Test-Key2"] = "Test-Value2";

			consumerBuilderImpl.topic(TOPIC_NAME).properties(properties);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertiesKeyIsNull()
		public virtual void testConsumerBuilderImplWhenPropertiesKeyIsNull()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties[null] = "Test-Value";

			consumerBuilderImpl.topic(TOPIC_NAME).properties(properties);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertiesKeyIsBlank()
		public virtual void testConsumerBuilderImplWhenPropertiesKeyIsBlank()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["  "] = "Test-Value";

			consumerBuilderImpl.topic(TOPIC_NAME).properties(properties);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertiesValueIsNull()
		public virtual void testConsumerBuilderImplWhenPropertiesValueIsNull()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key"] = null;

			consumerBuilderImpl.topic(TOPIC_NAME).properties(properties);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertiesValueIsBlank()
		public virtual void testConsumerBuilderImplWhenPropertiesValueIsBlank()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties["Test-Key"] = "   ";

			consumerBuilderImpl.topic(TOPIC_NAME).properties(properties);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPropertiesIsEmpty()
		public virtual void testConsumerBuilderImplWhenPropertiesIsEmpty()
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();

			consumerBuilderImpl.topic(TOPIC_NAME).properties(properties);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testConsumerBuilderImplWhenPropertiesIsNull()
		public virtual void testConsumerBuilderImplWhenPropertiesIsNull()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).properties(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testConsumerBuilderImplWhenSubscriptionInitialPositionIsNull()
		public virtual void testConsumerBuilderImplWhenSubscriptionInitialPositionIsNull()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).subscriptionInitialPosition(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = NullPointerException.class) public void testConsumerBuilderImplWhenSubscriptionTopicsModeIsNull()
		public virtual void testConsumerBuilderImplWhenSubscriptionTopicsModeIsNull()
		{
			consumerBuilderImpl.topic(TOPIC_NAME).subscriptionTopicsMode(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenNegativeAckRedeliveryDelayPropertyIsNegative()
		public virtual void testConsumerBuilderImplWhenNegativeAckRedeliveryDelayPropertyIsNegative()
		{
			consumerBuilderImpl.negativeAckRedeliveryDelay(-1, TimeUnit.MILLISECONDS);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPriorityLevelPropertyIsNegative()
		public virtual void testConsumerBuilderImplWhenPriorityLevelPropertyIsNegative()
		{
			consumerBuilderImpl.priorityLevel(-1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenMaxTotalReceiverQueueSizeAcrossPartitionsPropertyIsNegative()
		public virtual void testConsumerBuilderImplWhenMaxTotalReceiverQueueSizeAcrossPartitionsPropertyIsNegative()
		{
			consumerBuilderImpl.maxTotalReceiverQueueSizeAcrossPartitions(-1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenPatternAutoDiscoveryPeriodPropertyIsNegative()
		public virtual void testConsumerBuilderImplWhenPatternAutoDiscoveryPeriodPropertyIsNegative()
		{
			consumerBuilderImpl.patternAutoDiscoveryPeriod(-1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenBatchReceivePolicyIsNull()
		public virtual void testConsumerBuilderImplWhenBatchReceivePolicyIsNull()
		{
			consumerBuilderImpl.batchReceivePolicy(null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testConsumerBuilderImplWhenBatchReceivePolicyIsNotValid()
		public virtual void testConsumerBuilderImplWhenBatchReceivePolicyIsNotValid()
		{
			consumerBuilderImpl.batchReceivePolicy(BatchReceivePolicy.builder().maxNumMessages(0).maxNumBytes(0).timeout(0, TimeUnit.MILLISECONDS).build());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConsumerBuilderImplWhenNumericPropertiesAreValid()
		public virtual void testConsumerBuilderImplWhenNumericPropertiesAreValid()
		{
			consumerBuilderImpl.negativeAckRedeliveryDelay(1, TimeUnit.MILLISECONDS);
			consumerBuilderImpl.priorityLevel(1);
			consumerBuilderImpl.maxTotalReceiverQueueSizeAcrossPartitions(1);
			consumerBuilderImpl.patternAutoDiscoveryPeriod(1);
		}

	}

}