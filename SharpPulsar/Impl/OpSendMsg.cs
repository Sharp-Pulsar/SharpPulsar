using System.Collections.Generic;
using System.Threading;
using DotNetty.Common;
using SharpPulsar.Shared;

namespace SharpPulsar.Impl
{
    public sealed class OpSendMsg
    {
        internal MessageImpl Msg;
        internal IList<MessageImpl> Msgs;
        internal ByteBufPair Cmd;
        internal ThreadStart RePopulate;
        internal long SequenceId;
        internal long CreatedAt;
        internal long HighestSequenceId;

        internal static OpSendMsg Create(MessageImpl msg, ByteBufPair cmd, long sequenceId)
        {
            var op = new OpSendMsg
            {
                Msg = msg, Cmd = cmd, SequenceId = sequenceId, CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
            };
            return op;
        }

        internal static OpSendMsg Create(IList<MessageImpl> msgs, ByteBufPair cmd, long sequenceId)
        {
            var op = new OpSendMsg
            {
                Msgs = msgs, Cmd = cmd, SequenceId = sequenceId, CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
            };
            return op;
        }

        internal static OpSendMsg Create(IList<MessageImpl> msgs, ByteBufPair cmd, long lowestSequenceId, long highestSequenceId)
        {
            var op = new OpSendMsg
            {
                Msgs = msgs,
                Cmd = cmd,
                SequenceId = lowestSequenceId,
                HighestSequenceId = highestSequenceId,
                CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
            };
            return op;
        }

        public void Recycle()
        {
            Msg = null;
            Msgs = null;
            Cmd = null;
            SequenceId = -1L;
            CreatedAt = -1L;
            HighestSequenceId = -1L;
        }

        public int NumMessagesInBatch { get; set; } = 1;

        public long BatchSizeByte { get; set; } = 0;

        public void SetMessageId(long ledgerId, long entryId, int partitionIndex)
        {
            if (Msg != null)
            {
                Msg.SetMessageId(new MessageIdImpl(ledgerId, entryId, partitionIndex));
            }
            else
            {
                for (var batchIndex = 0; batchIndex < Msgs.Count; batchIndex++)
                {
                    Msgs[batchIndex].SetMessageId(new BatchMessageIdImpl(ledgerId, entryId, partitionIndex, batchIndex));
                }
            }
        }

    }
}