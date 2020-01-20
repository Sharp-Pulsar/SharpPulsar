using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class MessageIdDataBuilder
    {
        private readonly MessageIdData _data;
        public MessageIdDataBuilder()
        {
            _data = new MessageIdData();
        }
        private MessageIdDataBuilder(MessageIdData data)
        {
            _data = data;
        }
        public MessageIdDataBuilder SetLedgerId(long ledgerid)
        {
            _data.ledgerId = (ulong)ledgerid;
            return new MessageIdDataBuilder(_data);
        }
        public  MessageIdDataBuilder SetEntryId(long entryId)
        {
            _data.entryId = (ulong)entryId;
            return new MessageIdDataBuilder(_data);
        }
        public MessageIdDataBuilder SetPartition(int partition)
        {
            _data.Partition = partition;
            return new MessageIdDataBuilder(_data);
        }
        public MessageIdDataBuilder SetBatchIndex(int batch)
        {
            _data.BatchIndex = batch;
            return new MessageIdDataBuilder(_data);
        }
        public MessageIdData Build()
        {
            return _data;
        }
    }
}
