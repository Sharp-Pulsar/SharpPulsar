using SharpPulsar.Common.PulsarApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Command.Builder
{
    public class CommandActiveConsumerChangeBuilder
    {
        public CommandActiveConsumerChange _change;
        public CommandActiveConsumerChangeBuilder()
        {
            _change = new CommandActiveConsumerChange();
        }
        private CommandActiveConsumerChangeBuilder(CommandActiveConsumerChange change)
        {
            _change = change;
        }
        public CommandActiveConsumerChangeBuilder SetConsumerId(long consumerId)
        {
            _change.ConsumerId = (ulong)consumerId;
            return new CommandActiveConsumerChangeBuilder(_change);
        }
        public CommandActiveConsumerChangeBuilder SetIsActive(bool isActive)
        {
            _change.IsActive = isActive;
            return new CommandActiveConsumerChangeBuilder(_change);
        }
        public CommandActiveConsumerChange Build()
        {
            return _change;
        }
    }
}
