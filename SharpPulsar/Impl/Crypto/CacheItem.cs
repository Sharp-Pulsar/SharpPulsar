using System;

namespace SharpPulsar.Impl.Crypto
{
    public class CacheItem<T>
    {
        public CacheItem(T value)
        {
            Value = value;
        }
        public T Value { get; }
        internal DateTimeOffset AccessedTime { get; set; }
        internal TimeSpan ExpiresAfter { get; set; }
        internal bool Accessed { get; set; } = false;
    }
}
