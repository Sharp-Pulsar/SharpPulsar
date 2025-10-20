using System.Collections;
using SharpPulsar.API;

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
    public class TopicMessageId : IMessageIdAdv, ITopicMessageId
	{

		/// <summary>
		/// This topicPartitionName is get from ConsumerImpl, it contains partition part. </summary>
		
		private readonly string _ownerTopic;
		private readonly string _topicName;
        private readonly IMessageIdAdv _msgId;
        public TopicMessageId(string topic, IMessageIdAdv msgId)
        {
            _ownerTopic = topic;
            _msgId = msgId;
            _topicName = "";
        }

        public TopicMessageId(string topicPartitionName, string topicName, IMessageId messageId)
		{
			_msgId = (IMessageIdAdv)messageId;
			_ownerTopic = topicPartitionName;
			_topicName = topicName;
		}

		/// <summary>
		/// Get the topic name without partition part of this message. </summary>
		/// <returns> the name of the topic on which this message was published </returns>
		public string TopicName => _topicName;

        /// <summary>
		/// Get the topic name which contains partition part for this message. </summary>
		/// <returns> the topic name which contains Partition part </returns>
		public  string TopicPartitionName => _ownerTopic;

        public long LedgerId => _msgId.LedgerId;

        public long EntryId => _msgId.EntryId;

        public string OwnerTopic => _ownerTopic;

        public int PartitionIndex => _msgId.PartitionIndex;

        public int BatchIndex => _msgId.BatchIndex; 

        public int BatchSize => _msgId.BatchSize;   

        public BitArray AckSet => _msgId.AckSet; 

        public IMessageIdAdv FirstChunkMessageId => _msgId.FirstChunkMessageId;

        public IMessageIdAdv MessageId => _msgId;


        public byte[] ToByteArray()
		{
            return _msgId.ToByteArray();
        }

        public override int GetHashCode()
		{
            return _msgId.GetHashCode();
		}

		public override bool Equals(object obj)
		{
            return _msgId.Equals(obj);
        }

		public int CompareTo(IMessageId o)
		{
            return _msgId.CompareTo(o);
		}
        public override string ToString()
        {
            return _msgId.ToString();
        }

    }

}