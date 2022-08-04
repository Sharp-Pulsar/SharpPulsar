
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SharpPulsar.Batch.Api;
using SharpPulsar.Interfaces;
using SharpPulsar.Common;
using static SharpPulsar.Protocol.Proto.CommandSubscribe;
using SharpPulsar.Precondition;
using System;

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
		private TimeSpan _autoUpdatePartitionsInterval = TimeSpan.FromSeconds(5);
		public void SetAutoUpdatePartitionsInterval(TimeSpan interval)
		{
			Condition.CheckArgument(interval.TotalMilliseconds > 0, "interval needs to be > 0");
			_autoUpdatePartitionsInterval = interval;
		}
		public TimeSpan AutoUpdatePartitionsInterval { get => _autoUpdatePartitionsInterval; }
		public IMessageCrypto MessageCrypto { get; set; }
		public IMessageId StartMessageId { get; set; }
        public ConsumptionType ConsumptionType { get; set; } = ConsumptionType.Listener;
		public ISet<string> TopicNames { get; set; } = new SortedSet<string>();
		public List<IConsumerInterceptor<T>> Interceptors { get; set; }
		public SubType SubscriptionType { get; set; } = SubType.Exclusive;
		internal IMessageListener<T> MessageListener { get; set; }
        public bool ForceTopicCreation { get; set; } = false;
		public IConsumerEventListener ConsumerEventListener { get; set; }
        public bool UseTls { get; set; } = false;
		public int ReceiverQueueSize { get; set; } = 1_000;

		public TimeSpan AcknowledgementsGroupTime { get; set; } = TimeSpan.FromMilliseconds(100);

		public TimeSpan NegativeAckRedeliveryDelay { get; set; } = TimeSpan.FromMilliseconds(30000);

		public int MaxTotalReceiverQueueSizeAcrossPartitions { get; set; } = 50000;

		public TimeSpan AckTimeout { get; set; } = TimeSpan.Zero;
		public bool AckReceiptEnabled { get; set; } = false;
		public bool StartPaused { get; set; } = false;

        [NonSerialized]
        public IMessagePayloadProcessor PayloadProcessor = null;

        public TimeSpan TickDuration { get; set; } = TimeSpan.FromMilliseconds(1000);

		public int PriorityLevel { get; set; } = 0;

        public int MaxPendingChuckedMessage { get; set; }
        public TimeSpan ExpireTimeOfIncompleteChunkedMessage { get; set; }
        public bool AutoAckOldestChunkedMessageOnQueueFull { get; set; }

        public bool BatchConsume { get; set; } = false;
        public bool BatchIndexAckEnabled { get; set; } = false;
		public TimeSpan BatchConsumeTimeout { get; set; } = TimeSpan.FromMilliseconds(30_000); //30 seconds

		public ICryptoKeyReader CryptoKeyReader { get; set; }

		public ConsumerCryptoFailureAction CryptoFailureAction { get; set; } = ConsumerCryptoFailureAction.Fail;

		public int PatternAutoDiscoveryPeriod { get; set; } = 30;

	    public SubscriptionMode SubscriptionMode = SubscriptionMode.Durable;

        public IDictionary<string, string> SubscriptionProperties;

        public RegexSubscriptionMode RegexSubscriptionMode { get; set; } = RegexSubscriptionMode.PersistentOnly;


        public BatchReceivePolicy BatchReceivePolicy { get; set; } 

		public bool AutoUpdatePartitions { get; set; } = true;

		public bool ReplicateSubscriptionState { get; set; } = false;
		public bool RetryEnable { get; set; } = false;

		public bool ResetIncludeHead { get; set; } = false;

        public KeySharedPolicy KeySharedPolicy { get; set; }


		public bool ReadCompacted { get; set; }

		public DeadLetterPolicy DeadLetterPolicy { get; set; }

        public SubscriptionInitialPosition SubscriptionInitialPosition { get; set; } =
            SubscriptionInitialPosition.Earliest;
		public Regex TopicsPattern { get; set; }

		public SortedDictionary<string, string> Properties { get; set; } = new SortedDictionary<string, string>();

		public string ConsumerName { get; set; }

		public string SubscriptionName { get; set; }
		public string SingleTopic
		{
			get => TopicNames.Count == 1 ? TopicNames.First() : string.Empty;
            set => TopicNames = new HashSet<string> {value};
        }

    }

    public enum ConsumptionType
    {
		Queue,
		Listener
    }
}