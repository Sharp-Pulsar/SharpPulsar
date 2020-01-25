using System;

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
	using IMessageId = SharpPulsar.Api.IMessageId;

	[Serializable]
	public class TopicMessageIdImpl : IMessageId
	{

		/// <summary>
		/// This topicPartitionName is get from ConsumerImpl, it contains partition part. </summary>
		private readonly string topicPartitionName;
		private readonly string topicName;
		public virtual InnerMessageId {get;}

		public TopicMessageIdImpl(string TopicPartitionName, string TopicName, IMessageId MessageId)
		{
			this.InnerMessageId = MessageId;
			this.topicPartitionName = TopicPartitionName;
			this.topicName = TopicName;
		}

		/// <summary>
		/// Get the topic name without partition part of this message. </summary>
		/// <returns> the name of the topic on which this message was published </returns>
		public virtual string TopicName
		{
			get
			{
				return this.topicName;
			}
		}

		/// <summary>
		/// Get the topic name which contains partition part for this message. </summary>
		/// <returns> the topic name which contains Partition part </returns>
		public virtual string TopicPartitionName
		{
			get
			{
				return this.topicPartitionName;
			}
		}


		public override sbyte[] ToByteArray()
		{
			return InnerMessageId.ToByteArray();
		}

		public override int GetHashCode()
		{
			return Objects.hash(topicPartitionName, InnerMessageId);
		}

		public override bool Equals(object Obj)
		{
			if (!(Obj is TopicMessageIdImpl))
			{
				return false;
			}
			TopicMessageIdImpl Other = (TopicMessageIdImpl) Obj;
			return Objects.equals(topicPartitionName, Other.topicPartitionName) && Objects.equals(InnerMessageId, Other.InnerMessageId);
		}

		public override int CompareTo(IMessageId O)
		{
			return InnerMessageId.CompareTo(O);
		}
	}

}