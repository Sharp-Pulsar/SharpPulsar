using System;

namespace SharpPulsar.Cache
{
    public class CacheItem<T>
    {
        public CacheItem(T value)
        {
            Value = value;
            Size = null;
        }
        public T Value { get; }
        internal DateTimeOffset AccessedTime { get; set; }
        internal TimeSpan ExpiresAfter { get; set; }
        internal bool Accessed { get; set; } = false;
        internal long? Size { get; set; }
    }
}
