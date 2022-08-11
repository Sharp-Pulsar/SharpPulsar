//-----------------------------------------------------------------------
// <copyright file="SnapshotEntry.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

namespace Akka.Persistence.Pulsar.Snapshot
{
    /// <summary>
    /// Class used for storing a Snapshot as BsonDocument
    /// </summary>
    public class SnapshotEntry
    {
        public string Id { get; set; }

        public string PersistenceId { get; set; }

        public long SequenceNr { get; set; }

        public long Timestamp { get; set; }

        public string Snapshot { get; set; }
    }
}