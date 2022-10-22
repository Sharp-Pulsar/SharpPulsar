using System.Collections.Generic;
using SharpPulsar.Auth;

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
    using System.Buffers;
    using Akka.Actor;
    using global::Akka.Util;
    using SharpPulsar.Interfaces;

    internal class TopicMessage<T> : IMessage<T>
	{

		/// <summary>
		/// This topicPartitionName is get from ConsumerImpl, it contains partition part. </summary>
		private readonly string _topicPartitionName;

		private readonly IMessage<T> _msg;
		private readonly TopicMessageId _messageId;

        // consumer if this message is received by that consumer
        internal readonly IActorRef ReceivedByConsumer;
        public readonly Consumer<T> ReceivedByconsumer;
        internal TopicMessage(string topicPartitionName, string topicName, IMessage<T> msg, IActorRef receivedByConsumer)
		{
			_topicPartitionName = topicPartitionName;
            ReceivedByConsumer = receivedByConsumer;
            _msg = msg;
			_messageId = new TopicMessageId(topicPartitionName, topicName, msg.MessageId);
		}
        public TopicMessage(string topicPartitionName, string topicName, IMessage<T> msg, Consumer<T> receivedByConsumer)
        {
            _topicPartitionName = topicPartitionName;
            ReceivedByconsumer = receivedByConsumer;
            _msg = msg;
            _messageId = new TopicMessageId(topicPartitionName, topicName, msg.MessageId);
        }
        /// <summary>
        /// Get the topic name without partition part of this message. </summary>
        /// <returns> the name of the topic on which this message was published </returns>
        public virtual string Topic => _msg.Topic;
		/// <summary>
		/// Get the topic name which contains partition part for this message. </summary>
		/// <returns> the topic name which contains Partition part </returns>
		public virtual string TopicPartitionName
		{
			get
			{
				return _topicPartitionName;
			}
		}
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

        public long Size()
        {
            return _msg.Size();
        }
		public string GetProperty(string name)
		{
			return _msg.GetProperty(name);
		}

		public virtual ReadOnlySequence<byte> Data => _msg.Data;

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

		public virtual byte[] KeyBytes => _msg.KeyBytes;

        public bool HasOrderingKey()
		{
			return _msg.HasOrderingKey();
		}

        public void AddProperty(IDictionary<string, string> props)
        {
           _msg.AddProperty(props);
        }

        public virtual byte[] OrderingKey => _msg.OrderingKey;

        public virtual T Value => _msg.Value;

        public virtual Option<EncryptionContext> EncryptionCtx => _msg.EncryptionCtx;

        public virtual int RedeliveryCount => _msg.RedeliveryCount;

        public virtual byte[] SchemaVersion => _msg.SchemaVersion;

        public virtual bool Replicated => _msg.Replicated;

        public virtual string ReplicatedFrom => _msg.ReplicatedFrom;

        public virtual IMessage<T> Message => _msg;
        public virtual ISchema<T> SchemaInternal
        {
            get
            {
                if (_msg is Message<T>)
                {
                    var message = (Message<T>)_msg;
                    return message.SchemaInternal();
                }
                return null;
            }
        }

        public  Option<ISchema<T>> ReaderSchema
        {
            get
            {
                return _msg.ReaderSchema;
            }
        }

        public bool HasBrokerPublishTime()
        {
            return _msg.HasBrokerPublishTime();
        }

        public long? BrokerPublishTime
        {
            get
            {
                return _msg.BrokerPublishTime;
            }
        }

        public bool HasIndex()
        {
            return _msg.HasIndex();
        }

        public long? Index
        {
            get
            {
                return _msg.Index;
            }
        }

    }

}