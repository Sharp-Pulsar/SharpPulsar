using System.Collections.Generic;
using SharpPulsar.Batch;

namespace SharpPulsar
{
    public sealed class OpSendMsg<T>
    {
        internal Message<T> Msg;
        internal IList<Message<T>> Msgs;
        internal byte[] Cmd;
        internal long SequenceId;
        internal long CreatedAt;
        internal long HighestSequenceId;

        internal int TotalChunks = 0;
        internal int ChunkId = -1;

        internal static OpSendMsg<T> Create(Message<T> msg, byte[] cmd, long sequenceId)
        {
            var op = new OpSendMsg<T>
            {
                Msg = msg,
                Cmd = cmd,
                SequenceId = sequenceId,
                CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
            };
            return op;
        }

        internal static OpSendMsg<T> Create(IList<Message<T>> msgs, byte[] cmd, long sequenceId)
        {
            var op = new OpSendMsg<T>
            {
                Msgs = msgs,
                Cmd = cmd,
                SequenceId = sequenceId,
                CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
            };
            return op;
        }

        internal static OpSendMsg<T> Create(IList<Message<T>> msgs, byte[] cmd, long lowestSequenceId, long highestSequenceId)
        {
            var op = new OpSendMsg<T>
            {
                Msgs = msgs,
                Cmd = cmd,
                SequenceId = lowestSequenceId,
                HighestSequenceId = highestSequenceId,
                CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
            };
            return op;
        }

        public int NumMessagesInBatch { get; set; } = 1;

        public long BatchSizeByte { get; set; } = 0;

        public void SetMessageId(long ledgerId, long entryId, int partitionIndex)
        {
            if (Msg != null)
            {
                Msg.SetMessageId(new MessageId(ledgerId, entryId, partitionIndex));
            }
            else
            {
                for (var batchIndex = 0; batchIndex < Msgs.Count; batchIndex++)
                {
                    Msgs[batchIndex].SetMessageId(new BatchMessageId(ledgerId, entryId, partitionIndex, batchIndex));
                }
            }
        }


        public void Recycle()
        {
            Msg = null;
            Msgs = null;
            Cmd = null;
            SequenceId = -1L;
            CreatedAt = -1L;
            HighestSequenceId = -1L;
            TotalChunks = 0;
            ChunkId = -1;
        }

    }
}