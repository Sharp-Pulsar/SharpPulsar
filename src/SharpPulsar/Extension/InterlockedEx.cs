using System.Runtime.CompilerServices;
using System.Threading;

namespace SharpPulsar.Extension
{//https://askcodes.net/coding/interlocked-compareexchange-with-enum
    public static class InterlockedEx
    {
        public static T CompareExchange<T>(ref T location, T value, T comparand)
            where T : struct
        {
            // Barrier 1 (fence conditional check)      
            Interlocked.MemoryBarrier();
            if (Unsafe.AreSame(ref location, ref comparand))
            {
                // Barrier 2 (fence assignment)
                Interlocked.MemoryBarrier();
                location = value;

                // Barrier 3 ("freshness" through assignment)
                Interlocked.MemoryBarrier();
            }

            return location;
        }

    }
}
