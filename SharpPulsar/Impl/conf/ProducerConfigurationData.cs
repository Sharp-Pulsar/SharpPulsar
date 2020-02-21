using SharpPulsar.Api;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
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
	[Serializable]
	public class ProducerConfigurationData : ICloneable
	{

		private const long SerialVersionUid = 1L;

		public const int DefaultBatchingMaxMessages  = 1000;
		public const int DefaultMaxPendingMessages = 1000;
		public const int DefaultMaxPendingMessagesAcrossPartitions = 50000;

		[NonSerialized]
		private string _topicName;

        public string TopicName
        {
            get => _topicName;
            set => _topicName = value;
        }
		[NonSerialized]
		private string _producerName;
		public long SendTimeoutMs { get; set; } = 30000;
		public bool BlockIfQueueFull { get; set; } = false;
		private int _maxPendingMessages = DefaultMaxPendingMessages;
		private int _maxPendingMessagesAcrossPartitions = DefaultMaxPendingMessagesAcrossPartitions;
        [NonSerialized]
		private MessageRoutingMode? _messageRoutingMode;

        public MessageRoutingMode? MessageRoutingMode
        {
            get => _messageRoutingMode;
            set => _messageRoutingMode = value;
        }
		public HashingScheme HashingScheme { get; set; } = HashingScheme.JavaStringHash;

		public ProducerCryptoFailureAction CryptoFailureAction { get; set; } = ProducerCryptoFailureAction.Fail;
        public IMessageRouter CustomMessageRouter { get; set; } = null;

		public long BatchingMaxPublishDelayMicros { get; set; } = BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.ToMicros(1);
		private int _batchingPartitionSwitchFrequencyByPublishDelay = 10;
		private int _batchingMaxMessages = DefaultBatchingMaxMessages;
		private int _batchingMaxBytes = 128 * 1024; // 128KB (keep the maximum consistent as previous versions)
		public bool BatchingEnabled { get; set; } = true; // enabled by default

        [JsonIgnore]
		public IBatcherBuilder BatcherBuilder { get; set; } = DefaultImplementation.NewDefaultBatcherBuilder();

        [JsonIgnore]
		public ICryptoKeyReader CryptoKeyReader { get; set; } = null;

		public ISet<string> EncryptionKeys { get; set; } = new SortedSet<string>();

		public ICompressionType CompressionType { get; set; } = ICompressionType.None;

		[NonSerialized]
		private long? _initialSequenceId;

        public long? InitialSequenceId
        {
            get => _initialSequenceId;
            set => _initialSequenceId = value;
        }

		public bool AutoUpdatePartitions { get; set; } = true;

		public bool MultiSchema { get; set; } = true;

		[NonSerialized]
		private SortedDictionary<string, string> _properties = new SortedDictionary<string, string>();

        public SortedDictionary<string, string> Properties
        {
            get => _properties;
            set => _properties = value;
        }
		/// 
		/// <summary>
		/// Returns true if encryption keys are added
		/// 
		/// </summary>
		/// 
		public virtual bool EncryptionEnabled => (EncryptionKeys != null) && EncryptionKeys.Count > 0 && (CryptoKeyReader != null);

        public virtual ProducerConfigurationData Clone()
		{
			try
			{
				var c = (ProducerConfigurationData) base.MemberwiseClone();
				c.EncryptionKeys = new SortedSet<string>(EncryptionKeys);
				c.Properties = new SortedDictionary<string, string>(Properties);
				return c;
			}
			catch (System.Exception e)
			{
				throw new System.Exception("Failed to clone ProducerConfigurationData", e);
			}
		}

		public virtual string ProducerName
		{
			get => _producerName;
            set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentException("producerName cannot be blank or null");
				_producerName = value;
			}
		}

		public virtual int MaxPendingMessages
		{
			get => _maxPendingMessages;
            set
			{
				if(value < 1)
					throw new ArgumentException("maxPendingMessages needs to be > 0");
				_maxPendingMessages = value;
			}
		}

		public virtual int MaxPendingMessagesAcrossPartitions
		{
			get => _maxPendingMessagesAcrossPartitions;
            set
			{
				if(value >= _maxPendingMessages)
				 _maxPendingMessagesAcrossPartitions = value;
			}
		}

		public virtual int BatchingMaxMessages
		{
			get => _batchingMaxMessages;
            set => _batchingMaxMessages = value;
        }

		public virtual int BatchingMaxBytes
		{
			get => _batchingMaxBytes;
            set => _batchingMaxBytes = value;
        }

		public virtual void SetSendTimeoutMs(int sendTimeout, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			if (sendTimeout < 1)
				throw new ArgumentException("sendTimeout needs to be >= 0");
			SendTimeoutMs = timeUnit.ToMillis(sendTimeout);
		}

		public virtual void SetBatchingMaxPublishDelayMicros(long batchDelay, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			var delayInMs = timeUnit.ToMillis(batchDelay);
			if (delayInMs < 1)
				throw new ArgumentException("configured value for batch delay must be at least 1ms");
			BatchingMaxPublishDelayMicros = delayInMs;
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