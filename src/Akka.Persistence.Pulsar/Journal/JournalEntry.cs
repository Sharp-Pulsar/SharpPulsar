﻿//-----------------------------------------------------------------------
// <copyright file="JournalEntry.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

namespace Akka.Persistence.Pulsar.Journal
{
    /// <summary>
    /// Class used for storing intermediate result of the <see cref="IPersistentRepresentation"/>
    /// </summary>
    public class JournalEntry
    {
        public string Id { get; set; }
        public string PersistenceId { get; set; }
        public long SequenceNr { get; set; }

        public bool IsDeleted { get; set; }

        public byte[] Payload { get; set; }

        public long Ordering { get; set; }
        public long PublishTime { get; set; }
        public long EventTime { get; set; }
        public int Partition { get; set; }
        public string Tags { get; set; }
        public string MessageId { get; set; }
        public string Key { get; set; }
        public string Properties { get; set; }
    }
}
