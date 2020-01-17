
namespace SharpPulsar.Util
{
    public static class TimeUnit
    {
        public static long ToNanoseconds(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS.Convert(duration, timeUnit);
        }
        public static long Microseconds(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.MICROSECONDS.Convert(duration, timeUnit);
        }
        public static long ToMilliseconds(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.Convert(duration, timeUnit);
        }
        public static long ToSeconds(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.SECONDS.Convert(duration, timeUnit);
        }
        
        public static long ToMinutes(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.MINUTES.Convert(duration, timeUnit);
        }
        public static long ToHours(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.HOURS.Convert(duration, timeUnit);
        }
        public static long ToDays(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.DAYS.Convert(duration, timeUnit);
        }
    }
}
