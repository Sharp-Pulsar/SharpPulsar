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
        private CommandSeekBuilder(CommandSeek seek)
        {
            _seek = seek;
        }
        public CommandSeekBuilder SetConsumerId(long consumerid)
        {
            _seek.ConsumerId = (ulong)consumerid;
            return new CommandSeekBuilder(_seek);
        }
        public CommandSeekBuilder SetRequestId(long requestid)
        {
            _seek.RequestId = (ulong)requestid;
            return new CommandSeekBuilder(_seek);
        }
        public CommandSeekBuilder SetMessageId(long ledgerid, long entryId)
        {
            var messageid = new MessageIdDataBuilder()
                .SetLedgerId(ledgerid)
                .SetEntryId(entryId).Build();
            _seek.MessageId = messageid;
            return new CommandSeekBuilder(_seek);
        }
        public CommandSeekBuilder SetMessagePublishTime(long messagePublishTime)
        {
            _seek.MessagePublishTime = (ulong)messagePublishTime;
            return new CommandSeekBuilder(_seek);
        }
        public CommandSeek Build()
        {
            return _seek;
        }
    }
}
