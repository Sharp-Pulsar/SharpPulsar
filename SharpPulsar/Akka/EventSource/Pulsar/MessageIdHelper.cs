using System.Linq;
using PulsarAdmin.Models;

namespace SharpPulsar.Akka.EventSource.Pulsar
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
                var diff = entries - start;
                entries += diff;
                var entry = (ledger.Entries - diff) - 1;
                ledgerId = ledger.LedgerId;
                entryId = entry;
                break;
            }

            if (ledgerId == 0L)
            {
                if (stats.CurrentLedgerEntries > 0)
                {
                    entries += stats.CurrentLedgerEntries;
                    var diff = entries - start;
                    entries += diff;
                    var entry = diff < 0 ? (stats.CurrentLedgerEntries + diff) - 1 : (stats.CurrentLedgerEntries - diff) - 1;
                    ledgerId = long.Parse(stats.LastConfirmedEntry.Split(":")[0]);
                    entryId = entry;

                }
                else
                {
                    var lac = stats.LastConfirmedEntry.Split(":");
                    entries += long.Parse(lac[1] + 1);
                    var diff = entries - start;
                    entries += diff;
                    var entry = diff < 0? (stats.CurrentLedgerEntries + diff) - 1 : (stats.CurrentLedgerEntries - diff) - 1;
                    ledgerId = long.Parse(lac[0]);
                    entryId = entry;
                }
            }

            return (ledgerId.Value, entryId.Value, entries.Value);
        }
    }
}
