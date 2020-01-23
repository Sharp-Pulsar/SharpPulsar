using Pulsar.Api;
using SharpPulsar.Enum;
using SharpPulsar.Interface;
using SharpPulsar.Interface.Message;
using SharpPulsar.Interface.Reader;
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
	public class ReaderConfigurationData<T> : ICloneable
	{

		private string topicName;
		private IMessageId startMessageId;
		private long startMessageFromRollbackDurationInSec;

		private int receiverQueueSize = 1000;

		private IReaderListener<T> readerListener;

		private string readerName = null;
		private string subscriptionRolePrefix = null;

		private ICryptoKeyReader cryptoKeyReader = null;
		private ConsumerCryptoFailureAction cryptoFailureAction = ConsumerCryptoFailureAction.FAIL;

		private bool readCompacted = false;
		private bool resetIncludeHead = false;

		private IList<Range> keyHashRanges;
		public virtual ReaderConfigurationData<T> clone()
		{
			try
			{
				return (ReaderConfigurationData<T>) base.Clone();
			}
			catch (CloneNotSupportedException)
			{
				throw new Exception("Failed to clone ReaderConfigurationData");
			}
		}

		public object Clone()
		{
			throw new NotImplementedException();
		}
	}

}