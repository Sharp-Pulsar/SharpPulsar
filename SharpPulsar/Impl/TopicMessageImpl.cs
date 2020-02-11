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
    using Optional;
    using Pulsar.Common.Auth;
    using Api;

	public class TopicMessageImpl<T> : Message<T>
	{

		/// <summary>
		/// This topicPartitionName is get from ConsumerImpl, it contains partition part. </summary>
		public string TopicPartitionName;

		private readonly Message<T> _msg;
		private readonly TopicMessageIdImpl _messageId;

		public TopicMessageImpl(string topicPartitionName, string topicName, Message<T> msg)
		{
			TopicPartitionName = topicPartitionName;

			_msg = msg;
			_messageId = new TopicMessageIdImpl(topicPartitionName, topicName, msg.MessageId);
		}

		/// <summary>
		/// Get the topic name without partition part of this message. </summary>
		/// <returns> the name of the topic on which this message was published </returns>
		public virtual string TopicName
		{
			get
			{
				return _msg.TopicName;
			}
		}

		/// <summary>
		/// Get the topic name which contains partition part for this message. </summary>
		/// <returns> the topic name which contains Partition part </returns>

		public virtual IMessageId MessageId
		{
			get
			{
				return _messageId;
			}
		}

		public virtual IMessageId InnerMessageId
		{
			get
			{
				return _messageId.InnerMessageId;
			}
		}

		public virtual IDictionary<string, string> Properties
		{
			get
			{
				return _msg.Properties;
			}
		}

		public bool HasProperty(string name)
		{
			return _msg.HasProperty(name);
		}

		public string GetProperty(string name)
		{
			return _msg.GetProperty(name);
		}

		public virtual sbyte[] Data
		{
			get
			{
				return _msg.Data;
			}
		}

		public virtual long PublishTime
		{
			get
			{
				return _msg.PublishTime;
			}
		}

		public virtual long EventTime
		{
			get
			{
				return _msg.EventTime;
			}
		}

		public virtual long SequenceId
		{
			get
			{
				return _msg.SequenceId;
			}
		}

		public virtual string ProducerName
		{
			get
			{
				return _msg.ProducerName;
			}
		}

		public bool HasKey()
		{
			return _msg.HasKey();
		}

		public virtual string Key
		{
			get
			{
				return _msg.Key;
			}
		}

		public bool HasBase64EncodedKey()
		{
			return _msg.HasBase64EncodedKey();
		}

		public virtual sbyte[] KeyBytes
		{
			get
			{
				return _msg.KeyBytes;
			}
		}

		public bool HasOrderingKey()
		{
			return _msg.HasOrderingKey();
		}

		public virtual sbyte[] OrderingKey
		{
			get
			{
				return _msg.OrderingKey;
			}
		}

		public virtual T Value
		{
			get
			{
				return _msg.Value;
			}
		}

		public virtual Option<EncryptionContext> EncryptionCtx
		{
			get
			{
				return _msg.EncryptionCtx;
			}
		}

		public virtual int RedeliveryCount
		{
			get
			{
				return _msg.RedeliveryCount;
			}
		}

		public virtual sbyte[] SchemaVersion
		{
			get
			{
				return _msg.SchemaVersion;
			}
		}

		public virtual bool Replicated
		{
			get
			{
				return _msg.Replicated;
			}
		}

		public virtual string ReplicatedFrom
		{
			get
			{
				return _msg.ReplicatedFrom;
			}
		}

		public virtual Message<T> Message
		{
			get
			{
				return _msg;
			}
		}
	}

}