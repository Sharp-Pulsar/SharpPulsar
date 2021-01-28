using System.Collections.Generic;
using SharpPulsar.Common;
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
        public IList<Range> KeyHashRanges { get; set; }
		public IMessageId StartMessageId { get; set; }
		public IConsumerEventListener EventListener { get; set; }
		public long StartMessageFromRollbackDurationInSec { get; set; }
        public ISchema<T> Schema { get; set; }
		public int ReceiverQueueSize { get; set; } = 1000;

		public IReaderListener<T> ReaderListener { get; set; }

		public ICryptoKeyReader CryptoKeyReader { get; set; }
		public ConsumerCryptoFailureAction CryptoFailureAction { get; set; } = ConsumerCryptoFailureAction.Fail;

		public bool ReadCompacted { get; set; } = false;
		public bool ResetIncludeHead { get; set; } = true;
		
        public string SubscriptionRolePrefix { get; set; }

		public string TopicName { get; set; }

        public string ReaderName { get; set; }
		
		
	}

}