using System.Collections.Generic;
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

    public class MessagePayloadContext : IMessagePayloadContext
    {        
        private BrokerEntryMetadata _brokerEntryMetadata;
        private MessageMetadata _messageMetadata;
        private SingleMessageMetadata _singleMessageMetadata;
        private MessageId _messageId;
        private ConsumerImpl<object> _consumer;
        private int _redeliveryCount;
        private BatchMessageAcker _acker;
        private BitSetRecyclable _ackBitSet;

        // JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        // ORIGINAL LINE: public static MessagePayloadContextImpl get(final org.apache.pulsar.common.api.proto.BrokerEntryMetadata brokerEntryMetadata, @NonNull final org.apache.pulsar.common.api.proto.MessageMetadata messageMetadata, @NonNull final MessageIdImpl messageId, @NonNull final ConsumerImpl<?> consumer, final int redeliveryCount, final java.util.List<long> ackSet)
        public static MessagePayloadContext Get<T1>(BrokerEntryMetadata brokerEntryMetadata, MessageMetadata messageMetadata, MessageId messageId, ConsumerImpl<T1> consumer, in int redeliveryCount, in IList<long> ackSet)
        {
            var context = new MessagePayloadContext();
            context._brokerEntryMetadata = brokerEntryMetadata;
            context._messageMetadata = messageMetadata;
            context._singleMessageMetadata = new SingleMessageMetadata();
            context._messageId = messageId;
            context._consumer = consumer;
            context._redeliveryCount = redeliveryCount;
            context._acker = BatchMessageAcker.NewAcker(context.NumMessages);
            context._ackBitSet = ackSet != null && ackSet.Count > 0 ? BitSetRecyclable.ValueOf(SafeCollectionUtils.LongListToArray(ackSet)) : null;
            return context;
        }

        public virtual void Recycle()
        {
            _brokerEntryMetadata = null;
            _messageMetadata = null;
            _singleMessageMetadata = null;
            _messageId = null;
            _consumer = null;
            _redeliveryCount = 0;
            _acker = null;
            if (ackBitSet != null)
            {
                ackBitSet.Recycle();
                ackBitSet = null;
            }
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
                return consumer.IsBatch(messageMetadata);
            }
        }

        public virtual Message<T> GetMessageAt<T>(int index, int numMessages, IMessagePayload payload, bool containMetadata, ISchema<T> schema)
        {
            var payloadBuffer = MessagePayloadUtils.ConvertToArray(payload);
            try
            {
                return consumer.NewSingleMessage(index, numMessages, _brokerEntryMetadata, _messageMetadata, _singleMessageMetadata, payloadBuffer, _messageId, schema, containMetadata, _ackBitSet, _acker, _redeliveryCount);
            }
            finally
            {
                PayloadBuffer.release();
            }
        }

        public virtual Message<T> AsSingleMessage<T>(IMessagePayload payload, ISchema<T> schema)
        {
            var payloadBuffer = MessagePayloadUtils.ConvertToArray(payload);
            try
            {
                return consumer.NewMessage(_messageId, _brokerEntryMetadata, _messageMetadata, payloadBuffer, schema, _redeliveryCount);
            }
            finally
            {
                payloadBuffer.release();
            }
        }
    }

}