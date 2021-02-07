using System;
using System.Runtime.InteropServices;

namespace SharpPulsar.Cache
{
    public class CacheItem<T>
    {
        public CacheItem(T value)
        {
            Value = value;
            Size = Marshal.SizeOf(value);
        }
        public T Value { get; }
        internal DateTimeOffset AccessedTime { get; set; }
        internal TimeSpan ExpiresAfter { get; set; }
        internal bool Accessed { get; set; } = false;
        internal long? Size { get; set; }
    }
}
