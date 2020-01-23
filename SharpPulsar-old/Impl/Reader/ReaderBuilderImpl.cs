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
namespace SharpPulsar.Impl
{

	using AccessLevel = lombok.AccessLevel;
	using Getter = lombok.Getter;
	using Preconditions = com.google.common.@base.Preconditions;
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using ConsumerCryptoFailureAction = org.apache.pulsar.client.api.ConsumerCryptoFailureAction;
	using CryptoKeyReader = org.apache.pulsar.client.api.CryptoKeyReader;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using Range = org.apache.pulsar.client.api.Range;
	using Reader = org.apache.pulsar.client.api.Reader;
	using ReaderBuilder = org.apache.pulsar.client.api.ReaderBuilder;
	using ReaderListener = org.apache.pulsar.client.api.ReaderListener;
	using Schema = org.apache.pulsar.client.api.Schema;
	using ConfigurationDataUtils = SharpPulsar.Impl.conf.ConfigurationDataUtils;
	using SharpPulsar.Impl.conf;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.api.KeySharedPolicy.DEFAULT_HASH_RANGE_SIZE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter(AccessLevel.PUBLIC) public class ReaderBuilderImpl<T> implements org.apache.pulsar.client.api.ReaderBuilder<T>
	public class ReaderBuilderImpl<T> : ReaderBuilder<T>
	{

		private readonly PulsarClientImpl client;

		private ReaderConfigurationData<T> conf;

		private readonly Schema<T> schema;

		public ReaderBuilderImpl(PulsarClientImpl client, Schema<T> schema) : this(client, new ReaderConfigurationData<T>(), schema)
		{
		}

		private ReaderBuilderImpl(PulsarClientImpl client, ReaderConfigurationData<T> conf, Schema<T> schema)
		{
			this.client = client;
			this.conf = conf;
			this.schema = schema;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public org.apache.pulsar.client.api.ReaderBuilder<T> clone()
		public override ReaderBuilder<T> clone()
		{
			return new ReaderBuilderImpl<T>(client, conf.clone(), schema);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.Reader<T> create() throws org.apache.pulsar.client.api.PulsarClientException
		public override Reader<T> create()
		{
			try
			{
				return createAsync().get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

		public override CompletableFuture<Reader<T>> createAsync()
		{
			if (conf.TopicName == null)
			{
				return FutureUtil.failedFuture(new System.ArgumentException("Topic name must be set on the reader builder"));
			}

			if (conf.StartMessageId == null)
			{
				return FutureUtil.failedFuture(new System.ArgumentException("Start message id must be set on the reader builder"));
			}

			return client.createReaderAsync(conf, schema);
		}

		public override ReaderBuilder<T> loadConf(IDictionary<string, object> config)
		{
			MessageId startMessageId = conf.StartMessageId;
			conf = ConfigurationDataUtils.loadData(config, conf, typeof(ReaderConfigurationData));
			conf.StartMessageId = startMessageId;
			return this;
		}

		public override ReaderBuilder<T> topic(string topicName)
		{
			conf.TopicName = StringUtils.Trim(topicName);
			return this;
		}

		public override ReaderBuilder<T> startMessageId(MessageId startMessageId)
		{
			conf.StartMessageId = startMessageId;
			return this;
		}

		public override ReaderBuilder<T> startMessageFromRollbackDuration(long rollbackDuration, TimeUnit timeunit)
		{
			conf.StartMessageFromRollbackDurationInSec = timeunit.toSeconds(rollbackDuration);
			return this;
		}

		public override ReaderBuilder<T> startMessageIdInclusive()
		{
			conf.ResetIncludeHead = true;
			return this;
		}

		public override ReaderBuilder<T> readerListener(ReaderListener<T> readerListener)
		{
			conf.ReaderListener = readerListener;
			return this;
		}

		public override ReaderBuilder<T> cryptoKeyReader(CryptoKeyReader cryptoKeyReader)
		{
			conf.CryptoKeyReader = cryptoKeyReader;
			return this;
		}

		public override ReaderBuilder<T> cryptoFailureAction(ConsumerCryptoFailureAction action)
		{
			conf.CryptoFailureAction = action;
			return this;
		}

		public override ReaderBuilder<T> receiverQueueSize(int receiverQueueSize)
		{
			conf.ReceiverQueueSize = receiverQueueSize;
			return this;
		}

		public override ReaderBuilder<T> readerName(string readerName)
		{
			conf.ReaderName = readerName;
			return this;
		}

		public override ReaderBuilder<T> subscriptionRolePrefix(string subscriptionRolePrefix)
		{
			conf.SubscriptionRolePrefix = subscriptionRolePrefix;
			return this;
		}

		public override ReaderBuilder<T> readCompacted(bool readCompacted)
		{
			conf.ReadCompacted = readCompacted;
			return this;
		}

		public override ReaderBuilder<T> keyHashRange(params Range[] ranges)
		{
			Preconditions.checkArgument(ranges != null && ranges.Length > 0, "Cannot specify a null ofr an empty key hash ranges for a reader");
			for (int i = 0; i < ranges.Length; i++)
			{
				Range range1 = ranges[i];
				if (range1.Start < 0 || range1.End > DEFAULT_HASH_RANGE_SIZE)
				{
					throw new System.ArgumentException("Ranges must be [0, 65535] but provided range is " + range1);
				}
				for (int j = 0; j < ranges.Length; j++)
				{
					Range range2 = ranges[j];
					if (i != j && range1.intersect(range2) != null)
					{
						throw new System.ArgumentException("Key hash ranges with overlap between " + range1 + " and " + range2);
					}
				}
			}
			conf.KeyHashRanges = Arrays.asList(ranges);
			return this;
		}
	}

}