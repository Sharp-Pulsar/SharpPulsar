using SharpPulsar.Enum;
using SharpPulsar.Interface;
using SharpPulsar.Interface.Batch;
using SharpPulsar.Interface.Message;
using System;
using System.Collections.Generic;

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
	public class ProducerConfigurationData : ICloneable
	{

		public const long serialVersionUID = 1L;

		public const int DEFAULT_BATCHING_MAX_MESSAGES = 1000;
		public const int DEFAULT_MAX_PENDING_MESSAGES = 1000;
		public const int DEFAULT_MAX_PENDING_MESSAGES_ACROSS_PARTITIONS = 50000;

		public string TopicName { get; set; }
		private string _producerName;
		public long SendTimeoutMs = 30000;
		public bool BlockIfQueueFull = false;
		private int _maxPendingMessages = DEFAULT_MAX_PENDING_MESSAGES;
		private int _maxPendingMessagesAcrossPartitions = DEFAULT_MAX_PENDING_MESSAGES_ACROSS_PARTITIONS;
		public MessageRoutingMode MessageRoutingMode { get; set; }
		public HashingScheme HashingScheme = HashingScheme.JavaStringHash;

		public ProducerCryptoFailureAction CryptoFailureAction = ProducerCryptoFailureAction.FAIL;
		public IMessageRouter CustomMessageRouter { get; set; }

		private long _batchingMaxPublishDelayMicros = TimeUnit.MILLISECONDS.toMicros(1);
		private int _batchingPartitionSwitchFrequencyByPublishDelay = 10;
		private int _batchingMaxMessages = DEFAULT_BATCHING_MAX_MESSAGES;
		private int _batchingMaxBytes = 128 * 1024; // 128KB (keep the maximum consistent as previous versions)
		public bool BatchingEnabled = true; // enabled by default

		public IBatcherBuilder BatcherBuilder { get; set; }
		public ICryptoKeyReader CryptoKeyReader;
		public ISet<string> EncryptionKeys = new SortedSet<string>();

		public CompressionType CompressionType = CompressionType.NONE;

		// Cannot use Optional<Long> since it's not serializable
		public long? InitialSequenceId { get; set; }

		public bool AutoUpdatePartitions = true;

		public bool MultiSchema = true;

		public SortedDictionary<string, string> Properties = new SortedDictionary<string, string>();

		/// 
		/// <summary>
		/// Returns true if encryption keys are added
		/// 
		/// </summary>
		public virtual bool EncryptionEnabled
		{
			get
			{
				return (this.EncryptionKeys != null) && this.EncryptionKeys.Count > 0 && (this.CryptoKeyReader != null);
			}
		}

		public virtual ProducerConfigurationData Clone()
		{
			try
			{
				ProducerConfigurationData c = (ProducerConfigurationData) base.Clone();
				c.EncryptionKeys = new HashSet<string>(this.EncryptionKeys);
				c.Properties = new SortedDictionary<string, string>(this.Properties);
				return c;
			}
			catch (CloneNotSupportedException e)
			{
				throw new System.Exception("Failed to clone ProducerConfigurationData", e);
			}
		}

		public virtual string ProducerName
		{
			set
			{
				checkArgument(StringUtils.isNotBlank(value), "producerName cannot be blank");
				this._producerName = value;
			}
		}

		public virtual int MaxPendingMessages
		{
			set
			{
				checkArgument(value > 0, "maxPendingMessages needs to be > 0");
				this._maxPendingMessages = value;
			}
		}

		public virtual int MaxPendingMessagesAcrossPartitions
		{
			set
			{
				checkArgument(value >= maxPendingMessages);
				this._maxPendingMessagesAcrossPartitions = value;
			}
		}

		public virtual int BatchingMaxMessages
		{
			set
			{
				this._batchingMaxMessages = value;
			}
		}

		public virtual int BatchingMaxBytes
		{
			set
			{
				this._batchingMaxBytes = value;
			}
		}

		public virtual void SetSendTimeoutMs(int sendTimeout, TimeUnit timeUnit)
		{
			checkArgument(sendTimeout >= 0, "sendTimeout needs to be >= 0");
			this.SendTimeoutMs = timeUnit.toMillis(sendTimeout);
		}

		public virtual void SetBatchingMaxPublishDelayMicros(long batchDelay, TimeUnit timeUnit)
		{
			long delayInMs = timeUnit.toMillis(batchDelay);
			checkArgument(delayInMs >= 1, "configured value for batch delay must be at least 1ms");
			this._batchingMaxPublishDelayMicros = timeUnit.toMicros(batchDelay);
		}

		public virtual int BatchingPartitionSwitchFrequencyByPublishDelay
		{
			set
			{
				checkArgument(value >= 1, "configured value for partition switch frequency must be >= 1");
				this._batchingPartitionSwitchFrequencyByPublishDelay = value;
			}
		}

		public virtual long BatchingPartitionSwitchFrequencyIntervalMicros()
		{
			return this._batchingPartitionSwitchFrequencyByPublishDelay * _batchingMaxPublishDelayMicros;
		}

		object ICloneable.Clone()
		{
			throw new NotImplementedException();
		}
	}

}