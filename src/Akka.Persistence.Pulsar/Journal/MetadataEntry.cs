//-----------------------------------------------------------------------
// <copyright file="MetadataEntry.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

namespace Akka.Persistence.Pulsar.Journal
{
    public class MetadataEntry
    {
        public string Id { get; set; }
        public string PersistenceId { get; set; }
        public long SequenceNr { get; set; }
        public long Ledger { get; set; }
        public long EntryNr { get; set; }
    }
}
