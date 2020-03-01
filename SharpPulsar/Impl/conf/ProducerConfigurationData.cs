using SharpPulsar.Api;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.Handlers;
using SharpPulsar.Api.Interceptor;
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
    public class ProducerConfigurationData 
	{
        public IProducerEventListener ProducerEventListener { get; set; }
		public const int DefaultBatchingMaxMessages  = 1000;
		public const int DefaultMaxPendingMessages = 1000;
		public const int DefaultMaxPendingMessagesAcrossPartitions = 50000;
        public string TopicName { get; set; }
        public int Partitions { get; set; } = 0;
        public ISchema Schema;
        public List<IProducerInterceptor> Interceptors;
        public bool UseTls { get; set; } = false;
		public long SendTimeoutMs { get; set; } = 30000;
		public bool BlockIfQueueFull { get; set; } = false;
        public MessageRoutingMode? MessageRoutingMode { get; set; }
		public HashingScheme HashingScheme { get; set; } = HashingScheme.JavaStringHash;

		public ProducerCryptoFailureAction CryptoFailureAction { get; set; } = ProducerCryptoFailureAction.Fail;
        public IMessageRouter CustomMessageRouter { get; set; } = null;
		public IHandler Handler { get; set; } = new ProducerHandler();
		public long BatchingMaxPublishDelayMicros { get; set; } = BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.ToMicros(1);
		private int _batchingMaxBytes = 128 * 1024; // 128KB (keep the maximum consistent as previous versions)
		public bool BatchingEnabled { get; set; } = true; // enabled by default

        [JsonIgnore]
		public IBatcherBuilder BatcherBuilder { get; set; } = DefaultImplementation.NewDefaultBatcherBuilder();

        [JsonIgnore]
		public ICryptoKeyReader CryptoKeyReader { get; set; } = null;

		public ISet<string> EncryptionKeys { get; set; } = new SortedSet<string>();

		public ICompressionType CompressionType { get; set; } = ICompressionType.None;

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

		public  int BatchingMaxMessages { get; set; }

		public int BatchingMaxBytes
		{
			get => _batchingMaxBytes;
            set => _batchingMaxBytes = value;
        }

		public void SetSendTimeoutMs(int sendTimeout, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			if (sendTimeout < 1)
				throw new ArgumentException("sendTimeout needs to be >= 0");
			SendTimeoutMs = timeUnit.ToMillis(sendTimeout);
		}

		public void SetBatchingMaxPublishDelayMicros(long batchDelay, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			var delayInMs = timeUnit.ToMillis(batchDelay);
			if (delayInMs < 1)
				throw new ArgumentException("configured value for batch delay must be at least 1ms");
			BatchingMaxPublishDelayMicros = delayInMs;
		}

		public int BatchingPartitionSwitchFrequencyByPublishDelay { get; set; }

        public virtual long BatchingPartitionSwitchFrequencyIntervalMicros()
		{
			return BatchingPartitionSwitchFrequencyByPublishDelay * BatchingMaxPublishDelayMicros;
		}

	}

}