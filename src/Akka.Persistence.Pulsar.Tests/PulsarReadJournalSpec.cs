
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence.Pulsar.Query;
using Akka.Persistence.Pulsar.Tests.Observer;
using Akka.Persistence.Query;
using Akka.Streams;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.Pulsar.Tests
{
    [Collection("PulsarReadJournalSpec")]
    public class PulsarReadJournalSpec
    {
        private static IObservable<EventEnvelope> _persistenceStream;
        private static IObservable<EventEnvelope> _tagStream;
        private static PulsarReadJournal _readJournal;
        private static ActorMaterializer _mat;
        private static readonly Config Config = ConfigurationFactory.ParseString(@"
            akka.persistence.journal.plugin = ""akka.persistence.journal.pulsar""
            akka.test.single-expect-default = 180s
        ").WithFallback(PulsarPersistence.DefaultConfiguration());

        private long _timeout = 30_000;
        private readonly ITestOutputHelper _output;

        public PulsarReadJournalSpec(ITestOutputHelper output) 
        {
            _output = output;
            var actorSystem = ActorSystem.Create("PulsarSystem", Config);
            _mat = ActorMaterializer.Create(actorSystem);
            _readJournal = PersistenceQuery.Get(actorSystem).ReadJournalFor<PulsarReadJournal>("akka.persistence.query.journal.pulsar");

        }
        [Fact]
        public void PersistenceIds()
        {
            var values = new List<string>();
            var completed = false;
            var persistenceIdsSource = _readJournal.PersistenceIds();
            var persistenceStream = new SourceObservable<string>(persistenceIdsSource, _mat);
            persistenceStream.Subscribe(
                value =>
                {
                    _output.WriteLine($"onNext: {value}");
                    _output.WriteLine($"PersistenceId '{value}' added");
                    Assert.False(completed);

                    values.Add(value);
                },
                exception => 
                {
                    Assert.False(true);
                },
                () => 
                {
                    _output.WriteLine($"onCompleted");
                    Assert.False(completed);

                    completed = true;
                });

            // Assert
           // Assert.Equal(new List<int>() { 0, 1, 2, }, values);
            Assert.True(completed); 
        }
        [Fact]
        public void CurrentPersistenceIds()
        {
            var persistenceIdsSource1 = _readJournal.CurrentPersistenceIds();
            var persistenceStream1 = new SourceObservable<string>(persistenceIdsSource1, _mat);
            persistenceStream1.Subscribe(e =>
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"PersistenceId '{e}' added");
                Console.ResetColor();
            });
        }
        [Fact]
        public async Task CurrentEventsByPersistenceId()
        {
            var persistenceIdsSource1 = _readJournal.CurrentEventsByPersistenceId("utcreader-2", 1L, 320);
            var persistenceStream1 = new SourceObservable<EventEnvelope>(persistenceIdsSource1, _mat);
            var completed = false;
            persistenceStream1.Subscribe(
                value =>
                {
                    _output.WriteLine($"onNext: {value}");
                    _output.WriteLine($"PersistenceId '{value}' added");
                    Assert.False(completed);

                    _output.WriteLine($"{JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true })}");
                },
                exception =>
                {
                    Assert.False(true);
                },
                () =>
                {
                    _output.WriteLine($"onCompleted");
                    Assert.False(completed);

                    completed = true;
                });
            await Task.Delay(30000);
            Assert.True(completed);
        }

        [Fact]
        public void EventsByPersistenceId()
        {
            var completed = false;
            var persistenceSource = _readJournal.EventsByPersistenceId("utcreader-2", 1L, long.MaxValue);
            _persistenceStream = new SourceObservable<EventEnvelope>(persistenceSource, _mat);
            _persistenceStream.Subscribe(
                value =>
                {
                    _output.WriteLine($"onNext: {value}");
                    _output.WriteLine($"PersistenceId '{value}' added");
                    Assert.False(completed);

                    _output.WriteLine($"{JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true })}");
                },
                exception =>
                {
                    Assert.False(true);
                },
                () =>
                {
                    _output.WriteLine($"onCompleted");
                    Assert.False(completed);

                    completed = true;
                });
            Assert.True(completed);
        }
        [Fact]
        public void EventsByTag()
        {
            var completed = false;
            var tagSource = _readJournal.EventsByTag("utc", Offset.Sequence(1L));
            _tagStream = new SourceObservable<EventEnvelope>(tagSource, _mat);
            _tagStream.Subscribe(
                value =>
                {
                    _output.WriteLine($"onNext: {value}");
                    _output.WriteLine($"PersistenceId '{value}' added");
                    Assert.False(completed);

                    _output.WriteLine($"{JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true })}");
                },
                exception =>
                {
                    Assert.False(true);
                },
                () =>
                {
                    _output.WriteLine($"onCompleted");
                    Assert.False(completed);

                    completed = true;
                });
            Assert.True(completed);
        }
        [Fact]
        public void CurrentEventsByTag()
        {
            var completed = false;
            var tagSource = _readJournal.CurrentEventsByTag("utc", Offset.Sequence(1L));
            _tagStream = new SourceObservable<EventEnvelope>(tagSource, _mat);
            _tagStream.Subscribe(
                value =>
                {
                    _output.WriteLine($"onNext: {value}");
                    _output.WriteLine($"PersistenceId '{value}' added");
                    Assert.False(completed);

                    _output.WriteLine($"{JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true })}");
                },
                exception =>
                {
                    Assert.False(true);
                },
                () =>
                {
                    _output.WriteLine($"onCompleted");
                    Assert.False(completed);

                    completed = true;
                });
            Assert.True(completed);
        }
    }
}
