using SharpPulsar.Api;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SharpPulsar.Utility;

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
		private const long SerialVersionUid = 1L;

		public ISet<string> TopicNames { get; set; } = new SortedSet<string>();

		[NonSerialized]
		private Regex _topicsPattern;

		[NonSerialized]
		private string _subscriptionName;

		public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Exclusive;
		public IMessageListener<T> MessageListener { get; set; }
		public IConsumerEventListener ConsumerEventListener { get; set; }

		public int ReceiverQueueSize { get; set; } = 1000;

		public long AcknowledgementsGroupTimeMicros { get; set; } = BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.ToMicros(100);

		public long NegativeAckRedeliveryDelayMicros { get; set; } = BAMCIS.Util.Concurrent.TimeUnit.MINUTES.ToMicros(1);

		public int MaxTotalReceiverQueueSizeAcrossPartitions { get; set; } = 50000;

		[NonSerialized]
		private string _consumerName;

		public long AckTimeoutMillis { get; set; } = 0;

		public long TickDurationMillis { get; set; } = 1000;

		public int PriorityLevel { get; set; } = 0;

		public ICryptoKeyReader CryptoKeyReader { get; set; }

		public ConsumerCryptoFailureAction CryptoFailureAction { get; set; } = ConsumerCryptoFailureAction.Fail;

		[NonSerialized]
		private SortedDictionary<string, string> _properties  = new SortedDictionary<string, string>();

		[NonSerialized]
		private bool _readCompacted = false;

		[NonSerialized]
		private SubscriptionInitialPosition _subscriptionInitialPosition = SubscriptionInitialPosition.Latest;

		public int PatternAutoDiscoveryPeriod { get; set; } = 1;

		public RegexSubscriptionMode RegexSubscriptionMode { get; set; } = RegexSubscriptionMode.PersistentOnly;

		[NonSerialized]
		private DeadLetterPolicy _deadLetterPolicy;
		[NonSerialized]
		private BatchReceivePolicy _batchReceivePolicy;

        public BatchReceivePolicy BatchReceivePolicy
        {
            get => _batchReceivePolicy;
            set => _batchReceivePolicy = value;
        }

		public bool AutoUpdatePartitions { get; set; } = true;

		public bool ReplicateSubscriptionState { get; set; } = false;

		public bool ResetIncludeHead { get; set; } = false;

		[NonSerialized]
		private KeySharedPolicy _keySharedPolicy;

        public KeySharedPolicy KeySharedPolicy
        {
            get => _keySharedPolicy;
            set => _keySharedPolicy = value;
        }


		public bool ReadCompacted
        {
            get => _readCompacted;
            set => _readCompacted = value;
        }

        public DeadLetterPolicy DeadLetterPolicy
        {
            get => _deadLetterPolicy;
            set => _deadLetterPolicy = value;
        }

		public SubscriptionInitialPosition SubscriptionInitialPosition
        {
            get => _subscriptionInitialPosition;
            set => _subscriptionInitialPosition = value;
        }
		public Regex TopicsPattern
        {
            get => _topicsPattern;
            set => _topicsPattern = value;
        }

        public SortedDictionary<string, string> Properties
        {
            get => _properties;
            set => _properties = value;
        }

		public string ConsumerName
        {
            get => _consumerName;
            set => _consumerName = value;
        }

		public string SubscriptionName
        {
            get => _subscriptionName;
            set => _subscriptionName = value;
        }
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
				var c = (ConsumerConfigurationData<T>) base.MemberwiseClone();
				c.TopicNames = new SortedSet<string>(TopicNames);
				c.Properties = new SortedDictionary<string, string>(Properties);
				return c;
			}
			catch (System.Exception)
			{
				throw new System.Exception("Failed to clone ConsumerConfigurationData");
			}
		}

		object ICloneable.Clone()
		{
			return this;
		}
	}

}