
namespace SharpPulsar.Protocol.Proto
{
    public partial class MessageIdData
    {
        public static Builder NewBuilder()
        {
            return Builder.Create();
        }
        
        public sealed class Builder
        {
            private MessageIdData _messageId;

            public Builder()
            {
                _messageId = new MessageIdData();
            }
            internal static Builder Create()
            {
                return new Builder();
            }
            
            public MessageIdData Build()
            {
                return _messageId;
            }

            public bool HasLedgerId()
            {
                return _messageId.HasLedgerId;
            }

            public Builder SetLedgerId(long value)
            {
                _messageId.LedgerId = (ulong)value;
                return this;
            }
            
            public bool HasEntryId()
            {
                return _messageId.HasEntryId;
            }

            public Builder SetEntryId(long value)
            {
                _messageId.EntryId = (ulong)value;
                return this;
            }
            
            public bool HasPartition()
            {
                return _messageId.HasPartition;
            }

            public Builder SetPartition(int value)
            {
                _messageId.Partition = value;
                return this;
            }
            
            public bool HasBatchIndex()
            {
                return _messageId.HasBatchIndex;
            }

            public Builder SetBatchIndex(int value)
            {
                _messageId.BatchIndex = value;
                return this;
            }
            
        }

    }


}
