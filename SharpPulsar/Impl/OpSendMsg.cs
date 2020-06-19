using System.Collections.Generic;
using System.Threading;

namespace SharpPulsar.Impl
{
    public sealed class OpSendMsg
    {
        internal Message Msg;
        internal IList<Message> Msgs;
        internal byte[] Cmd;
        internal ThreadStart RePopulate;
        internal long SequenceId;
        internal long CreatedAt;
        internal long HighestSequenceId;

        internal static OpSendMsg Create(Message msg, byte[] cmd, long sequenceId)
        {
            var op = new OpSendMsg
            {
                Msg = msg, Cmd = cmd, SequenceId = sequenceId, CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
            };
            return op;
        }

        internal static OpSendMsg Create(IList<Message> msgs, byte[] cmd, long sequenceId)
        {
            var op = new OpSendMsg
            {
                Msgs = msgs, Cmd = cmd, SequenceId = sequenceId, CreatedAt = DateTimeHelper.CurrentUnixTimeMillis()
            };
            return op;
        }

        internal static OpSendMsg Create(IList<Message> msgs, byte[] cmd, long lowestSequenceId, long highestSequenceId)
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
                    Msgs[batchIndex].SetMessageId(new BatchMessageId(ledgerId, entryId, partitionIndex, batchIndex, null));
                }
            }
        }

    }
}