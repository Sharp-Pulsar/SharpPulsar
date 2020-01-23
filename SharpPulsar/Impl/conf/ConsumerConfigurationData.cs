using SharpPulsar.Api;
using SharpPulsar.Util;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

	[Serializable]
	public class ConsumerConfigurationData<T> : ICloneable
	{
		private const long SerialVersionUID = 1L;

		public ISet<string> TopicNames = new SortedSet<string>();

		[NonSerialized]
		public Regex TopicsPattern;

		[NonSerialized]
		private string subscriptionName;

		public SubscriptionType SubscriptionType = SubscriptionType.Exclusive;
		public MessageListener<T> MessageListener;
		public ConsumerEventListener ConsumerEventListener;

		public int ReceiverQueueSize = 1000;

		public long AcknowledgementsGroupTimeMicros = BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.ToMicros(100);

		public long NegativeAckRedeliveryDelayMicros = BAMCIS.Util.Concurrent.TimeUnit.MINUTES.ToMicros(1);

		public int MaxTotalReceiverQueueSizeAcrossPartitions = 50000;

		[NonSerialized]
		public string ConsumerName = null;

		public long AckTimeoutMillis = 0;

		public long TickDurationMillis = 1000;

		public int PriorityLevel = 0;

		public CryptoKeyReader CryptoKeyReader = null;

		public ConsumerCryptoFailureAction CryptoFailureAction = ConsumerCryptoFailureAction.FAIL;

		[NonSerialized]
		public SortedDictionary<string, string> Properties = new SortedDictionary<string, string>();

		[NonSerialized]
		public bool ReadCompacted = false;

		[NonSerialized]
		public SubscriptionInitialPosition SubscriptionInitialPosition = SubscriptionInitialPosition.Latest;

		public int PatternAutoDiscoveryPeriod = 1;

		public RegexSubscriptionMode RegexSubscriptionMode = RegexSubscriptionMode.PersistentOnly;

		[NonSerialized]
		public DeadLetterPolicy DeadLetterPolicy;
		[NonSerialized]
		public BatchReceivePolicy BatchReceivePolicy;

		public bool AutoUpdatePartitions = true;

		public bool ReplicateSubscriptionState = false;

		public bool ResetIncludeHead = false;

		[NonSerialized]
		public KeySharedPolicy KeySharedPolicy;

		public virtual string SingleTopic
		{
			get
			{
				if(TopicNames.Count == 1)
				{
					TopicNames.GetEnumerator().MoveNext();
					return TopicNames.GetEnumerator().Current;
				}
				return string.Empty;
			}
		}

		public virtual ConsumerConfigurationData<T> Clone()
		{
			try
			{
//JAVA 
				ConsumerConfigurationData<T> C = (ConsumerConfigurationData<T>) base.MemberwiseClone();
				C.TopicNames = new SortedSet<string>(TopicNames);
				C.Properties = new SortedDictionary<string, string>(Properties);
				return C;
			}
			catch (System.Exception)
			{
				throw new System.Exception("Failed to clone ConsumerConfigurationData");
			}
		}

		object ICloneable.Clone()
		{
			throw new NotImplementedException();
		}
	}

}