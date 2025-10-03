
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SharpPulsar.Common.Precondition;
using SharpPulsar.Builder;
using SharpPulsar.API;
using SharpPulsar.Shared;
using SharpPulsar.Crypto;

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
    public class ProducerConfigurationData
    {
        public long BatchingMaxPublishDelayMillis { get; set; } = 1000;
        private int _batchingPartitionSwitchFrequencyByPublishDelay = 10;

        /// <summary>
        /// The maximum number of messages permitted in a batch.
        /// </summary>
        public int BatchingMaxMessages { get; set; } = DefaultBatchingMaxMessages;
		public int BatchingMaxBytes { get; set; } = 128 * 1024; // 128KB (keep the maximum consistent as previous versions)

        /// <summary>
        /// Enable batching of messages.
        /// </summary>
        public bool BatchingEnabled { get; set; } = true; // enabled by default
        public MessageCrypto MessageCrypto { get; set; }

        public Action<Messages.AckReceived> AckReceivedListerner { get; set; }
		public IProducerEventListener ProducerEventListener { get; set; }
        public const int DefaultBatchingMaxMessages = 1000;
		public const int DefaultMaxPendingMessages = 0;
		public const int DefaultMaxPendingMessagesAcrossPartitions = 0;
        /// <summary>
        /// MaxMessageSize is set at the server side,
        /// But when we need a smaller size than the size set by the server when chunking
        /// we can do it here
        /// </summary>
        /// 
        public Shared.ProducerAccessMode AccessMode = Shared.ProducerAccessMode.Shared;
        // public int MaxMessageSize { get; set; } = -1;

        /// <summary>
        /// Topic name
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// Use this configuration to automatically create an initial subscription when creating a topic.
        /// If this field is not set, the initial subscription is not created.
        /// </summary>
        public string InitialSubscriptionName { get; set; } = "";
        public int Partitions { get; set; } = 0;

        public bool IsNonPartitionedTopicExpected { get; set; } = false;

        public bool IsReplProducer {  get; set; }
        
        private long _autoUpdatePartitionsIntervalSeconds = 60;
        public bool UseTls { get; set; } = false;

        /// <summary>
        /// Message send timeout in ms.
        /// If a message is not acknowledged by a server before the `sendTimeout` expires, an error occurs.
        /// </summary>
		public TimeSpan SendTimeoutMs { get; set; } = TimeSpan.FromMilliseconds(30000);

        /// <summary>
        /// If it is set to `true`, when the outgoing message queue is full, the `Send` and `SendAsync`
        /// methods of producer block, rather than failing and throwing errors.\n"
        /// If it is set to `false`, when the outgoing message queue is full, the `Send` and `SendAsync`
        /// methods of producer fail and `ProducerQueueIsFullError` exceptions occur.
        /// The `MaxPendingMessages` parameter determines the size of the outgoing message queue.
        /// </summary>
        public bool BlockIfQueueFull { get; set; } = false;

        /// <summary>
        /// Message routing logic for producers on [partitioned topics]
        /// (https://pulsar.apache.org/docs/concepts-architecture-overview#partitioned-topics).
        /// Apply the logic only when setting no key on messages.
        /// Available options are as follows:
        /// * `pulsar.RoundRobinDistribution`: round robin
        /// * `pulsar.UseSinglePartition`: publish all messages to a single partition
        /// * `pulsar.CustomPartition`: a custom partitioning scheme
        /// </summary>
        public MessageRoutingMode MessageRoutingMode { get; set; } = MessageRoutingMode.RoundRobinMode;

        /// <summary>
        /// Hashing function determining the partition where you publish a particular message (partitioned
        /// topics only).
        /// Available options are as follows:
        /// * `pulsar.JavastringHash`: the equivalent of `string.hashCode()` in Java
        /// * `pulsar.Murmur3_32Hash`: applies the [Murmur3](https://en.wikipedia.org/wiki/MurmurHash)
        /// hashing function\n"
        /// * `pulsar.BoostHash`: applies the hashing function from C++'s
        /// [Boost](https://www.boost.org/doc/libs/1_62_0/doc/html/hash.html) library
        /// </summary>
		public HashingScheme HashingScheme { get; set; } = HashingScheme.JavaStringHash;
        public bool NonPartitionedTopicExpected { get; set; } = false;  

        /// <summary>
        /// Producer should take action when encryption fails.
        /// * **FAIL**: if encryption fails, unencrypted messages fail to send.
        /// * **SEND**: if encryption fails, unencrypted messages are sent.
        /// </summary>
        public ProducerCryptoFailureAction CryptoFailureAction { get; set; } = ProducerCryptoFailureAction.Fail;
        public IMessageRouter CustomMessageRouter { get; set; } = null;
        /// <summary>
        /// Batching time period of sending messages.
        /// </summary>
        public long BatchingMaxPublishDelayMicros = TimeUnit.TimeUnit.MILLISECONDS.ToMicroseconds(1);

        /// <summary>
        /// Enable chunking of messages.
        /// </summary>
        public bool ChunkingEnabled { get; set; } = false;
        public int ChunkMaxMessageSize { get; set; } = -1;
        private int _maxPendingMessagesAcrossPartitions = DefaultMaxPendingMessagesAcrossPartitions;
        private int _maxPendingMessages = DefaultMaxPendingMessages;

        [JsonIgnore]
		public IBatcherBuilder BatcherBuilder { get; set; }

        [JsonIgnore]
		public ICryptoKeyReader CryptoKeyReader { get; set; } = null;

		public ISet<string> EncryptionKeys { get; set; } = new SortedSet<string>();

        /// <summary>
        /// Message data compression type used by a producer.
        /// Available options:
        /// * [LZ4](https://github.com/lz4/lz4)
        /// * [ZLIB](https://zlib.net/)
        /// * [ZSTD](https://facebook.github.io/zstd/)
        /// * [SNAPPY](https://google.github.io/snappy/)
        /// </summary>
		public Shared.CompressionType CompressionType { get; set; } = Shared.CompressionType.NONE;

        public int CompressMinMsgBodySize = 4 * 1024; // 4kb

        public double BatchingMaxPublishDelayMs = TimeSpan.FromMilliseconds(5000).TotalMilliseconds;

		public long? InitialSequenceId { get; set; }

		public bool AutoUpdatePartitions { get; set; } = true;

		public bool MultiSchema { get; set; } = true;
        public bool LazyStartPartitionedProducers { get; set; } = false;
        public SortedDictionary<string, string> Properties { get; set; }
        
		/// 
		/// <summary>
		/// Returns true if encryption keys are added
		/// 
		/// </summary>
		/// 
		public virtual bool EncryptionEnabled => (EncryptionKeys != null) && EncryptionKeys.Count > 0 && (CryptoKeyReader != null);

		public virtual void SetAutoUpdatePartitionsIntervalSeconds(TimeSpan interval)
        {
            var interv = (long)interval.TotalSeconds;
			Condition.CheckArgument(interv > 0, "interval needs to be > 0");
			_autoUpdatePartitionsIntervalSeconds = interv;
        }

        /// <summary>
        /// The maximum number of pending messages across partitions.
        /// Use the setting to lower the max pending messages for each partition ({@link 
        /// #setMaxPendingMessages(int)}) if the total number exceeds the configured value.
        /// </summary>
        public virtual int MaxPendingMessagesAcrossPartitions
        {
            get { return _maxPendingMessagesAcrossPartitions; }
            set
            {
                Condition.CheckArgument(value >= _maxPendingMessages, "maxPendingMessagesAcrossPartitions needs to be >= maxPendingMessages");
                _maxPendingMessagesAcrossPartitions = value;
            }
        }
        public long AutoUpdatePartitionsIntervalSeconds => _autoUpdatePartitionsIntervalSeconds;

        /// <summary>
        /// producer name
        /// </summary>
		public string ProducerName { get; set; }

        /// <summary>
        /// The maximum size of a queue holding pending messages.
        /// For example, a message waiting to receive an acknowledgment from a [broker]
        /// (https://pulsar.apache.org/docs/reference-terminology#broker).
        /// By default, when the queue is full, all calls to the `Send` and `SendAsync` methods fail
        /// **unless** you set `BlockIfQueueFull` to `true`.
        /// </summary>
		public int MaxPendingMessages
		{
			get => _maxPendingMessages;
            set
			{
                if (value < 1)
                    throw new ArgumentException("maxPendingMessages needs to be > 0");
                else
                    _maxPendingMessages = value;
                    
			}
		}

		public void SetBatchingMaxPublishDelayMs(TimeSpan batchDelay)
		{
			var delayInMs = batchDelay.TotalMilliseconds;
			Condition.CheckArgument(delayInMs >= 1, "configured value for batch delay must be at least 1ms");
			BatchingMaxPublishDelayMs = delayInMs;
		}

		public int BatchingPartitionSwitchFrequencyByPublishDelay
		{
			set
			{
				Condition.CheckArgument(value >= 1, "configured value for partition switch frequency must be >= 1");
				_batchingPartitionSwitchFrequencyByPublishDelay = value;
			}
		}

		public long BatchingPartitionSwitchFrequencyIntervalMicros()
		{
			return _batchingPartitionSwitchFrequencyByPublishDelay * (long)BatchingMaxPublishDelayMs;
		}

		public void SetSendTimeoutMs(TimeSpan sendTimeoutMs)
		{
			if (sendTimeoutMs.TotalMilliseconds < 0)
				throw new ArgumentException("sendTimeout needs to be >= 0");
			SendTimeoutMs = sendTimeoutMs;
		}

	}

}