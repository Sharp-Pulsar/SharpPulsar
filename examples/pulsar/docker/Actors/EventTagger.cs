using System.Collections.Immutable;
using Akka.Persistence.Journal;
using Sample.Event;

namespace Producer.Actors
{
    public sealed class EventTagger: IWriteEventAdapter
    {
        public string Manifest(object evt)
        {
            return string.Empty;
        }

        public object ToJournal(object evt)
        {
            switch (evt)
            {
                case SystemCurrentTimeUtcRead utc:
                    return new Tagged(evt, ImmutableHashSet<string>.Empty.Add("utc"));
                default: return evt;
            }
        }
    }
}
