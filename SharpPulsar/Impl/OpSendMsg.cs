using System.Collections.Generic;
using System.Threading;
using DotNetty.Common;
using SharpPulsar.Shared;

namespace SharpPulsar.Impl
{
    public sealed class OpSendMsg<T>
    {
        internal MessageImpl<object> Msg;
        internal IList<MessageImpl<object>> Msgs;
        internal ByteBufPair Cmd;
        internal SendCallback Callback;
        internal ThreadStart RePopulate;
        internal long SequenceId;
        internal long CreatedAt;
        internal long HighestSequenceId;

        internal static OpSendMsg<T> Create(MessageImpl<T> msg, ByteBufPair cmd, long sequenceId, SendCallback callback)
        {
            var op = Pool.Take();
            op.Msg = msg;
            op.Cmd = cmd;
            op.Callback = callback;
            op.SequenceId = sequenceId;
            op.CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
            return op;
        }

        internal static OpSendMsg<T> Create(IList<MessageImpl<T>> msgs, ByteBufPair cmd, long sequenceId, SendCallback callback)
        {
            var op = Pool.Take();
            op.Msgs = (IList<MessageImpl<object>>) msgs;
            op.Cmd = cmd;
            op.Callback = callback;
            op.SequenceId = sequenceId;
            op.CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
            return op;
        }

        internal static OpSendMsg<T> Create(IList<MessageImpl<T>> msgs, ByteBufPair cmd, long lowestSequenceId, long highestSequenceId, SendCallback callback)
        {
            var op = Pool.Take();
            op.Msgs = (IList<MessageImpl<object>>)msgs;
            op.Cmd = cmd;
            op.Callback = callback;
            op.SequenceId = lowestSequenceId;
            op.HighestSequenceId = highestSequenceId;
            op.CreatedAt = DateTimeHelper.CurrentUnixTimeMillis();
            return op;
        }

        public void Recycle()
        {
            Msg = null;
            Msgs = null;
            Cmd = null;
            Callback = null;
            RePopulate = null;
            SequenceId = -1L;
            CreatedAt = -1L;
            HighestSequenceId = -1L;
            Handle.Release(this);
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

        internal static ThreadLocalPool<OpSendMsg<T>> Pool = new ThreadLocalPool<OpSendMsg<T>>(handle => new OpSendMsg<T>(handle), 1, true);

        internal ThreadLocalPool.Handle Handle;
        private OpSendMsg(ThreadLocalPool.Handle handle)
        {
            Handle = handle;
        }
    }
}