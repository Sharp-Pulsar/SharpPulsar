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
                var entry = (ledger.Entries - diff);
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
                    var entry = (_stats.CurrentLedgerEntries - diff);//entry starts from zero
                    ledgerId = long.Parse(_stats.LastConfirmedEntry.Split(":")[0]);
                    entryId = entry;
                }
                else
                {
                    var lac = _stats.LastConfirmedEntry.Split(":");
                    entries += long.Parse(lac[1]);
                    var diff = entries - _from;
                    var entry = (_stats.CurrentLedgerEntries - diff);
                    ledgerId = long.Parse(lac[0]);
                    entryId = entry;
                }
            }

            return (ledgerId,entryId, _max, _to);
        }
    }
}
