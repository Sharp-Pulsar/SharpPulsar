using Akka.Actor;
using Akka.Persistence.Query;
using System;
using System.IO;
using System.Text.Json;
using Akka.Configuration;
using Akka.Persistence.Pulsar.Query;
using Akka.Streams;
using Sourcerer.Observer;

namespace Sourcerer
{
    class Program
    {
        private static IObservable<EventEnvelope> _persistenceStream;
        private static IObservable<EventEnvelope> _tagStream;
        private static PulsarReadJournal _readJournal;
        private static ActorMaterializer _mat;
        static void Main(string[] args)
        {
            var config = File.ReadAllText("host.hocon");
            var actorSystem = ActorSystem.Create("SampleSystem", ConfigurationFactory.ParseString(config));
            _mat = ActorMaterializer.Create(actorSystem);
            _readJournal = PersistenceQuery.Get(actorSystem).ReadJournalFor<PulsarReadJournal>("akka.persistence.query.journal.pulsar");
            PersistenceIds();
            //CurrentPersistenceIds();
            //EventsByPersistenceId();
            CurrentEventsByPersistenceId();
            //EventsByTag();
            //CurrentEventsByTag();

            Console.ReadLine();
        }

        private static void PersistenceIds()
        {
            var persistenceIdsSource = _readJournal.PersistenceIds();
            var persistenceStream = new SourceObservable<string>(persistenceIdsSource, _mat);
            persistenceStream.Subscribe(e =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"PersistenceId '{e}' added");
                Console.ResetColor();
            });
        }
        private static void CurrentPersistenceIds()
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
        private static void CurrentEventsByPersistenceId()
        {
            var persistenceIdsSource1 = _readJournal.CurrentEventsByPersistenceId("utcreader-2", 1L, 320);
            var persistenceStream1 = new SourceObservable<EventEnvelope>(persistenceIdsSource1, _mat);
            persistenceStream1.Subscribe(e =>
            {
                Console.WriteLine($"{JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true })}");
            });
        }

        private static void EventsByPersistenceId()
        {
            var persistenceSource = _readJournal.EventsByPersistenceId("utcreader-2", 1L, long.MaxValue);
            _persistenceStream = new SourceObservable<EventEnvelope>(persistenceSource, _mat);
            _persistenceStream.Subscribe(e =>
            {
                Console.WriteLine($"{JsonSerializer.Serialize(e, new JsonSerializerOptions { WriteIndented = true })}");
            });
        }
        private static void EventsByTag()
        {
            var tagSource = _readJournal.EventsByTag("utc", Offset.Sequence(1L));
             _tagStream = new SourceObservable<EventEnvelope>(tagSource, _mat);
             _tagStream.Subscribe(e =>
             {
                 Console.WriteLine($"{JsonSerializer.Serialize(e, new JsonSerializerOptions{WriteIndented = true})}");
             });
        }
        private static void CurrentEventsByTag()
        {
            var tagSource = _readJournal.CurrentEventsByTag("utc", Offset.Sequence(1L));
             _tagStream = new SourceObservable<EventEnvelope>(tagSource, _mat);
             _tagStream.Subscribe(e =>
             {
                 Console.WriteLine($"{JsonSerializer.Serialize(e, new JsonSerializerOptions{WriteIndented = true})}");
             });
        }
    }
}
