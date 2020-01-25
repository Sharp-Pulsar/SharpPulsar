
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Protocol.Builder
{
    public class MessageIdDataBuilder
    {
        private readonly MessageIdData _data;
        public MessageIdDataBuilder()
        {
            _data = new MessageIdData();
        }
        
        public MessageIdDataBuilder SetLedgerId(long ledgerid)
        {
            _data.ledgerId = (ulong)ledgerid;
            return this;
        }
        public  MessageIdDataBuilder SetEntryId(long entryId)
        {
            _data.entryId = (ulong)entryId;
            return this;
        }
        public MessageIdDataBuilder SetPartition(int partition)
        {
            _data.Partition = partition;
            return this;
        }
        public MessageIdDataBuilder SetBatchIndex(int batch)
        {
            _data.BatchIndex = batch;
            return this;
        }
        public MessageIdData Build()
        {
            return _data;
        }
    }
}
