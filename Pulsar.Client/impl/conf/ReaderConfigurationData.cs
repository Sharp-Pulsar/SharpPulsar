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
namespace org.apache.pulsar.client.impl.conf
{
	using JsonIgnore = com.fasterxml.jackson.annotation.JsonIgnore;

	using ConsumerCryptoFailureAction = org.apache.pulsar.client.api.ConsumerCryptoFailureAction;
	using CryptoKeyReader = org.apache.pulsar.client.api.CryptoKeyReader;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using Range = org.apache.pulsar.client.api.Range;
	using ReaderListener = org.apache.pulsar.client.api.ReaderListener;

	using Data = lombok.Data;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data public class ReaderConfigurationData<T> implements java.io.Serializable, Cloneable
	[Serializable]
	public class ReaderConfigurationData<T> : ICloneable
	{

		private string topicName;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private org.apache.pulsar.client.api.MessageId startMessageId;
		private MessageId startMessageId;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private long startMessageFromRollbackDurationInSec;
		private long startMessageFromRollbackDurationInSec;

		private int receiverQueueSize = 1000;

		private ReaderListener<T> readerListener;

		private string readerName = null;
		private string subscriptionRolePrefix = null;

		private CryptoKeyReader cryptoKeyReader = null;
		private ConsumerCryptoFailureAction cryptoFailureAction = ConsumerCryptoFailureAction.FAIL;

		private bool readCompacted = false;
		private bool resetIncludeHead = false;

		private IList<Range> keyHashRanges;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public ReaderConfigurationData<T> clone()
		public virtual ReaderConfigurationData<T> clone()
		{
			try
			{
				return (ReaderConfigurationData<T>) base.clone();
			}
			catch (CloneNotSupportedException)
			{
				throw new Exception("Failed to clone ReaderConfigurationData");
			}
		}
	}

}