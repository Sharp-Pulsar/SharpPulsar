using SharpPulsar.Api;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SharpPulsar.Protocol.Proto;
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
	public sealed class ConsumerConfigurationData
	{
        public BatchMessageId StartMessageId { get; set; } 
		public ConsumptionType? ConsumptionType { get; set; }
		public ISet<string> TopicNames { get; set; } = new SortedSet<string>();
		public List<IConsumerInterceptor> Interceptors { get; set; }
		public CommandSubscribe.SubType SubscriptionType { get; set; } = CommandSubscribe.SubType.Exclusive;
		public IMessageListener MessageListener { get; set; }
        public bool ForceTopicCreation { get; set; } = false;
		public IConsumerEventListener ConsumerEventListener { get; set; }
		public ISchema Schema { get; set; }
        public bool UseTls { get; set; } = false;
		public int ReceiverQueueSize { get; set; } = 1_000;

		public long AcknowledgementsGroupTimeMicros { get; set; } = BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.ToMicros(100);

		public long NegativeAckRedeliveryDelayMicros { get; set; } = BAMCIS.Util.Concurrent.TimeUnit.MINUTES.ToMicros(1);

		public int MaxTotalReceiverQueueSizeAcrossPartitions { get; set; } = 50000;

		public long AckTimeoutMillis { get; set; } = 0;

		public long TickDurationMillis { get; set; } = 1000;

		public int PriorityLevel { get; set; } = 0;

		public ICryptoKeyReader CryptoKeyReader { get; set; }

		public ConsumerCryptoFailureAction CryptoFailureAction { get; set; } = ConsumerCryptoFailureAction.Fail;

		public int PatternAutoDiscoveryPeriod { get; set; } = 1;

		public RegexSubscriptionMode RegexSubscriptionMode { get; set; } = RegexSubscriptionMode.PersistentOnly;

		
        public BatchReceivePolicy BatchReceivePolicy { get; set; }

		public bool AutoUpdatePartitions { get; set; } = true;

		public bool ReplicateSubscriptionState { get; set; } = false;

		public bool ResetIncludeHead { get; set; } = false;

        public KeySharedPolicy KeySharedPolicy { get; set; }


		public bool ReadCompacted { get; set; }

		public DeadLetterPolicy DeadLetterPolicy { get; set; }

        public SubscriptionInitialPosition SubscriptionInitialPosition { get; set; } =
            SubscriptionInitialPosition.Earliest;
		public Regex TopicsPattern { get; set; }

		public SortedDictionary<string, string> Properties { get; set; }

		public string ConsumerName { get; set; }

		public string SubscriptionName { get; set; }
		public string SingleTopic
		{
			get
			{
				if(TopicNames.Count == 1)
				{
					return TopicNames.First();
				}
				return string.Empty;
			}
		}

    }

    public enum ConsumptionType
    {
		Queue,
		Listener
    }
}