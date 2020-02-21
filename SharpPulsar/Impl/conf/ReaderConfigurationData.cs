using SharpPulsar.Api;
using System;

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
	public class ReaderConfigurationData<T> : ICloneable
	{
		[NonSerialized]
		private string _topicName;
		public IMessageId StartMessageId { get; set; }
		public long StartMessageFromRollbackDurationInSec { get; set; }

		public int ReceiverQueueSize { get; set; } = 1000;

		public IReaderListener<T> ReaderListener { get; set; }
		[NonSerialized]
		private string _readerName;
		[NonSerialized]
		private string _subscriptionRolePrefix;

		public ICryptoKeyReader CryptoKeyReader { get; set; }
		public ConsumerCryptoFailureAction CryptoFailureAction { get; set; } = ConsumerCryptoFailureAction.Fail;

		public bool ReadCompacted { get; set; } = false;
		public bool ResetIncludeHead { get; set; } = false;
		public virtual ReaderConfigurationData<T> Clone()
		{
			try
			{
				return (ReaderConfigurationData<T>) base.MemberwiseClone();
			}
			catch (Exception e)
			{
				throw new Exception("Failed to clone ReaderConfigurationData", e);
			}
		}

        public string SubscriptionRolePrefix
        {
            get => _subscriptionRolePrefix;
            set => _subscriptionRolePrefix = value;
        }

		public string TopicName
        {
            get => _topicName;
            set => _topicName = value;
        }

        public string ReaderName
        {
            get => _readerName;
            set => _readerName = value;
        }
		object ICloneable.Clone()
		{
			return this;
		}
	}

}