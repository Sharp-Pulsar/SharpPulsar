using System;
using System.Collections.Generic;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Utility;

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

        public ReaderConfigurationData ReaderConfigurationData => _conf;
		public void LoadConf(IDictionary<string, object> config)
		{
			var startMessageId = _conf.StartMessageId;
			_conf = ConfigurationDataUtils.LoadData(config, _conf);
			_conf.StartMessageId = startMessageId;
			
		}

		public void Topic(string topicName)
		{
			_conf.TopicName = topicName.Trim();
			
		}

		public void StartMessageId(IMessageId startMessageId)
		{
			_conf.StartMessageId = startMessageId;
			
		}

		public void StartMessageFromRollbackDuration(long rollbackDuration, BAMCIS.Util.Concurrent.TimeUnit timeUnit)
		{
			_conf.StartMessageFromRollbackDurationInSec = timeUnit.ToSecs(rollbackDuration);
			
		}

		public void StartMessageIdInclusive()
		{
			_conf.ResetIncludeHead = true;
			
		}

		public  void ReaderListener(IReaderListener readerListener)
		{
			_conf.ReaderListener = readerListener;
			
		}

		public void CryptoKeyReader(ICryptoKeyReader cryptoKeyReader)
		{
			_conf.CryptoKeyReader = cryptoKeyReader;
			
		}

		public void CryptoFailureAction(ConsumerCryptoFailureAction action)
		{
			_conf.CryptoFailureAction = action;
			
		}

		public void ReceiverQueueSize(int receiverQueueSize)
		{
			_conf.ReceiverQueueSize = receiverQueueSize;
			
		}

		public void ReaderName(string readerName)
		{
			_conf.ReaderName = readerName;
			
		}

		public  void SubscriptionRolePrefix(string subscriptionRolePrefix)
		{
			_conf.SubscriptionRolePrefix = subscriptionRolePrefix;
			
		}

		public void ReadCompacted(bool readCompacted)
		{
			_conf.ReadCompacted = readCompacted;
			
		}
		public void Schema(ISchema schema)
		{
			if(schema == null)
				throw new ArgumentException("Schema is null");
            _conf.Schema = schema;
        }
    }

}