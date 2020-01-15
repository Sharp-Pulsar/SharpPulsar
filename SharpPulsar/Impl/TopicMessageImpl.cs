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

namespace org.apache.pulsar.client.impl
{
	using Message = org.apache.pulsar.client.api.Message;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using EncryptionContext = org.apache.pulsar.common.api.EncryptionContext;

	public class TopicMessageImpl<T> : Message<T>
	{

		/// <summary>
		/// This topicPartitionName is get from ConsumerImpl, it contains partition part. </summary>
		private readonly string topicPartitionName;

		private readonly Message<T> msg;
		private readonly TopicMessageIdImpl messageId;

		internal TopicMessageImpl(string topicPartitionName, string topicName, Message<T> msg)
		{
			this.topicPartitionName = topicPartitionName;

			this.msg = msg;
			this.messageId = new TopicMessageIdImpl(topicPartitionName, topicName, msg.MessageId);
		}

		/// <summary>
		/// Get the topic name without partition part of this message. </summary>
		/// <returns> the name of the topic on which this message was published </returns>
		public override string TopicName
		{
			get
			{
				return msg.TopicName;
			}
		}

		/// <summary>
		/// Get the topic name which contains partition part for this message. </summary>
		/// <returns> the topic name which contains Partition part </returns>
		public virtual string TopicPartitionName
		{
			get
			{
				return topicPartitionName;
			}
		}

		public override MessageId MessageId
		{
			get
			{
				return messageId;
			}
		}

		public virtual MessageId InnerMessageId
		{
			get
			{
				return messageId.InnerMessageId;
			}
		}

		public override IDictionary<string, string> Properties
		{
			get
			{
				return msg.Properties;
			}
		}

		public override bool hasProperty(string name)
		{
			return msg.hasProperty(name);
		}

		public override string getProperty(string name)
		{
			return msg.getProperty(name);
		}

		public override sbyte[] Data
		{
			get
			{
				return msg.Data;
			}
		}

		public override long PublishTime
		{
			get
			{
				return msg.PublishTime;
			}
		}

		public override long EventTime
		{
			get
			{
				return msg.EventTime;
			}
		}

		public override long SequenceId
		{
			get
			{
				return msg.SequenceId;
			}
		}

		public override string ProducerName
		{
			get
			{
				return msg.ProducerName;
			}
		}

		public override bool hasKey()
		{
			return msg.hasKey();
		}

		public override string Key
		{
			get
			{
				return msg.Key;
			}
		}

		public override bool hasBase64EncodedKey()
		{
			return msg.hasBase64EncodedKey();
		}

		public override sbyte[] KeyBytes
		{
			get
			{
				return msg.KeyBytes;
			}
		}

		public override bool hasOrderingKey()
		{
			return msg.hasOrderingKey();
		}

		public override sbyte[] OrderingKey
		{
			get
			{
				return msg.OrderingKey;
			}
		}

		public override T Value
		{
			get
			{
				return msg.Value;
			}
		}

		public override Optional<EncryptionContext> EncryptionCtx
		{
			get
			{
				return msg.EncryptionCtx;
			}
		}

		public override int RedeliveryCount
		{
			get
			{
				return msg.RedeliveryCount;
			}
		}

		public override sbyte[] SchemaVersion
		{
			get
			{
				return msg.SchemaVersion;
			}
		}

		public override bool Replicated
		{
			get
			{
				return msg.Replicated;
			}
		}

		public override string ReplicatedFrom
		{
			get
			{
				return msg.ReplicatedFrom;
			}
		}

		public virtual Message<T> Message
		{
			get
			{
				return msg;
			}
		}
	}

}