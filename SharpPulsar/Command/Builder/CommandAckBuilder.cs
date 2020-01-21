using SharpPulsar.Common.PulsarApi;
using System;
using System.Collections.Generic;
using System.Text;
using static SharpPulsar.Common.PulsarApi.CommandAck;

namespace SharpPulsar.Command.Builder
{
    public class CommandAckBuilder
    {
        private CommandAck _ack;
        public CommandAckBuilder()
        {
            _ack = new CommandAck();
        }
        
        public CommandAckBuilder SetAckType(AckType type)
        {
            _ack.ack_type = type;
            return this;
        }
        public CommandAckBuilder SetConsumerId(long consumerid)
        {
            _ack.ConsumerId = (ulong)consumerid;
            return this;
        }
        public CommandAckBuilder SetTxnidLeastBits(long txnIdLeastBits)
        {
            if (txnIdLeastBits > 0)
            {
                _ack.TxnidLeastBits = (ulong)txnIdLeastBits;
            }
            return this;
        }
        public CommandAckBuilder SetTxnidMostBits(long txnidMostBits)
        {
            if (txnidMostBits > 0)
            {
                _ack.TxnidMostBits = (ulong)txnidMostBits;
            }
            return this;
        }
        public CommandAckBuilder SetValidationError(ValidationError error)
        {
            _ack.validation_error = error;
            return this;
        }
        public CommandAckBuilder AddProperties(IDictionary<string, long> properties)
        {
            foreach (KeyValuePair<string, long> e in properties.SetOfKeyValuePairs())
            {
                _ack.Properties.Add(new KeyLongValue { Key = e.Key, Value = (ulong)e.Value });
            }
            return this;
        }
        public CommandAckBuilder SetMessageIds(IList<KeyValuePair<long, long>> entries)
        {
            var entriesCount = entries.Count;
            for (int i = 0; i < entriesCount; i++)
            {
                long ledgerId = entries[i].Key;
                long entryId = entries[i].Value;

                var messageIdData = new MessageIdDataBuilder()
                .SetLedgerId(ledgerId)
                .SetEntryId(entryId).Build();
                _ack.MessageIds.Add(messageIdData);
            }
            return this;
        }
        public CommandAck Build()
        {
            return _ack;
        }
    }
}
