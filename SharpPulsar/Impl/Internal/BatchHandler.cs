﻿using SharpPulsar.Common.PulsarApi;
using SharpPulsar.Interface.Message;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace SharpPulsar.Impl.Internal
{
    public sealed class BatchHandler<T>
    {
        private readonly bool _trackBatches;
        private readonly Queue<IMessage<T>> _messages;
        private readonly LinkedList<Batch> _batches;

        public BatchHandler(bool trackBatches)
        {
            _trackBatches = trackBatches;
            _messages = new Queue<IMessage<T>>();
            _batches = new LinkedList<Batch>();
        }

        public IMessage Add(MessageIdData messageId, MessageMetadata metadata, ReadOnlySequence<byte> data)
        {
            if (_trackBatches)
                _batches.AddLast(new Batch(messageId, metadata.NumMessagesInBatch));

            long index = 0;
            for (var i = 0; i < metadata.NumMessagesInBatch; ++i)
            {
                var singleMetadataSize = data.ReadUInt32(index, true);
                index += 4;
                var singleMetadata = Serializer.Deserialize<SingleMessageMetadata>(data.Slice(index, singleMetadataSize));
                index += singleMetadataSize;
                var singleMessageId = new MessageId(messageId.LedgerId, messageId.EntryId, messageId.Partition, i);
                var message = new Message(singleMessageId, metadata, singleMetadata, data.Slice(index, singleMetadata.PayloadSize));
                _messages.Enqueue(message);
                index += (uint)singleMetadata.PayloadSize;
            }

            return _messages.Dequeue();
        }

        public Message? GetNext() => _messages.Count == 0 ? null : _messages.Dequeue();

        public void Clear()
        {
            _messages.Clear();
            _batches.Clear();
        }

        public MessageIdData? Acknowledge(MessageIdData messageId)
        {
            foreach (var batch in _batches)
            {
                if (messageId.LedgerId != batch.MessageId.LedgerId ||
                    messageId.EntryId != batch.MessageId.EntryId ||
                    messageId.Partition != batch.MessageId.Partition)
                    continue;

                batch.Acknowledge(messageId.BatchIndex);
                if (batch.IsAcknowledged())
                {
                    _batches.Remove(batch);
                    return batch.MessageId;
                }
                break;
            }

            return null;
        }

        private sealed class Batch
        {
            private readonly BitArray _acknowledgementIndex;

            public Batch(MessageIdData messageId, int numberOfMessages)
            {
                MessageId = messageId;
                _acknowledgementIndex = new BitArray(numberOfMessages, false);
            }

            public MessageIdData MessageId { get; }

            public void Acknowledge(int batchIndex) => _acknowledgementIndex.Set(batchIndex, true);

            public bool IsAcknowledged()
            {
                for (var i = 0; i < _acknowledgementIndex.Length; i++)
                {
                    if (!_acknowledgementIndex[i])
                        return false;
                }

                return true;
            }
        }
    }
}
