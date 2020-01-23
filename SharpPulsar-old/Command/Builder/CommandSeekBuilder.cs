using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandSeekBuilder
    {
        private readonly CommandSeek _seek;
        public CommandSeekBuilder()
        {
            _seek = new CommandSeek();
        }
        
        public CommandSeekBuilder SetConsumerId(long consumerid)
        {
            _seek.ConsumerId = (ulong)consumerid;
            return this;
        }
        public CommandSeekBuilder SetRequestId(long requestid)
        {
            _seek.RequestId = (ulong)requestid;
            return this;
        }
        public CommandSeekBuilder SetMessageId(long ledgerid, long entryId)
        {
            var messageid = new MessageIdDataBuilder()
                .SetLedgerId(ledgerid)
                .SetEntryId(entryId).Build();
            _seek.MessageId = messageid;
            return this;
        }
        public CommandSeekBuilder SetMessagePublishTime(long messagePublishTime)
        {
            _seek.MessagePublishTime = (ulong)messagePublishTime;
            return this;
        }
        public CommandSeek Build()
        {
            return _seek;
        }
    }
}
