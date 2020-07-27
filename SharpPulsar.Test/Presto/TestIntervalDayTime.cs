using System;
using SharpPulsar.Presto;
using Xunit;

namespace SharpPulsar.Test.Presto
{
    public class TestIntervalDayTime
    {
        [Fact]
        public void TestFormat()
        {
            AssertMillis(0, "0 00:00:00.000");
            AssertMillis(1, "0 00:00:00.001");
            AssertMillis(-1, "-0 00:00:00.001");

            AssertMillis(IntervalDayTime.ToMillis(12, 13, 45, 56, 789), "12 13:45:56.789");
            AssertMillis(IntervalDayTime.ToMillis(-12, -13, -45, -56, -789), "-12 13:45:56.789");

            AssertMillis(long.MaxValue, "106751991167 07:12:55.807");
            AssertMillis(long.MinValue + 1, "-106751991167 07:12:55.807");
            AssertMillis(long.MinValue, "-106751991167 07:12:55.808");
        }

        private static void AssertMillis(long millis, string formatted)
        {
            Assert.Equal(IntervalDayTime.FormatMillis(millis), formatted);
            Assert.Equal(IntervalDayTime.ParseMillis(formatted), millis);
        }
        [Fact]
        public void TextMaxDays()
        {
            long days = long.MaxValue / (long)TimeSpan.FromDays(1).TotalMilliseconds;
            Assert.Equal(IntervalDayTime.ToMillis(days, 0, 0, 0, 0), days);
        }
        [Fact]
        public void TestOverflow()
        {
            long days = (long.MaxValue / (long)TimeSpan.FromDays(1).TotalMilliseconds) + 1;
            Assert.Throws<ArgumentException>(() => IntervalDayTime.ToMillis(days, 0, 0, 0, 0));
        }
    }
}
