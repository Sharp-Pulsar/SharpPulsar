using System.Linq;
using PulsarAdmin.Models;

namespace SharpPulsar.Akka.EventSource
{
    public sealed class ComputeMessageId
    {
        private readonly PersistentTopicInternalStats _stats;
        private readonly long _from;
        private readonly long _to;
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
            var numberOfEntries = _stats.NumberOfEntries;
            var ledgers = _stats.Ledgers.Where(x => x.Entries > 0);
            long? track = 0L;
            long? ledgerId = 0L;
            long? entryId = 0L;
            foreach (var ledger in ledgers)
            {
                track += ledger.Entries;
                if (_from > track)
                    continue;
                var diff = track - _from;
                var entry = (ledger.Entries - diff);
                ledgerId = ledger.LedgerId;
                entryId = entry;
                track -= diff;
                break;
            }

            if (ledgerId == 0L)
            {
                if (_stats.CurrentLedgerEntries > 0)
                {
                    track += _stats.CurrentLedgerEntries;
                    var diff = track - _from;
                    var entry = (_stats.CurrentLedgerEntries - diff);//entry starts from zero
                    ledgerId = long.Parse(_stats.LastConfirmedEntry.Split(":")[0]);
                    entryId = entry;
                    track -= diff;
                }
                else
                {
                    var lac = _stats.LastConfirmedEntry.Split(":");
                    track += long.Parse(lac[1]);
                    var diff = track - _from;
                    var entry = (_stats.CurrentLedgerEntries - diff);
                    ledgerId = long.Parse(lac[0]);
                    entryId = entry;
                    track -= diff;
                }
            }

            var max = (numberOfEntries - track);
            var frotodiff = (_to - _from);
            if (_max > max)
                _max = (frotodiff > 0 && frotodiff < max) ? frotodiff : max;
            else if (frotodiff < _max)
                _max = frotodiff;
            return (ledgerId,entryId, _max, (_from + _max));
        }
    }
}
