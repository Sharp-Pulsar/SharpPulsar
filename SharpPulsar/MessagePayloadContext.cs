using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Batch;
using SharpPulsar.Interfaces;
using SharpPulsar.Protocol.Proto;

// / <summary>
// / Licensed to the Apache Software Foundation (ASF) under one
// / or more contributor license agreements.  See the NOTICE file
// / distributed with this work for additional information
// / regarding copyright ownership.  The ASF licenses this file
// / to you under the Apache License, Version 2.0 (the
// / "License"); you may not use this file except in compliance
// / with the License.  You may obtain a copy of the License at
// / 
// /   http://www.apache.org/licenses/LICENSE-2.0
// / 
// / Unless required by applicable law or agreed to in writing,
// / software distributed under the License is distributed on an
// / "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// / KIND, either express or implied.  See the License for the
// / specific language governing permissions and limitations
// / under the License.
// / </summary>
namespace SharpPulsar
{

    public class MessagePayloadContext<T> : IMessagePayloadContext<T>
    {        
        private BrokerEntryMetadata _brokerEntryMetadata;
        private MessageMetadata _messageMetadata;
        private SingleMessageMetadata _singleMessageMetadata;
        private MessageId _messageId;
        private int _redeliveryCount;
        private BatchMessageAcker _acker;
        private BitSet _ackBitSet;
        private Func<MessageId, BrokerEntryMetadata, MessageMetadata, ReadOnlySequence<byte>, ISchema<T>, int, IMessage<T>> _asSingleMessage;
        private Func<int, int, BrokerEntryMetadata, MessageMetadata, SingleMessageMetadata, byte[], MessageId, ISchema<T>, bool, BitSet, BatchMessageAcker, int, IMessage<T>> _newSingleMessage;
        private Func<MessageMetadata, bool> _isBatch;

        public static MessagePayloadContext<T> Get(BrokerEntryMetadata brokerEntryMetadata, MessageMetadata messageMetadata, MessageId messageId, int redeliveryCount, IList<long> ackSet, 
            Func<MessageMetadata, bool> isbacth, 
            Func<MessageId, BrokerEntryMetadata, MessageMetadata, ReadOnlySequence<byte>, ISchema<T>, int, IMessage<T>> asSingleMessage,
            Func<int, int, BrokerEntryMetadata, MessageMetadata, SingleMessageMetadata, byte[], MessageId, ISchema<T>, bool, BitSet, BatchMessageAcker, int, IMessage<T>> newSingleMessage)
        {
            var context = new MessagePayloadContext<T>
            {
                _brokerEntryMetadata = brokerEntryMetadata,
                _messageMetadata = messageMetadata,
                _singleMessageMetadata = new SingleMessageMetadata(),
                _messageId = messageId,
                _redeliveryCount = redeliveryCount,
                _isBatch = isbacth,
                _asSingleMessage = asSingleMessage,
                _newSingleMessage = newSingleMessage
            };
            context._acker = BatchMessageAcker.NewAcker(context.NumMessages);
            context._ackBitSet = ackSet != null && ackSet.Count > 0 ? BitSet.ValueOf(ackSet.ToArray()) : null;
            return context;
        }

        public virtual void Recycle()
        {
            _brokerEntryMetadata = null;
            _messageMetadata = null;
            _singleMessageMetadata = null;
            _messageId = null;
            _redeliveryCount = 0;
            _acker = null;
            _ackBitSet = null;
        }

        public virtual string GetProperty(string key)
        {
            foreach (var keyValue in _messageMetadata.Properties)
            {
                if (!string.IsNullOrWhiteSpace(keyValue.Key) && keyValue.Key.Equals(key))
                {
                    return keyValue.Value;
                }
            }
            return null;
        }

        public virtual int NumMessages
        {
            get
            {
                return _messageMetadata.NumMessagesInBatch;
            }
        }

        public virtual bool Batch
        {
            get
            {
                return _isBatch(_messageMetadata);
            }
        }

        public virtual IMessage<T> GetMessageAt(int index, int numMessages, IMessagePayload payload, bool containMetadata, ISchema<T> schema)
        {
            var payloadBuffer = MessagePayloadUtils.ConvertToArray(payload);
            try
            {
                return _newSingleMessage(index, numMessages, _brokerEntryMetadata, _messageMetadata, _singleMessageMetadata, payloadBuffer, _messageId, schema, containMetadata, _ackBitSet, _acker, _redeliveryCount);
            }
            finally
            {
                //PayloadBuffer.release();
            }
        }

        public virtual IMessage<T> AsSingleMessage(IMessagePayload payload, ISchema<T> schema)
        {
            var payloadBuffer = MessagePayloadUtils.ConvertToArray(payload);
            try
            {
                //return consumer.NewMessage(_messageId, _brokerEntryMetadata, _messageMetadata, payloadBuffer, schema, _redeliveryCount);
                return (IMessage<T>)_asSingleMessage(_messageId, _brokerEntryMetadata, _messageMetadata, new ReadOnlySequence<byte>(payloadBuffer), schema, _redeliveryCount);
            }
            finally
            {
                
            }
        }
    }

}