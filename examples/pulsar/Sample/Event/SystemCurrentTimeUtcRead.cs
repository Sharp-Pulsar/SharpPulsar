using System;

namespace Sample.Event
{
    public class SystemCurrentTimeUtcRead : IEvent
    {
        public long EventTime => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public readonly long CurrentTime;
        public SystemCurrentTimeUtcRead(long time)
        {
            CurrentTime = time;
        }
    }
}
