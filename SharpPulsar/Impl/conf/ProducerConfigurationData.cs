using SharpPulsar.Api;
using SharpPulsar.Util;
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
namespace SharpPulsar.Impl.Conf
{
	[Serializable]
	public class ProducerConfigurationData : ICloneable
	{

		private const long SerialVersionUID = 1L;

		public const int DefaultBatchingMaxMessages = 1000;
		public const int DefaultMaxPendingMessages = 1000;
		public const int DefaultMaxPendingMessagesAcrossPartitions = 50000;

		[NonSerialized]
		public string TopicName = null;
		[NonSerialized]
		private string _producerName = null;
		public long SendTimeoutMs = 30000;
		public bool BlockIfQueueFull = false;
		private int _maxPendingMessages = DefaultMaxPendingMessages;
		private int _maxPendingMessagesAcrossPartitions = DefaultMaxPendingMessagesAcrossPartitions;
		public MessageRoutingMode MessageRoutingMode;
		public HashingScheme HashingScheme = HashingScheme.JavaStringHash;

		public ProducerCryptoFailureAction CryptoFailureAction = ProducerCryptoFailureAction.Fail;
		public IMessageRouter CustomMessageRouter = null;

		public long BatchingMaxPublishDelayMicros = BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.ToMicros(1);
		private int _batchingPartitionSwitchFrequencyByPublishDelay = 10;
		private int _batchingMaxMessages = DefaultBatchingMaxMessages;
		private int _batchingMaxBytes = 128 * 1024; // 128KB (keep the maximum consistent as previous versions)
		public bool BatchingEnabled = true; // enabled by default

		public BatcherBuilder BatcherBuilder = BatcherBuilderFields.Default;

		public CryptoKeyReader CryptoKeyReader;

		public ISet<string> EncryptionKeys = new SortedSet<string>();

		public ICompressionType CompressionType = ICompressionType.None;

		[NonSerialized]
		public long?InitialSequenceId = null;

		public bool AutoUpdatePartitions = true;

		public bool MultiSchema = true;

		[NonSerialized]
		public SortedDictionary<string, string> Properties = new SortedDictionary<string, string>();

		/// 
		/// <summary>
		/// Returns true if encryption keys are added
		/// 
		/// </summary>
		/// 
		public virtual bool EncryptionEnabled
		{
			get
			{
				return (EncryptionKeys != null) && EncryptionKeys.Count > 0 && (CryptoKeyReader != null);
			}
		}

		public virtual ProducerConfigurationData Clone()
		{
			try
			{
				var C = (ProducerConfigurationData) base.MemberwiseClone();
				C.EncryptionKeys = new SortedSet<string>(EncryptionKeys);
				C.Properties = new SortedDictionary<string, string>(Properties);
				return C;
			}
			catch (System.Exception e)
			{
				throw new System.Exception("Failed to clone ProducerConfigurationData", e);
			}
		}

		public virtual string ProducerName
		{
			get
			{
				return _producerName;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new System.Exception("producerName cannot be blank");
				_producerName = value;
			}
		}

		public virtual int MaxPendingMessages
		{
			get
			{
				return _maxPendingMessages;
			}
			set
			{
				if(value < 1)
					throw new System.Exception("maxPendingMessages needs to be > 0");
				_maxPendingMessages = value;
			}
		}

		public virtual int MaxPendingMessagesAcrossPartitions
		{
			get
			{
				return _maxPendingMessagesAcrossPartitions;
			}
			set
			{
				if(value >= _maxPendingMessages)
				 _maxPendingMessagesAcrossPartitions = value;
			}
		}

		public virtual int BatchingMaxMessages
		{
			get
			{
				return _batchingMaxMessages;
			}
			set
			{
				_batchingMaxMessages = value;
			}
		}

		public virtual int BatchingMaxBytes
		{
			get
			{
				return _batchingMaxBytes;
			}
			set
			{
				_batchingMaxBytes = value;
			}
		}

		public virtual void SetSendTimeoutMs(int SendTimeout, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			if (SendTimeout < 1)
				throw new System.Exception("sendTimeout needs to be >= 0");
			_sendTimeoutMs = timeUnit.ToMillis(SendTimeout);
		}

		public virtual void SetBatchingMaxPublishDelayMicros(long BatchDelay, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			var DelayInMs = timeUnit.ToMillis(BatchDelay);
			if (DelayInMs < 1)
				throw new System.Exception("configured value for batch delay must be at least 1ms");
			BatchingMaxPublishDelayMicros = DelayInMs;
		}

		public virtual int BatchingPartitionSwitchFrequencyByPublishDelay
		{
			set
			{
				if (value < 1)
					throw new System.Exception("configured value for partition switch frequency must be >= 1");
				_batchingPartitionSwitchFrequencyByPublishDelay = value;
			}
		}

		public virtual long BatchingPartitionSwitchFrequencyIntervalMicros()
		{
			return _batchingPartitionSwitchFrequencyByPublishDelay * BatchingMaxPublishDelayMicros;
		}

		object ICloneable.Clone()
		{
			throw new NotImplementedException();
		}
	}

}