using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SharpPulsar.API;
using SharpPulsar.Crypto;
using SharpPulsar.Shared;

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

        /// <summary>
        /// Size of a consumer's receiver queue.
        /// For example, the number of messages that can be accumulated by a consumer before an
        /// application calls `Receive`.
        /// A value higher than the default value increases consumer throughput, though at the expense of
        /// more memory utilization.
        /// </summary>
		public int ReceiverQueueSize { get; set; } = 1000;

        /// <summary>
        /// A listener that is called for message received.
        /// </summary>
		public IReaderListener<T> ReaderListener { get; set; }

        /// <summary>
        /// Interface that abstracts the access to a key store.
        /// </summary>
		public ICryptoKeyReader CryptoKeyReader { get; set; }

        /// <summary>
        /// Consumer should take action when it receives a message that can not be decrypted.\n"
        /// * **FAIL**: this is the default option to fail messages until crypto succeeds.
        /// * **DISCARD**: silently acknowledge and not deliver message to an application.
        /// * **CONSUME**: deliver encrypted messages to applications. It is the application's
        ///  responsibility to decrypt the message.
        /// The message decompression fails.
        /// If messages contain batch messages, a client is not be able to retrieve individual messages in
        /// batch.
        /// Delivered encrypted message contains {@link EncryptionContext} which contains encryption and
        /// compression information in it using which application can decrypt consumed message payload.
        /// </summary>
		public Shared.ConsumerCryptoFailureAction CryptoFailureAction { get; set; } = Shared.ConsumerCryptoFailureAction.FAIL;

        /// <summary>
        /// If enabling `readCompacted`, a consumer reads messages from a compacted topic rather than a full
        /// message backlog of a topic.
        /// A consumer only sees the latest value for each key in the compacted topic, up until reaching
        /// the point in the topic message when compacting backlog. Beyond that point, send messages as
        /// normal.
        /// `readCompacted` can only be enabled on subscriptions to persistent topics, which have a single
        /// active consumer (for example, failure or exclusive subscriptions).
        /// Attempting to enable it on subscriptions to non-persistent topics or on shared subscriptions
        /// leads to a subscription call throwing a `PulsarClientException`.
        /// </summary>
		public bool ReadCompacted { get; set; } = false;

        /// <summary>
        /// If set to true, the first message to be returned is the one specified by `messageId`.
        /// If set to false, the first message to be returned is the one next to the message specified by
        /// `messageId`.
        /// </summary>
		public bool ResetIncludeHead { get; set; } = false;
        [NonSerialized]
        public IList<IReaderInterceptor<T>> ReaderInterceptorList;

        /// <summary>
        /// Prefix of subscription role.
        /// </summary>
        public string SubscriptionRolePrefix { get; set; }

        /// <summary>
        /// Topic name
        /// </summary>
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

        /// <summary>
        /// Reader name
        /// </summary>
        public string ReaderName { get; set; }

        /// <summary>
        /// Subscription name
        /// </summary>
        public string SubscriptionName { get; set; }

        // max pending chunked message to avoid sending incomplete message into the queue and memory
        public int MaxPendingChunkedMessage { get; set; } = 10;

        public bool AutoAckOldestChunkedMessageOnQueueFull { get; set; } = false;

        public long ExpireTimeOfIncompleteChunkedMessageMillis { get; set; } = TimeUnit.TimeUnit.MINUTES.ToMilliseconds(1);
        public SubscriptionInitialPosition SubscriptionInitialPosition = SubscriptionInitialPosition.Latest;

        [JsonIgnore]
        public MessageCrypto MessageCrypto = null;

    }

}