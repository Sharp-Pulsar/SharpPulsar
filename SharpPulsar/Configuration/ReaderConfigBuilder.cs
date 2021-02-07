﻿using System;
using System.Collections.Generic;
using SharpPulsar.Batch;
using SharpPulsar.Common;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using Range = SharpPulsar.Common.Range;

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
    public sealed class ReaderConfigBuilder<T>
	{
        private ReaderConfigurationData<T> _conf = new ReaderConfigurationData<T>();

        public ReaderConfigurationData<T> ReaderConfigurationData
        {
            get
            {
				if(_conf.EventListener == null)
					throw new ArgumentException("IConsumerEventListener is not implemented. It cannot be null");
				if (_conf.Schema == null)
                    throw new ArgumentException("Hey, we need the schema!");
				if (_conf.StartMessageId == null)
                    _conf.StartMessageId = IMessageId.Latest;
				if(_conf.ReaderListener == null)
					throw new ArgumentException("Reader Listener Cannot be null");
                return _conf;
            }
        }
        public ReaderConfigBuilder<T> EventListener(IConsumerEventListener consumerEventListener)
        {
            _conf.EventListener = consumerEventListener;
            return this;
        }
		public ReaderConfigBuilder<T> LoadConf(IDictionary<string, object> config)
		{
			var startMessageId = _conf.StartMessageId;
			_conf = (ReaderConfigurationData<T>)ConfigurationDataUtils.LoadData(config, _conf);
			_conf.StartMessageId = startMessageId;
            return this;
        }
        public ReaderConfigBuilder<T> KeyHashRange(params Range[] ranges)
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

		public ReaderConfigBuilder<T> Topic(string topicName)
		{
			_conf.TopicName = topicName.Trim();
            return this;
		}

		public ReaderConfigBuilder<T> StartMessageId(long ledgerId, long entryId, int partitionIndex, int batchIndex)
		{
            _conf.StartMessageId = new BatchMessageId(ledgerId, entryId, partitionIndex, batchIndex);
            return this;
		}
        public ReaderConfigBuilder<T> StartMessageId(IMessageId id)
        {
            _conf.StartMessageId = id;

            return this;
        }

		public ReaderConfigBuilder<T> StartMessageFromRollbackDuration(int rollbackDurationSecs)
		{
			_conf.StartMessageFromRollbackDurationInSec = rollbackDurationSecs;
            return this;
		}

		public ReaderConfigBuilder<T> StartMessageIdInclusive()
		{
			_conf.ResetIncludeHead = true;
            return this;
		}

		public  ReaderConfigBuilder<T> ReaderListener(IReaderListener<T> readerListener)
		{
			_conf.ReaderListener = readerListener;
            return this;
		}

		public ReaderConfigBuilder<T> CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
		{
			_conf.CryptoKeyReader = cryptoKeyReader;
            return this;
		}

		public ReaderConfigBuilder<T> CryptoFailureAction(ConsumerCryptoFailureAction action)
		{
			_conf.CryptoFailureAction = action;
            return this;
		}

		public ReaderConfigBuilder<T> ReceiverQueueSize(int receiverQueueSize)
		{
			_conf.ReceiverQueueSize = receiverQueueSize;
            return this;
		}

		public ReaderConfigBuilder<T> ReaderName(string readerName)
		{
			_conf.ReaderName = readerName;
            return this;
		}

		public  ReaderConfigBuilder<T> SubscriptionRolePrefix(string subscriptionRolePrefix)
		{
			_conf.SubscriptionRolePrefix = subscriptionRolePrefix;
            return this;
		}

		public ReaderConfigBuilder<T> ReadCompacted(bool readCompacted)
		{
			_conf.ReadCompacted = readCompacted;
            return this;
		}
		public ReaderConfigBuilder<T> Schema(ISchema<T> schema)
		{
			if(schema == null)
				throw new ArgumentException("Schema is null");
            _conf.Schema = schema;
            return this;
		}
    }

}