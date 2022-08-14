using System.Collections.Immutable;
using Akka.Persistence.Journal;

namespace Sample.Event
{
    public sealed class EventTagger: IWriteEventAdapter
    {
        public string Manifest(object evt)
        {
            return string.Empty;
        }

        public object ToJournal(object evt)
        {
            return new Tagged(evt, ImmutableHashSet<string>.Empty.Add("utc"));
        }
    }
}
