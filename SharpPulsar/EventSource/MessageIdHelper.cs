using System;
using System.Linq;
using SharpPulsar.Admin.Admin.Models;

namespace SharpPulsar.EventSource
{
    public sealed class MessageIdHelper
    {
        public static (long Ledger, long Entry, long Index) Calculate( long start, PersistentTopicInternalStats stats)
        {
            var ledgers = stats.Ledgers.Where(x => x.Entries > 0);
            long? entries = 0L;
            long? ledgerId = 0L;
            long? entryId = 0L;
            foreach (var ledger in ledgers)
            {
                entries += ledger.Entries;
                if (start > entries)
                    continue;
                var diff = Math.Max(0L, entries.Value - start);
                entries -= diff;
                var entry = Math.Max(0L, (ledger.Entries.Value - diff) - 1);
                ledgerId = ledger.LedgerId;
                entryId = entry;
                break;
            }

            if (ledgerId == 0L)
            {
                if (stats.CurrentLedgerEntries > 0)
                {
                    entries += stats.CurrentLedgerEntries;
                    var diff = Math.Max(0L, entries.Value - start);
                    entries -= diff;
                    var entry = Math.Max(0L, (stats.CurrentLedgerEntries - diff).Value - 1);
                    ledgerId = long.Parse(stats.LastConfirmedEntry.Split(":")[0]);
                    entryId = entry;

                }
                else
                {
                    var lac = stats.LastConfirmedEntry.Split(":");
                    entries += long.Parse(lac[1] + 1);
                    var diff = Math.Max(0L, entries.Value - start);
                    entries -= diff;
                    var entry = Math.Max(0L, (stats.CurrentLedgerEntries - diff).Value - 1);
                    ledgerId = long.Parse(lac[0]);
                    entryId = entry;
                }
            }

            return (ledgerId.Value, entryId.Value, Math.Max(0, entries.Value));
        }
        public static (long Ledger, long Entry, long Index) NextFlow(PersistentTopicInternalStats stats)
        {
            var ids = stats.LastConfirmedEntry.Split(":");
            return (long.Parse(ids[0]), long.Parse(ids[1]), stats.NumberOfEntries.Value);
        }
    }
}
