
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Batch.Api;
using SharpPulsar.Interfaces;
using SharpPulsar.Common;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Precondition;
using SharpPulsar.Protocol.Proto;

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
        private int _batchingPartitionSwitchFrequencyByPublishDelay;
        public int BatchingMaxMessages { get; set; } = DefaultBatchingMaxMessages;
		public int BatchingMaxBytes { get; set; } = 128 * 1024; // 128KB (keep the maximum consistent as previous versions)
        public bool BatchingEnabled { get; set; } = false;
		public IMessageCrypto MessageCrypto { get; set; }
		public IProducerEventListener ProducerEventListener { get; set; }
        public const int DefaultBatchingMaxMessages = 1000;
		public const int DefaultMaxPendingMessages = 1000;
		public const int DefaultMaxPendingMessagesAcrossPartitions = 50000;
        public int MaxMessageSize { get; set; } = 2 * 1024; //2kb
        public string TopicName { get; set; }
        public int Partitions { get; set; } = 0;

		private long _autoUpdatePartitionsIntervalSeconds = 60;
        public bool UseTls { get; set; } = false;
		public long SendTimeoutMs { get; set; } = 30000;
        public MessageRoutingMode MessageRoutingMode { get; set; } = MessageRoutingMode.RoundRobinMode;
		public HashingScheme HashingScheme { get; set; } = HashingScheme.JavaStringHash;

		public ProducerCryptoFailureAction CryptoFailureAction { get; set; } = ProducerCryptoFailureAction.Fail;
        public IMessageRouter CustomMessageRouter { get; set; } = null;
		public bool ChunkingEnabled { get; set; } = false;

		[JsonIgnore]
		public IBatcherBuilder BatcherBuilder { get; set; }

        [JsonIgnore]
		public ICryptoKeyReader CryptoKeyReader { get; set; } = null;

		public ISet<string> EncryptionKeys { get; set; } = new SortedSet<string>();

		public CompressionType CompressionType { get; set; } = CompressionType.None;
		public long BatchingMaxPublishDelayMicros = TimeUnit.MILLISECONDS.ToMicroseconds(1);

		public long? InitialSequenceId { get; set; }

		public bool AutoUpdatePartitions { get; set; } = true;

		public bool MultiSchema { get; set; } = true;

        public SortedDictionary<string, string> Properties { get; set; }
        
		/// 
		/// <summary>
		/// Returns true if encryption keys are added
		/// 
		/// </summary>
		/// 
		public virtual bool EncryptionEnabled => (EncryptionKeys != null) && EncryptionKeys.Count > 0 && (CryptoKeyReader != null);

		public virtual void SetAutoUpdatePartitionsIntervalSeconds(int interval, TimeUnit timeUnit)
		{
			Condition.CheckArgument(interval > 0, "interval needs to be > 0");
			_autoUpdatePartitionsIntervalSeconds = timeUnit.ToSeconds(interval);
		}
		public long AutoUpdatePartitionsIntervalSeconds => _autoUpdatePartitionsIntervalSeconds;
		public  string ProducerName { get; set; }

		public int MaxPendingMessages
		{
			get => DefaultMaxPendingMessages;
            set
			{
				if(value < 1)
					throw new ArgumentException("maxPendingMessages needs to be > 0");
			}
		}

		public int MaxPendingMessagesAcrossPartitions
		{
			get => DefaultMaxPendingMessagesAcrossPartitions;
            set
			{
				if(value >= MaxPendingMessages)
				 MaxPendingMessagesAcrossPartitions = value;
			}
		}

		public void SetBatchingMaxPublishDelayMicros(long batchDelay, TimeUnit timeUnit)
		{
			long delayInMs = timeUnit.ToMilliseconds(batchDelay);
			Condition.CheckArgument(delayInMs >= 1, "configured value for batch delay must be at least 1ms");
			BatchingMaxPublishDelayMicros = timeUnit.ToMicroseconds(batchDelay);
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
			return _batchingPartitionSwitchFrequencyByPublishDelay * BatchingMaxPublishDelayMicros;
		}

		public void SetSendTimeoutMs(long sendTimeoutMs)
		{
			if (sendTimeoutMs < 1)
				throw new ArgumentException("sendTimeout needs to be >= 0");
			SendTimeoutMs = sendTimeoutMs;
		}

	}

}