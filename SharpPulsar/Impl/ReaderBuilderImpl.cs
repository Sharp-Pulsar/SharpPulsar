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
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using ConsumerCryptoFailureAction = SharpPulsar.Api.ConsumerCryptoFailureAction;
	using CryptoKeyReader = SharpPulsar.Api.CryptoKeyReader;
	using IMessageId = SharpPulsar.Api.IMessageId;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using Reader = SharpPulsar.Api.IReader;
	using SharpPulsar.Api;
	using SharpPulsar.Api;
	using SharpPulsar.Api;
	using ConfigurationDataUtils = SharpPulsar.Impl.Conf.ConfigurationDataUtils;
	using SharpPulsar.Impl.Conf;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter(AccessLevel.PUBLIC) public class ReaderBuilderImpl<T> implements SharpPulsar.api.ReaderBuilder<T>
	public class ReaderBuilderImpl<T> : ReaderBuilder<T>
	{

		private readonly PulsarClientImpl client;

		private ReaderConfigurationData<T> conf;

		private readonly ISchema<T> schema;

		public ReaderBuilderImpl(PulsarClientImpl Client, ISchema<T> Schema) : this(Client, new ReaderConfigurationData<T>(), Schema)
		{
		}

		private ReaderBuilderImpl(PulsarClientImpl Client, ReaderConfigurationData<T> Conf, ISchema<T> Schema)
		{
			this.client = Client;
			this.conf = Conf;
			this.schema = Schema;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public SharpPulsar.api.ReaderBuilder<T> clone()
		public override ReaderBuilder<T> Clone()
		{
			return new ReaderBuilderImpl<T>(client, conf.Clone(), schema);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.Reader<T> create() throws SharpPulsar.api.PulsarClientException
		public override IReader<T> Create()
		{
			try
			{
				return CreateAsync().get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

		public override CompletableFuture<IReader<T>> CreateAsync()
		{
			if (conf.TopicName == null)
			{
				return FutureUtil.failedFuture(new System.ArgumentException("Topic name must be set on the reader builder"));
			}

			if (conf.StartMessageId == null)
			{
				return FutureUtil.failedFuture(new System.ArgumentException("Start message id must be set on the reader builder"));
			}

			return client.CreateReaderAsync(conf, schema);
		}

		public override ReaderBuilder<T> LoadConf(IDictionary<string, object> Config)
		{
			IMessageId StartMessageId = conf.StartMessageId;
			conf = ConfigurationDataUtils.loadData(Config, conf, typeof(ReaderConfigurationData));
			conf.StartMessageId = StartMessageId;
			return this;
		}

		public override ReaderBuilder<T> Topic(string TopicName)
		{
			conf.TopicName = StringUtils.Trim(TopicName);
			return this;
		}

		public override ReaderBuilder<T> StartMessageId(IMessageId StartMessageId)
		{
			conf.StartMessageId = StartMessageId;
			return this;
		}

		public override ReaderBuilder<T> StartMessageFromRollbackDuration(long RollbackDuration, BAMCIS.Util.Concurrent.TimeUnit BAMCIS.Util.Concurrent.TimeUnit)
		{
			conf.StartMessageFromRollbackDurationInSec = BAMCIS.Util.Concurrent.TimeUnit.toSeconds(RollbackDuration);
			return this;
		}

		public override ReaderBuilder<T> StartMessageIdInclusive()
		{
			conf.ResetIncludeHead = true;
			return this;
		}

		public override ReaderBuilder<T> ReaderListener(ReaderListener<T> ReaderListener)
		{
			conf.ReaderListener = ReaderListener;
			return this;
		}

		public override ReaderBuilder<T> CryptoKeyReader(CryptoKeyReader CryptoKeyReader)
		{
			conf.CryptoKeyReader = CryptoKeyReader;
			return this;
		}

		public override ReaderBuilder<T> CryptoFailureAction(ConsumerCryptoFailureAction Action)
		{
			conf.CryptoFailureAction = Action;
			return this;
		}

		public override ReaderBuilder<T> ReceiverQueueSize(int ReceiverQueueSize)
		{
			conf.ReceiverQueueSize = ReceiverQueueSize;
			return this;
		}

		public override ReaderBuilder<T> ReaderName(string ReaderName)
		{
			conf.ReaderName = ReaderName;
			return this;
		}

		public override ReaderBuilder<T> SubscriptionRolePrefix(string SubscriptionRolePrefix)
		{
			conf.SubscriptionRolePrefix = SubscriptionRolePrefix;
			return this;
		}

		public override ReaderBuilder<T> ReadCompacted(bool ReadCompacted)
		{
			conf.ReadCompacted = ReadCompacted;
			return this;
		}
	}

}