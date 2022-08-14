using System;
using System.Collections.Generic;
using System.Text.Json;
using Akka.Actor;
using Akka.Persistence;
using Sample.Command;
using Sample.Event;

namespace Producer.Actors
{
    public class SamplePersistentActor : ReceivePersistentActor
    {
        private SampleActorState _state;
        private List<ReadSystemCurrentTimeUtc> _stash;
        public override string PersistenceId { get; }
        private int _snapCount = 0;

        public SamplePersistentActor()
        {
            _state = new SampleActorState();
            _stash = new List<ReadSystemCurrentTimeUtc>();
            Become(Recover);
            PersistenceId = Context.Self.Path.Name;
            //
        }

        private void Recover()
        {
            Recover<IEvent>(c =>
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine("Recovered Event!!");
                Console.ResetColor();
                switch (c)
                {
                    case SystemCurrentTimeUtcRead time:
                    {
                        _state.HandledCount++;
                    }
                        break;
                }
            });
            
            Recover<SnapshotOffer>(s =>
            {
                if (s.Snapshot is SampleActorState state)
                {
                    _state = state;
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"Snapshot Offered: {_state.HandledCount}");
                    Console.ResetColor();
                }
            });
            Recover<RecoveryCompleted>(r =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Recovery completed");
                Console.ResetColor();
                Become(Active);
            });
            Command<ReadSystemCurrentTimeUtc>(o =>
            {
                _stash.Add(o);
            });
            
        }

        private void Active()
        {
            Command<ReadSystemCurrentTimeUtc>(c =>
            {
                Console.WriteLine("Command Received");
                _state.HandledCount++;
                var readTimeEvent = new SystemCurrentTimeUtcRead(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                Persist(readTimeEvent, @event =>
                {
                    _snapCount++;
                    if (_snapCount >= 5)
                    {
                        SaveSnapshot(_state);
                        _snapCount = 0;
                    }
                });
            });
            Command<SaveSnapshotSuccess>(s =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(JsonSerializer.Serialize(s.Metadata, new JsonSerializerOptions{WriteIndented = true}));
                Console.ResetColor();
            });
            _stash.ForEach(r=> Self.Tell(r));
        }
        protected override void Unhandled(object message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"unhandled => {message.GetType().FullName}");
            Console.ResetColor();
        }

        public static Props Prop()
        {
            return Props.Create(() => new SamplePersistentActor());
        }
    }
    public sealed class SampleActorState
    {
        public int HandledCount { get; set; }
    }
}
