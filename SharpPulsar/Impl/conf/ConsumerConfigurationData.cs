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
namespace SharpPulsar.Impl.Conf
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	using JsonIgnore = com.fasterxml.jackson.annotation.JsonIgnore;
	using Maps = com.google.common.collect.Maps;
	using Sets = com.google.common.collect.Sets;


	using AllArgsConstructor = lombok.AllArgsConstructor;
	using Data = lombok.Data;
	using NoArgsConstructor = lombok.NoArgsConstructor;
	using BatchReceivePolicy = SharpPulsar.Api.BatchReceivePolicy;
	using ConsumerCryptoFailureAction = SharpPulsar.Api.ConsumerCryptoFailureAction;
	using ConsumerEventListener = SharpPulsar.Api.ConsumerEventListener;
	using CryptoKeyReader = SharpPulsar.Api.CryptoKeyReader;
	using DeadLetterPolicy = SharpPulsar.Api.DeadLetterPolicy;
	using KeySharedPolicy = SharpPulsar.Api.KeySharedPolicy;
	using SharpPulsar.Api;
	using RegexSubscriptionMode = SharpPulsar.Api.RegexSubscriptionMode;
	using SubscriptionInitialPosition = SharpPulsar.Api.SubscriptionInitialPosition;
	using SubscriptionType = SharpPulsar.Api.SubscriptionType;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @NoArgsConstructor @AllArgsConstructor public class ConsumerConfigurationData<T> implements java.io.Serializable, Cloneable
	[Serializable]
	public class ConsumerConfigurationData<T> : ICloneable
	{
		private const long SerialVersionUID = 1L;

		private ISet<string> topicNames = Sets.newTreeSet();

		private Pattern topicsPattern;

		private string subscriptionName;

		private SubscriptionType subscriptionType = SubscriptionType.Exclusive;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private SharpPulsar.api.MessageListener<T> messageListener;
		private MessageListener<T> messageListener;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private SharpPulsar.api.ConsumerEventListener consumerEventListener;
		private ConsumerEventListener consumerEventListener;

		private int receiverQueueSize = 1000;

		private long acknowledgementsGroupTimeMicros = BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.toMicros(100);

		private long negativeAckRedeliveryDelayMicros = BAMCIS.Util.Concurrent.TimeUnit.MINUTES.toMicros(1);

		private int maxTotalReceiverQueueSizeAcrossPartitions = 50000;

		private string consumerName = null;

		private long ackTimeoutMillis = 0;

		private long tickDurationMillis = 1000;

		private int priorityLevel = 0;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private SharpPulsar.api.CryptoKeyReader cryptoKeyReader = null;
		private CryptoKeyReader cryptoKeyReader = null;

		private ConsumerCryptoFailureAction cryptoFailureAction = ConsumerCryptoFailureAction.FAIL;

		private SortedDictionary<string, string> properties = new SortedDictionary<string, string>();

		private bool readCompacted = false;

		private SubscriptionInitialPosition subscriptionInitialPosition = SubscriptionInitialPosition.Latest;

		private int patternAutoDiscoveryPeriod = 1;

		private RegexSubscriptionMode regexSubscriptionMode = RegexSubscriptionMode.PersistentOnly;

		private DeadLetterPolicy deadLetterPolicy;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private SharpPulsar.api.BatchReceivePolicy batchReceivePolicy;
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

		public virtual ConsumerConfigurationData<T> Clone()
		{
			try
			{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") ConsumerConfigurationData<T> c = (ConsumerConfigurationData<T>) super.clone();
				ConsumerConfigurationData<T> C = (ConsumerConfigurationData<T>) base.Clone();
				C.topicNames = Sets.newTreeSet(this.topicNames);
				C.properties = Maps.newTreeMap(this.properties);
				return C;
			}
			catch (CloneNotSupportedException)
			{
				throw new Exception("Failed to clone ConsumerConfigurationData");
			}
		}
	}

}