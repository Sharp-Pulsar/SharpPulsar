#region copyright
// -----------------------------------------------------------------------
//  <copyright file="PlusarJournalSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using Akka.Configuration;
using Akka.Persistence.Pulsar.Journal;
using Akka.Persistence.TCK.Journal;
using Xunit.Abstractions;

namespace Akka.Persistence.Pulsar.Tests
{
    public class PlusarJournalSpec : JournalSpec
    {
        private static readonly Config SpecConfig = ConfigurationFactory.ParseString(@"
            akka.persistence.journal.plugin = ""akka.persistence.journal.pulsar""
            akka.test.single-expect-default = 180s
        ").WithFallback(PulsarPersistence.DefaultConfiguration());
        public PlusarJournalSpec(ITestOutputHelper output) : base(FromConfig(SpecConfig).WithFallback(Config), "JournalSpec", output)
        {
            PulsarPersistence.Get(Sys);
            Initialize();
        }
        protected static readonly Config Config =
            ConfigurationFactory.ParseString(@"akka.persistence.publish-plugin-commands = on
            akka.actor{
                serializers{
                    persistence-tck-test=""Akka.Persistence.TCK.Serialization.TestSerializer,Akka.Persistence.TCK""
                }
                serialization-bindings {
                    ""Akka.Persistence.TCK.Serialization.TestPayload,Akka.Persistence.TCK"" = persistence-tck-test
                }
            }");

        private static readonly string _specConfigTemplate = @"
            akka.persistence {{
                publish-plugin-commands = on
                journal {{
                    plugin = ""akka.persistence.journal.testspec""
                    testspec {{
                        class = ""{0}""
                        plugin-dispatcher = ""akka.actor.default-dispatcher""
                    }}
                }}
            }}
        ";
    }
}