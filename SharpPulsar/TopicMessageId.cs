using SharpPulsar.Batch;
using SharpPulsar.Interfaces;
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
namespace SharpPulsar
{
	public class TopicMessageId : IMessageId
	{

		/// <summary>
		/// This topicPartitionName is get from ConsumerImpl, it contains partition part. </summary>
		
		private readonly string _topicPartitionName;
		private string _topicName;
		public readonly IMessageId InnerMessageId;

		public TopicMessageId(string topicPartitionName, string topicName, IMessageId messageId)
		{
			this.InnerMessageId = messageId;
			_topicPartitionName = topicPartitionName;
			_topicName = topicName;
		}

		/// <summary>
		/// Get the topic name without partition part of this message. </summary>
		/// <returns> the name of the topic on which this message was published </returns>
		public string TopicName => _topicName;

        /// <summary>
		/// Get the topic name which contains partition part for this message. </summary>
		/// <returns> the topic name which contains Partition part </returns>
		public  string TopicPartitionName => _topicPartitionName;


        public sbyte[] ToByteArray()
		{
			return InnerMessageId.ToByteArray();
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_topicPartitionName, InnerMessageId);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TopicMessageId))
			{
				return false;
			}
			var other = (TopicMessageId) obj;
			return object.Equals(TopicPartitionName, other.TopicPartitionName) && object.Equals(InnerMessageId, other.InnerMessageId);
		}

		public int CompareTo(IMessageId o)
		{
			var m = InnerMessageId as BatchMessageId;
			if (m != null)
				return m.CompareTo(o);
			return ((MessageId)InnerMessageId).CompareTo(o);
		}
	}

}