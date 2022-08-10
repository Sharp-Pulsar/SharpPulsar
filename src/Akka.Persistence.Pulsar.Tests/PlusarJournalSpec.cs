#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PlusarJournalSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using Akka.Persistence.TCK.Journal;
using Xunit.Abstractions;

namespace Akka.Persistence.Pulsar.Tests
{
    public class PlusarJournalSpec : JournalSpec
    {
        public PlusarJournalSpec(ITestOutputHelper output) : base(typeof(PulsarJournal), "PlusarJournalSpec", output)
        {
            Initialize();
        }
    }
}