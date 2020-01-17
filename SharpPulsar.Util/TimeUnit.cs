
namespace SharpPulsar.Util
{
    public static class TimeUnit
    {
        public static long ToNanos(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS.Convert(duration, timeUnit);
        }
        public static long Micros(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.MICROSECONDS.Convert(duration, timeUnit);
        }
        public static long ToMillis(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.Convert(duration, timeUnit);
        }
        public static long ToSecs(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.SECONDS.Convert(duration, timeUnit);
        }
        
        public static long ToMins(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.MINUTES.Convert(duration, timeUnit);
        }
        public static long ToHrs(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.HOURS.Convert(duration, timeUnit);
        }
        public static long ToDys(this BAMCIS.Util.Concurrent.TimeUnit timeUnit, int duration)
        {
            return BAMCIS.Util.Concurrent.TimeUnit.DAYS.Convert(duration, timeUnit);
        }
    }
}
