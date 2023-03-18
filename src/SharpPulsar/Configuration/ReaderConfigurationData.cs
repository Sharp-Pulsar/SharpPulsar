using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Common.Compression;
using SharpPulsar.Interfaces;

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
    public sealed class ReaderConfigurationData<T>
	{
        public IList<Common.Range> KeyHashRanges { get; set; }
		public IMessageId StartMessageId { get; set; }
		public IConsumerEventListener EventListener { get; set; }
        public bool AutoUpdatePartitions { get; set; } = true;
        public TimeSpan AutoUpdatePartitionsInterval = TimeSpan.FromSeconds(60);
        public bool PoolMessages { get; set; } = false;
        public long StartMessageFromRollbackDurationInSec { get; set; }
        public ISchema<T> Schema { get; set; }
		public int ReceiverQueueSize { get; set; } = 1000;

		public IReaderListener<T> ReaderListener { get; set; }

		public ICryptoKeyReader CryptoKeyReader { get; set; }
		public ConsumerCryptoFailureAction CryptoFailureAction { get; set; } = ConsumerCryptoFailureAction.Fail;

		public bool ReadCompacted { get; set; } = false;
		public bool ResetIncludeHead { get; set; } = true;
        [NonSerialized]
        public IList<IReaderInterceptor<T>> ReaderInterceptorList;
        public string SubscriptionRolePrefix { get; set; }

		public string TopicName 
		{ 
			get 
			{
				if (TopicNames.Count > 1)
				{
					throw new ArgumentException("topicNames needs to be = 1");
				}
				return TopicNames.FirstOrDefault();
			} 
			set
            {
				TopicNames.Clear();
				TopicNames.Add(value);
			}

		}
		public List<string> TopicNames { get; set; } = new List<string>();

        public string ReaderName { get; set; }
        public string SubscriptionName { get; set; }

        // max pending chunked message to avoid sending incomplete message into the queue and memory
        public int MaxPendingChunkedMessage { get; set; } = 10;

        public bool AutoAckOldestChunkedMessageOnQueueFull { get; set; } = false;

        public long ExpireTimeOfIncompleteChunkedMessage { get; set; } = (long)TimeSpan.FromMinutes(1).TotalMilliseconds;

    }

}