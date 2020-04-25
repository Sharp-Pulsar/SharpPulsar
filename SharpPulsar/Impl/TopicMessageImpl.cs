using System.Collections.Generic;
using SharpPulsar.Impl.Auth;

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
    using Api;

	public class TopicMessageImpl : IMessage
	{

		/// <summary>
		/// This topicPartitionName is get from ConsumerImpl, it contains partition part. </summary>
		public string TopicPartitionName;

		private readonly IMessage _msg;
		private  TopicMessageIdImpl _messageId;

		public TopicMessageImpl(string topicPartitionName, string topicName, IMessage msg)
		{
			TopicPartitionName = topicPartitionName;

			_msg = msg;
			_messageId = new TopicMessageIdImpl(topicPartitionName, topicName, msg.MessageId);
		}

		/// <summary>
		/// Get the topic name without partition part of this message. </summary>
		/// <returns> the name of the topic on which this message was published </returns>
		public virtual string TopicName => _msg.TopicName;

        /// <summary>
		/// Get the topic name which contains partition part for this message. </summary>
		/// <returns> the topic name which contains Partition part </returns>

		public virtual IMessageId MessageId => _messageId;

        public virtual IMessageId InnerMessageId => _messageId.InnerMessageId;

        public virtual IDictionary<string, string> Properties => _msg.Properties;

        public bool HasProperty(string name)
		{
			return _msg.HasProperty(name);
		}

		public string GetProperty(string name)
		{
			return _msg.GetProperty(name);
		}

		public virtual sbyte[] Data => _msg.Data;

        public virtual long PublishTime => _msg.PublishTime;

        public virtual long EventTime => _msg.EventTime;

        public virtual long SequenceId => _msg.SequenceId;

        public virtual string ProducerName => _msg.ProducerName;

        public bool HasKey()
		{
			return _msg.HasKey();
		}

		public virtual string Key => _msg.Key;

        public bool HasBase64EncodedKey()
		{
			return _msg.HasBase64EncodedKey();
		}

		public virtual sbyte[] KeyBytes => _msg.KeyBytes;

        public bool HasOrderingKey()
		{
			return _msg.HasOrderingKey();
		}

		public virtual sbyte[] OrderingKey => _msg.OrderingKey;

        public virtual object Value => _msg.Value;

        public virtual EncryptionContext EncryptionCtx => _msg.EncryptionCtx;

        public virtual int RedeliveryCount => _msg.RedeliveryCount;

        public virtual sbyte[] SchemaVersion => _msg.SchemaVersion;

        public virtual bool Replicated => _msg.Replicated;

        public virtual string ReplicatedFrom => _msg.ReplicatedFrom;
        public T ToTypeOf<T>()
        {
            throw new System.NotImplementedException();
        }

        public virtual IMessage Message => _msg;
    }

}