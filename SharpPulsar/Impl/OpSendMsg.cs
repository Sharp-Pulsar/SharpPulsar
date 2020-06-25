using System.Collections.Generic;
using SharpPulsar.Api;
using SharpPulsar.Batch;

namespace SharpPulsar.Impl
{
    public sealed class OpSendMsg
    {
        internal Message Msg;
        internal IList<Message> Msgs;
        internal ISendCallback Callback;
        internal byte[] Cmd;
        internal long SequenceId;
        internal long CreatedAt;
        internal long HighestSequenceId;
        
        internal int TotalChunks = 0;
        internal int ChunkId = -1;

        internal static OpSendMsg Create(Message msg, byte[] cmd, long sequenceId, ISendCallback callback)
        {
            var op = new OpSendMsg
            {
                Msg = msg, 
                Cmd = cmd, 
                SequenceId = sequenceId, 
                CreatedAt = DateTimeHelper.CurrentUnixTimeMillis(),
                Callback = callback
            };
            return op;
        }

        internal static OpSendMsg Create(IList<Message> msgs, byte[] cmd, long sequenceId, ISendCallback callback)
        {
            var op = new OpSendMsg
            {
                Msgs = msgs, 
                Cmd = cmd, 
                SequenceId = sequenceId, 
                CreatedAt = DateTimeHelper.CurrentUnixTimeMillis(),
                Callback = callback
            };
            return op;
        }

        internal static OpSendMsg Create(IList<Message> msgs, byte[] cmd, long lowestSequenceId, long highestSequenceId, ISendCallback callback)
        {
            var op = new OpSendMsg
            {
                Msgs = msgs,
                Cmd = cmd,
                SequenceId = lowestSequenceId,
                HighestSequenceId = highestSequenceId,
                CreatedAt = DateTimeHelper.CurrentUnixTimeMillis(),
                Callback = callback
            };
            return op;
        }
        
        public int NumMessagesInBatch { get; set; } = 1;

        public long BatchSizeByte { get; set; } = 0;

        public void SetMessageId(long ledgerId, long entryId, int partitionIndex)
        {
            if (Msg != null)
            {
                Msg.SetMessageId(new MessageId(ledgerId, entryId, partitionIndex, null));
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
            Callback = null;
            SequenceId = -1L;
            CreatedAt = -1L;
            HighestSequenceId = -1L;
            TotalChunks = 0;
            ChunkId = -1;
        }

    }
}