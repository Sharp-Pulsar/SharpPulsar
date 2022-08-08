
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
            private readonly MessageIdData _messageId;

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

            public Builder SetLedgerId(long value)
            {
                _messageId.ledgerId = (ulong)value;
                return this;
            }
            
            public Builder SetEntryId(long value)
            {
                _messageId.entryId = (ulong)value;
                return this;
            }
            
            public Builder SetPartition(int value)
            {
                _messageId.Partition = value;
                return this;
            }
            
            public Builder SetBatchIndex(int value)
            {
                _messageId.BatchIndex = value;
                return this;
            }
            
        }

    }


}
