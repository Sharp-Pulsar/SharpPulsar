using System;
using System.Linq;
using PulsarAdmin.Models;

namespace SharpPulsar.Akka.EventSource
{
    public sealed class ComputeMessageId
    {
        private readonly PersistentTopicInternalStats _stats;
        private readonly long _from;
        private long _to;
        private long? _max;

        public ComputeMessageId(PersistentTopicInternalStats stats, long @from, long to, long max)
        {
            _stats = stats;
            _from = @from;
            _max = max;
            _to = to;
        }

        public (long? Ledger, long? Entry, long? Max, long? To) GetFrom()
        {
            var ledgers = _stats.Ledgers.Where(x => x.Entries > 0);
            long? entries = 0L;
            long? ledgerId = 0L;
            long? entryId = 0L;
            foreach (var ledger in ledgers)
            {
                entries += ledger.Entries;
                if (_from > entries)
                    continue;
                var diff = entries - _from;
                var entry = (ledger.Entries - diff) - 1;
                ledgerId = ledger.LedgerId;
                entryId = entry;
                break;
            }

            if (ledgerId == 0L)
            {
                if (_stats.CurrentLedgerEntries > 0)
                {
                    entries += _stats.CurrentLedgerEntries;
                    var diff = entries - _from;
                    if (diff < 0)
                    {
                        //if we are replaying multiple topics, some topics will have less entry
                        return default;
                    }
                    var entry = (_stats.CurrentLedgerEntries - diff) - 1;//entry starts from zero
                    ledgerId = long.Parse(_stats.LastConfirmedEntry.Split(":")[0]);
                    entryId = entry;

                }
                else
                {
                    var lac = _stats.LastConfirmedEntry.Split(":");
                    entries += long.Parse(lac[1]);
                    var diff = entries - _from;
                    if (diff < 0)
                    {
                        //if we are replaying multiple topics, some topics will have less entry
                        return default;
                    }
                    var entry = (_stats.CurrentLedgerEntries - diff) - 1;
                    ledgerId = long.Parse(lac[0]);
                    entryId = entry;
                }
            }

            return (ledgerId,entryId, _max, _to);
        }
    }
}
