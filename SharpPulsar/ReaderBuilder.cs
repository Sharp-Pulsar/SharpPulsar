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
namespace SharpPulsar
{
	public class ReaderBuilder<T> : IReaderBuilder<T>
	{

		private readonly PulsarClientImpl _client;

		private ReaderConfigurationData<T> _conf;

		private readonly Schema<T> _schema;

		public ReaderBuilder(PulsarClientImpl client, Schema<T> schema) : this(client, new ReaderConfigurationData<T>(), schema)
		{
		}

		private ReaderBuilder(PulsarClientImpl client, ReaderConfigurationData<T> conf, Schema<T> schema)
		{
			this._client = client;
			this._conf = conf;
			this._schema = schema;
		}

		public virtual ReaderBuilder<T> Clone()
		{
			return new ReaderBuilderImpl<T>(_client, _conf.Clone(), _schema);
		}

		public virtual ReaderActor<T> Create()
		{
			try
			{
				return CreateAsync().get();
			}
			catch(Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public virtual CompletableFuture<ReaderActor<T>> CreateAsync()
		{
			if(_conf.TopicName == null)
			{
				return FutureUtil.FailedFuture(new System.ArgumentException("Topic name must be set on the reader builder"));
			}

			if(_conf.StartMessageId != null && _conf.StartMessageFromRollbackDurationInSec > 0 || _conf.StartMessageId == null && _conf.StartMessageFromRollbackDurationInSec <= 0)
			{
				return FutureUtil.FailedFuture(new System.ArgumentException("Start message id or start message from roll back must be specified but they cannot be specified at the same time"));
			}

			if(_conf.StartMessageFromRollbackDurationInSec > 0)
			{
				_conf.StartMessageId = MessageId.earliest;
			}

			return _client.CreateReaderAsync(_conf, _schema);
		}

		public virtual ReaderBuilder<T> LoadConf(IDictionary<string, object> config)
		{
			MessageId startMessageId = _conf.StartMessageId;
			_conf = ConfigurationDataUtils.LoadData(config, _conf, typeof(ReaderConfigurationData));
			_conf.StartMessageId = startMessageId;
			return this;
		}

		public virtual ReaderBuilder<T> Topic(string topicName)
		{
			_conf.TopicName = StringUtils.Trim(topicName);
			return this;
		}

		public virtual ReaderBuilder<T> StartMessageId(MessageId startMessageId)
		{
			_conf.StartMessageId = startMessageId;
			return this;
		}

		public virtual ReaderBuilder<T> StartMessageFromRollbackDuration(long rollbackDuration, TimeUnit timeunit)
		{
			_conf.StartMessageFromRollbackDurationInSec = timeunit.toSeconds(rollbackDuration);
			return this;
		}

		public virtual ReaderBuilder<T> StartMessageIdInclusive()
		{
			_conf.ResetIncludeHead = true;
			return this;
		}

		public virtual ReaderBuilder<T> ReaderListener(ReaderListener<T> readerListener)
		{
			_conf.ReaderListener = readerListener;
			return this;
		}

		public virtual ReaderBuilder<T> CryptoKeyReader(CryptoKeyReader cryptoKeyReader)
		{
			_conf.CryptoKeyReader = cryptoKeyReader;
			return this;
		}

		public virtual ReaderBuilder<T> CryptoFailureAction(ConsumerCryptoFailureAction action)
		{
			_conf.CryptoFailureAction = action;
			return this;
		}

		public virtual ReaderBuilder<T> ReceiverQueueSize(int receiverQueueSize)
		{
			_conf.ReceiverQueueSize = receiverQueueSize;
			return this;
		}

		public virtual ReaderBuilder<T> ReaderName(string readerName)
		{
			_conf.ReaderName = readerName;
			return this;
		}

		public virtual ReaderBuilder<T> SubscriptionRolePrefix(string subscriptionRolePrefix)
		{
			_conf.SubscriptionRolePrefix = subscriptionRolePrefix;
			return this;
		}

		public virtual ReaderBuilder<T> ReadCompacted(bool readCompacted)
		{
			_conf.ReadCompacted = readCompacted;
			return this;
		}

		public virtual ReaderBuilder<T> KeyHashRange(params Range[] ranges)
		{
			Preconditions.checkArgument(ranges != null && ranges.Length > 0, "Cannot specify a null ofr an empty key hash ranges for a reader");
			for(int i = 0; i < ranges.Length; i++)
			{
				Range range1 = ranges[i];
				if(range1.Start < 0 || range1.End > DEFAULT_HASH_RANGE_SIZE)
				{
					throw new System.ArgumentException("Ranges must be [0, 65535] but provided range is " + range1);
				}
				for(int j = 0; j < ranges.Length; j++)
				{
					Range range2 = ranges[j];
					if(i != j && range1.Intersect(range2) != null)
					{
						throw new System.ArgumentException("Key hash ranges with overlap between " + range1 + " and " + range2);
					}
				}
			}
			_conf.KeyHashRanges = Arrays.asList(ranges);
			return this;
		}
	}

}