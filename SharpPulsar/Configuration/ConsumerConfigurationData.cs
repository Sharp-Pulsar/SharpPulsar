using Pulsar.Api;
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
namespace SharpPulsar.Configuration
{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @NoArgsConstructor @AllArgsConstructor public class ConsumerConfigurationData<T> implements java.io.Serializable, Cloneable
	[Serializable]
	public class ConsumerConfigurationData<T> : ICloneable
	{
		private const long serialVersionUID = 1L;

		private ISet<string> topicNames = Sets.newTreeSet();

		private Pattern topicsPattern;

		private string subscriptionName;

		private SubscriptionType subscriptionType = SubscriptionType.Exclusive;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private org.apache.pulsar.client.api.MessageListener<T> messageListener;
		private MessageListener<T> MessageListener;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private org.apache.pulsar.client.api.ConsumerEventListener consumerEventListener;
		private ConsumerEventListener consumerEventListener;

		private int receiverQueueSize = 1000;

		private long acknowledgementsGroupTimeMicros = TimeUnit.MILLISECONDS.toMicros(100);

		private long negativeAckRedeliveryDelayMicros = TimeUnit.MINUTES.toMicros(1);

		private int maxTotalReceiverQueueSizeAcrossPartitions = 50000;

		private string consumerName = null;

		private long ackTimeoutMillis = 0;

		private long tickDurationMillis = 1000;

		private int priorityLevel = 0;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private org.apache.pulsar.client.api.CryptoKeyReader cryptoKeyReader = null;
		private CryptoKeyReader cryptoKeyReader = null;

		private ConsumerCryptoFailureAction cryptoFailureAction = ConsumerCryptoFailureAction.FAIL;

		private SortedDictionary<string, string> properties = new SortedDictionary<string, string>();

		private bool readCompacted = false;

		private SubscriptionInitialPosition subscriptionInitialPosition = SubscriptionInitialPosition.Latest;

		private int patternAutoDiscoveryPeriod = 1;

		private RegexSubscriptionMode regexSubscriptionMode = RegexSubscriptionMode.PersistentOnly;

		private DeadLetterPolicy deadLetterPolicy;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private org.apache.pulsar.client.api.BatchReceivePolicy batchReceivePolicy;
		private BatchReceivePolicy batchReceivePolicy;

		private bool autoUpdatePartitions = true;

		private bool replicateSubscriptionState = false;

		private bool resetIncludeHead = false;

		private KeySharedPolicy keySharedPolicy;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore public String getSingleTopic()
		public virtual string SingleTopic
		{
			get
			{
				checkArgument(topicNames.Count == 1);
				return topicNames.GetEnumerator().next();
			}
		}

		public virtual ConsumerConfigurationData<T> clone()
		{
			try
			{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") ConsumerConfigurationData<T> c = (ConsumerConfigurationData<T>) super.clone();
				ConsumerConfigurationData<T> c = (ConsumerConfigurationData<T>) base.clone();
				c.topicNames = Sets.newTreeSet(this.topicNames);
				c.properties = Maps.newTreeMap(this.properties);
				return c;
			}
			catch (CloneNotSupportedException)
			{
				throw new Exception("Failed to clone ConsumerConfigurationData");
			}
		}

		public object Clone()
		{
			throw new NotImplementedException();
		}
	}

}