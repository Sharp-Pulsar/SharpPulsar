using TimeUnit = BAMCIS.Util.Concurrent.TimeUnit;
using Pulsar.Api;
using SharpPulsar.Enum;
using SharpPulsar.Interface.Consumer;
using SharpPulsar.Interface.Message;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SharpPulsar.Util;
using SharpPulsar.Interface;
using SharpPulsar.Entity;

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
	public sealed class ConsumerConfigurationData<T>
	{
		public const long serialVersionUID = 1L;

		public HashSet<string> TopicNames = new HashSet<string>();

		public Regex TopicsPattern;

		public string SubscriptionName;

		public SubscriptionType SubscriptionType = SubscriptionType.Exclusive;
		public IMessageListener<T> MessageListener;
		public IConsumerEventListener SonsumerEventListener;

		public int ReceiverQueueSize = 1000;

		public long AcknowledgementsGroupTimeMicros = TimeUnit.MILLISECONDS.ToMicros(100);

		public long NegativeAckRedeliveryDelayMicros = TimeUnit.MINUTES.ToMicros(1);

		public int MaxTotalReceiverQueueSizeAcrossPartitions = 50000;

		public string ConsumerName = null;

		public long AckTimeoutMillis = 0;

		public long TickDurationMillis = 1000;

		public int PriorityLevel = 0;

		public ICryptoKeyReader CryptoKeyReader = null;

		public ConsumerCryptoFailureAction CryptoFailureAction = ConsumerCryptoFailureAction.FAIL;

		public SortedDictionary<string, string> Properties = new SortedDictionary<string, string>();

		public bool ReadCompacted = false;

		public SubscriptionInitialPosition SubscriptionInitialPosition = SubscriptionInitialPosition.Latest;

		public int PatternAutoDiscoveryPeriod = 1;

		public RegexSubscriptionMode RegexSubscriptionMode = RegexSubscriptionMode.PersistentOnly;

		public DeadLetterPolicy DeadLetterPolicy;
		public BatchReceivePolicy BatchReceivePolicy;

		public bool AutoUpdatePartitions = true;

		public bool ReplicateSubscriptionState = false;

		public bool ResetIncludeHead = false;

		public KeySharedPolicy KeySharedPolicy;
		public string SingleTopic
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

		public IConsumerEventListener ConsumerEventListener { get; internal set; }
	}

}