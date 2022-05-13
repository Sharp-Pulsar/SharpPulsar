
using System;
using SharpPulsar.Interfaces;
using SharpPulsar.Protocol.Proto;
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

    [Serializable]
    public class ChunkMessageId : MessageId, IMessageId
    {
        private MessageId _firstChunkMsgId;

        public ChunkMessageId(MessageId firstChunkMsgId, MessageId lastChunkMsgId) : base(lastChunkMsgId.LedgerId, lastChunkMsgId.EntryId, lastChunkMsgId.PartitionIndex)
        {
            _firstChunkMsgId = firstChunkMsgId;
        }

        public virtual MessageId FirstChunkMessageId
        {
            get
            {
                return _firstChunkMsgId;
            }
        }

        public virtual MessageId LastChunkMessageId
        {
            get
            {
                return this;
            }
        }

        public override string ToString()
        {
            return _firstChunkMsgId.ToString() + ';' + base.ToString();
        }

        public override byte[] ToByteArray()
        {

            // write last chunk message id
            var msgId = new MessageIdData { ledgerId = 0, entryId = 0, Partition = 0 };

            // write first chunk message id
            msgId.FirstChunkMessageId = msgId;
            _firstChunkMsgId = new MessageId(-1, -1, 0);

            return FirstChunkMessageId.ToByteArray();
        }

        public override bool Equals(object O)
        {
            return base.Equals(O);
        }

        public override int GetHashCode()
        {
            // return  (base.GetHashCode(), _firstChunkMsgId.GetHashCode());
            return 1;
        }

    }

}