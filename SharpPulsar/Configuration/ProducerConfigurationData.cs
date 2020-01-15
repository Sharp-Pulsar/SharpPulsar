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


	//using Data = lombok.Data;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data @NoArgsConstructor @AllArgsConstructor public class ProducerConfigurationData implements java.io.Serializable, Cloneable
	[Serializable]
	public class ProducerConfigurationData : ICloneable
	{

		private const long serialVersionUID = 1L;

		public const int DEFAULT_BATCHING_MAX_MESSAGES = 1000;
		public const int DEFAULT_MAX_PENDING_MESSAGES = 1000;
		public const int DEFAULT_MAX_PENDING_MESSAGES_ACROSS_PARTITIONS = 50000;

		private string topicName = null;
		private string producerName = null;
		private long sendTimeoutMs = 30000;
		private bool blockIfQueueFull = false;
		private int maxPendingMessages = DEFAULT_MAX_PENDING_MESSAGES;
		private int maxPendingMessagesAcrossPartitions = DEFAULT_MAX_PENDING_MESSAGES_ACROSS_PARTITIONS;
		private MessageRoutingMode messageRoutingMode = null;
		private HashingScheme hashingScheme = HashingScheme.JavaStringHash;

		private ProducerCryptoFailureAction cryptoFailureAction = ProducerCryptoFailureAction.FAIL;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private org.apache.pulsar.client.api.MessageRouter customMessageRouter = null;
		private MessageRouter customMessageRouter = null;

		private long batchingMaxPublishDelayMicros = TimeUnit.MILLISECONDS.toMicros(1);
		private int batchingPartitionSwitchFrequencyByPublishDelay = 10;
		private int batchingMaxMessages = DEFAULT_BATCHING_MAX_MESSAGES;
		private int batchingMaxBytes = 128 * 1024; // 128KB (keep the maximum consistent as previous versions)
		private bool batchingEnabled = true; // enabled by default
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private org.apache.pulsar.client.api.BatcherBuilder batcherBuilder = org.apache.pulsar.client.api.BatcherBuilder.DEFAULT;
		private BatcherBuilder batcherBuilder = BatcherBuilder.DEFAULT;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private org.apache.pulsar.client.api.CryptoKeyReader cryptoKeyReader;
		private CryptoKeyReader cryptoKeyReader;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private java.util.Set<String> encryptionKeys = new java.util.TreeSet<>();
		private ISet<string> encryptionKeys = new SortedSet<string>();

		private CompressionType compressionType = CompressionType.NONE;

		// Cannot use Optional<Long> since it's not serializable
		private long? initialSequenceId = null;

		private bool autoUpdatePartitions = true;

		private bool multiSchema = true;

		private SortedDictionary<string, string> properties = new SortedDictionary<string, string>();

		/// 
		/// <summary>
		/// Returns true if encryption keys are added
		/// 
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore public boolean isEncryptionEnabled()
		public virtual bool EncryptionEnabled
		{
			get
			{
				return (this.encryptionKeys != null) && this.encryptionKeys.Count > 0 && (this.cryptoKeyReader != null);
			}
		}

		public virtual ProducerConfigurationData clone()
		{
			try
			{
				ProducerConfigurationData c = (ProducerConfigurationData) base.clone();
				c.encryptionKeys = Sets.newTreeSet(this.encryptionKeys);
				c.properties = Maps.newTreeMap(this.properties);
				return c;
			}
			catch (CloneNotSupportedException e)
			{
				throw new Exception("Failed to clone ProducerConfigurationData", e);
			}
		}

		public virtual string ProducerName
		{
			set
			{
				checkArgument(StringUtils.isNotBlank(value), "producerName cannot be blank");
				this.producerName = value;
			}
		}

		public virtual int MaxPendingMessages
		{
			set
			{
				checkArgument(value > 0, "maxPendingMessages needs to be > 0");
				this.maxPendingMessages = value;
			}
		}

		public virtual int MaxPendingMessagesAcrossPartitions
		{
			set
			{
				checkArgument(value >= maxPendingMessages);
				this.maxPendingMessagesAcrossPartitions = value;
			}
		}

		public virtual int BatchingMaxMessages
		{
			set
			{
				this.batchingMaxMessages = value;
			}
		}

		public virtual int BatchingMaxBytes
		{
			set
			{
				this.batchingMaxBytes = value;
			}
		}

		public virtual void setSendTimeoutMs(int sendTimeout, TimeUnit timeUnit)
		{
			checkArgument(sendTimeout >= 0, "sendTimeout needs to be >= 0");
			this.sendTimeoutMs = timeUnit.toMillis(sendTimeout);
		}

		public virtual void setBatchingMaxPublishDelayMicros(long batchDelay, TimeUnit timeUnit)
		{
			long delayInMs = timeUnit.toMillis(batchDelay);
			checkArgument(delayInMs >= 1, "configured value for batch delay must be at least 1ms");
			this.batchingMaxPublishDelayMicros = timeUnit.toMicros(batchDelay);
		}

		public virtual int BatchingPartitionSwitchFrequencyByPublishDelay
		{
			set
			{
				checkArgument(value >= 1, "configured value for partition switch frequency must be >= 1");
				this.batchingPartitionSwitchFrequencyByPublishDelay = value;
			}
		}

		public virtual long batchingPartitionSwitchFrequencyIntervalMicros()
		{
			return this.batchingPartitionSwitchFrequencyByPublishDelay * batchingMaxPublishDelayMicros;
		}

		public object Clone()
		{
			throw new NotImplementedException();
		}
	}

}