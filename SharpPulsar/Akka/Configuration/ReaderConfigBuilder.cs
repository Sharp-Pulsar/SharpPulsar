using System;
using System.Collections.Generic;
using SharpPulsar.Api;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Utility;
using Range = SharpPulsar.Api.Range;

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
namespace SharpPulsar.Akka.Configuration
{
    public sealed class ReaderConfigBuilder
	{
        private ReaderConfigurationData _conf = new ReaderConfigurationData();

        public ReaderConfigurationData ReaderConfigurationData
        {
            get
            {
				if(_conf.EventListener == null)
					throw new ArgumentException("IConsumerEventListener is not implemented. It cannot be null");
				if (_conf.Schema == null)
                    throw new ArgumentException("Hey, we need the schema!");
				if (_conf.StartMessageId == null)
                    _conf.StartMessageId = MessageIdFields.Latest;
				if(_conf.ReaderListener == null)
					throw new ArgumentException("Reader Listener Cannot be null");
                return _conf;
            }
        }
        public ReaderConfigBuilder EventListener(IConsumerEventListener consumerEventListener)
        {
            _conf.EventListener = consumerEventListener;
            return this;
        }
		public ReaderConfigBuilder LoadConf(IDictionary<string, object> config)
		{
			var startMessageId = _conf.StartMessageId;
			_conf = (ReaderConfigurationData)ConfigurationDataUtils.LoadData(config, _conf);
			_conf.StartMessageId = startMessageId;
            return this;
        }
        public ReaderConfigBuilder KeyHashRange(Range[] ranges)
        {
            Precondition.Condition.CheckArgument(ranges != null && ranges.Length > 0, "Cannot specify a null ofr an empty key hash ranges for a reader");
            for (var i = 0; i < ranges.Length; i++)
            {
                var range1 = ranges[i];
                if (range1.Start < 0 || range1.End > KeySharedPolicy.DefaultHashRangeSize)
                {
                    throw new ArgumentException("Ranges must be [0, 65535] but provided range is " + range1);
                }
                for (var j = 0; j < ranges.Length; j++)
                {
                    var range2 = ranges[j];
                    if (i != j && range1.Intersect(range2) != null)
                    {
                        throw new ArgumentException("Key hash ranges with overlap between " + range1 + " and " + range2);
                    }
                }
            }
            _conf.KeyHashRanges = new List<Range>(ranges);
            return this;
        }

		public ReaderConfigBuilder Topic(string topicName)
		{
			_conf.TopicName = topicName.Trim();
            return this;
		}

		public ReaderConfigBuilder StartMessageId(long ledgerId, long entryId, int partitionIndex, int batchIndex)
		{
            _conf.StartMessageId = new BatchMessageId(ledgerId, entryId, partitionIndex, batchIndex, null);
            return this;
		}
        public ReaderConfigBuilder StartMessageId(IMessageId id)
        {
            _conf.StartMessageId = id;

            return this;
        }

		public ReaderConfigBuilder StartMessageFromRollbackDuration(long rollbackDuration, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			_conf.StartMessageFromRollbackDurationInSec = timeUnit.ToSecs(rollbackDuration);
            return this;
		}

		public ReaderConfigBuilder StartMessageIdInclusive()
		{
			_conf.ResetIncludeHead = true;
            return this;
		}

		public  ReaderConfigBuilder ReaderListener(IReaderListener readerListener)
		{
			_conf.ReaderListener = readerListener;
            return this;
		}

		public ReaderConfigBuilder CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
		{
			_conf.CryptoKeyReader = cryptoKeyReader;
            return this;
		}

		public ReaderConfigBuilder CryptoFailureAction(ConsumerCryptoFailureAction action)
		{
			_conf.CryptoFailureAction = action;
            return this;
		}

		public ReaderConfigBuilder ReceiverQueueSize(int receiverQueueSize)
		{
			_conf.ReceiverQueueSize = receiverQueueSize;
            return this;
		}

		public ReaderConfigBuilder ReaderName(string readerName)
		{
			_conf.ReaderName = readerName;
            return this;
		}

		public  ReaderConfigBuilder SubscriptionRolePrefix(string subscriptionRolePrefix)
		{
			_conf.SubscriptionRolePrefix = subscriptionRolePrefix;
            return this;
		}

		public ReaderConfigBuilder ReadCompacted(bool readCompacted)
		{
			_conf.ReadCompacted = readCompacted;
            return this;
		}
		public ReaderConfigBuilder Schema(ISchema schema)
		{
			if(schema == null)
				throw new ArgumentException("Schema is null");
            _conf.Schema = schema;
            return this;
		}
    }

}