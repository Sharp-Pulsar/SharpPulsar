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
	using SharpPulsar.Api;
	using IMessageId = SharpPulsar.Api.IMessageId;
	using EncryptionContext = Org.Apache.Pulsar.Common.Api.EncryptionContext;

	public class TopicMessageImpl<T> : Message<T>
	{

		/// <summary>
		/// This topicPartitionName is get from ConsumerImpl, it contains partition part. </summary>
		public virtual TopicPartitionName {get;}

		private readonly Message<T> msg;
		private readonly TopicMessageIdImpl messageId;

		public TopicMessageImpl(string TopicPartitionName, string TopicName, Message<T> Msg)
		{
			this.TopicPartitionName = TopicPartitionName;

			this.msg = Msg;
			this.messageId = new TopicMessageIdImpl(TopicPartitionName, TopicName, Msg.MessageId);
		}

		/// <summary>
		/// Get the topic name without partition part of this message. </summary>
		/// <returns> the name of the topic on which this message was published </returns>
		public virtual string TopicName
		{
			get
			{
				return msg.TopicName;
			}
		}

		/// <summary>
		/// Get the topic name which contains partition part for this message. </summary>
		/// <returns> the topic name which contains Partition part </returns>

		public virtual IMessageId MessageId
		{
			get
			{
				return messageId;
			}
		}

		public virtual IMessageId InnerMessageId
		{
			get
			{
				return messageId.InnerMessageId;
			}
		}

		public virtual IDictionary<string, string> Properties
		{
			get
			{
				return msg.Properties;
			}
		}

		public override bool HasProperty(string Name)
		{
			return msg.HasProperty(Name);
		}

		public override string GetProperty(string Name)
		{
			return msg.GetProperty(Name);
		}

		public virtual sbyte[] Data
		{
			get
			{
				return msg.Data;
			}
		}

		public virtual long PublishTime
		{
			get
			{
				return msg.PublishTime;
			}
		}

		public virtual long EventTime
		{
			get
			{
				return msg.EventTime;
			}
		}

		public virtual long SequenceId
		{
			get
			{
				return msg.SequenceId;
			}
		}

		public virtual string ProducerName
		{
			get
			{
				return msg.ProducerName;
			}
		}

		public override bool HasKey()
		{
			return msg.HasKey();
		}

		public virtual string Key
		{
			get
			{
				return msg.Key;
			}
		}

		public override bool HasBase64EncodedKey()
		{
			return msg.HasBase64EncodedKey();
		}

		public virtual sbyte[] KeyBytes
		{
			get
			{
				return msg.KeyBytes;
			}
		}

		public override bool HasOrderingKey()
		{
			return msg.HasOrderingKey();
		}

		public virtual sbyte[] OrderingKey
		{
			get
			{
				return msg.OrderingKey;
			}
		}

		public virtual T Value
		{
			get
			{
				return msg.Value;
			}
		}

		public virtual Optional<EncryptionContext> EncryptionCtx
		{
			get
			{
				return msg.EncryptionCtx;
			}
		}

		public virtual int RedeliveryCount
		{
			get
			{
				return msg.RedeliveryCount;
			}
		}

		public virtual sbyte[] SchemaVersion
		{
			get
			{
				return msg.SchemaVersion;
			}
		}

		public virtual bool Replicated
		{
			get
			{
				return msg.Replicated;
			}
		}

		public virtual string ReplicatedFrom
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