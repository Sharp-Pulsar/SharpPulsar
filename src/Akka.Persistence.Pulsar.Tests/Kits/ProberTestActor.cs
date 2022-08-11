//-----------------------------------------------------------------------
// <copyright file="ProberTestActor.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using Akka.Persistence.Journal;

namespace Akka.Persistence.Pulsar.Tests.Kits
{
    internal class ProberTestActor : ReceivePersistentActor
    {
        public static Props Prop(string persistenceId)
        {
            return Props.Create(() => new ProberTestActor(persistenceId));
        }

        public sealed class DeleteCommand
        {
            public DeleteCommand(long toSequenceNr)
            {
                ToSequenceNr = toSequenceNr;
            }

            public long ToSequenceNr { get; }
        }

        public ProberTestActor(string persistenceId)
        {
            PersistenceId = persistenceId;
            Command<string>(c =>
            {
                var sender = Sender;
                Persist(c, e => sender.Tell($"{e}-done"));
            });
        }

        public override string PersistenceId { get; }

        protected Receive WhileDeleting(IActorRef originalSender)
        {
            return message =>
            {
                switch (message)
                {
                    case DeleteMessagesSuccess success:
                        originalSender.Tell($"{success.ToSequenceNr}-deleted");
                        Become(OnCommand);
                        Stash.UnstashAll();
                        break;
                    case DeleteMessagesFailure failure:
                        originalSender.Tell($"{failure.ToSequenceNr}-deleted-failed");
                        Become(OnCommand);
                        Stash.UnstashAll();
                        break;
                    default:
                        Stash.Stash();
                        break;
                }

                return true;
            };
        }
    }

    public class ColorFruitTagger : IWriteEventAdapter
    {
        public static IImmutableSet<string> Colors { get; } = ImmutableHashSet.Create("green", "black", "blue");
        public static IImmutableSet<string> Fruits { get; } = ImmutableHashSet.Create("apple", "banana");

        public string Manifest(object evt) => string.Empty;

        public object ToJournal(object evt)
        {
            if (evt is string s)
            {
                var colorTags = Colors.Aggregate(ImmutableHashSet<string>.Empty, (acc, color) => s.Contains(color) ? acc.Add(color) : acc);
                var fruitTags = Fruits.Aggregate(ImmutableHashSet<string>.Empty, (acc, color) => s.Contains(color) ? acc.Add(color) : acc);
                var tags = colorTags.Union(fruitTags);
                return tags.IsEmpty
                    ? evt
                    : new Tagged(evt, tags);
            }

            return evt;
        }
    }
}
