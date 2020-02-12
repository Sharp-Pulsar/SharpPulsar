using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpPulsar.Api;
using SharpPulsar.Exception;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Util;

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
    public class ReaderBuilderImpl<T> : IReaderBuilder<T>
	{

		private readonly PulsarClientImpl _client;

		private ReaderConfigurationData<T> _conf;

		private readonly ISchema<T> _schema;

		public ReaderBuilderImpl(PulsarClientImpl client, ISchema<T> schema) : this(client, new ReaderConfigurationData<T>(), schema)
		{
		}

		private ReaderBuilderImpl(PulsarClientImpl client, ReaderConfigurationData<T> conf, ISchema<T> schema)
		{
			this._client = client;
			this._conf = conf;
			this._schema = schema;
		}

		public IReaderBuilder<T> Clone()
		{
			return new ReaderBuilderImpl<T>(_client, _conf.Clone(), _schema);
		}

		public IReader<T> Create()
		{
			try
			{
				return CreateAsync().Result;
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public ValueTask<IReader<T>> CreateAsync()
		{
			if (_conf.TopicName == null)
			{
				return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(new ArgumentException("Topic name must be set on the reader builder")));
			}

			if (_conf.StartMessageId == null)
			{
                return new ValueTask<IReader<T>>(Task.FromException<IReader<T>>(new ArgumentException("Start message id must be set on the reader builder")));
			}

			return _client.CreateReaderAsync(_conf, _schema);
		}

		public IReaderBuilder<T> LoadConf(IDictionary<string, object> config)
		{
			var startMessageId = _conf.StartMessageId;
			_conf = ConfigurationDataUtils.LoadData(config, _conf, typeof(ReaderConfigurationData<T>));
			_conf.StartMessageId = startMessageId;
			return this;
		}

		public IReaderBuilder<T> Topic(string topicName)
		{
			_conf.TopicName = topicName.Trim();
			return this;
		}

		public IReaderBuilder<T> StartMessageId(IMessageId startMessageId)
		{
			_conf.StartMessageId = startMessageId;
			return this;
		}

		public IReaderBuilder<T> StartMessageFromRollbackDuration(long rollbackDuration, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			_conf.StartMessageFromRollbackDurationInSec = timeUnit.ToSecs(rollbackDuration);
			return this;
		}

		public IReaderBuilder<T> StartMessageIdInclusive()
		{
			_conf.ResetIncludeHead = true;
			return this;
		}

		public  IReaderBuilder<T> ReaderListener(IReaderListener<T> readerListener)
		{
			_conf.ReaderListener = readerListener;
			return this;
		}

		public IReaderBuilder<T> CryptoKeyReader(CryptoKeyReader cryptoKeyReader)
		{
			_conf.CryptoKeyReader = cryptoKeyReader;
			return this;
		}

		public IReaderBuilder<T> CryptoFailureAction(ConsumerCryptoFailureAction action)
		{
			_conf.CryptoFailureAction = action;
			return this;
		}

		public IReaderBuilder<T> ReceiverQueueSize(int receiverQueueSize)
		{
			_conf.ReceiverQueueSize = receiverQueueSize;
			return this;
		}

		public IReaderBuilder<T> ReaderName(string readerName)
		{
			_conf.ReaderName = readerName;
			return this;
		}

		public  IReaderBuilder<T> SubscriptionRolePrefix(string subscriptionRolePrefix)
		{
			_conf.SubscriptionRolePrefix = subscriptionRolePrefix;
			return this;
		}

		public IReaderBuilder<T> ReadCompacted(bool readCompacted)
		{
			_conf.ReadCompacted = readCompacted;
			return this;
		}

        object ICloneable.Clone()
        {
            return Clone();
        }
    }

}