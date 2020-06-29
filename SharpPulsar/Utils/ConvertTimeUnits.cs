namespace SharpPulsar.Utils
{
    //https://www.dotnetperls.com/convert-nanoseconds
    public static class ConvertTimeUnits
    {
        public static double ConvertMillisecondsToNanoseconds(double milliseconds)
        {
            return milliseconds * 1000000;
        }

        public static double ConvertMicrosecondsToNanoseconds(double microseconds)
        {
            return microseconds * 1000;
        }

        public static double ConvertMillisecondsToMicroseconds(double milliseconds)
        {
            return milliseconds * 1000;
        }

        public static double ConvertNanosecondsToMilliseconds(double nanoseconds)
        {
            return nanoseconds * 0.000001;
        }

        public static double ConvertMicrosecondsToMilliseconds(double microseconds)
        {
            return microseconds * 0.001;
        }

        public static double ConvertNanosecondsToMicroseconds(double nanoseconds)
        {
            return nanoseconds * 0.001;
        }
    }
}