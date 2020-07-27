using System;
using SharpPulsar.Presto;
using Xunit;

namespace SharpPulsar.Test.Presto
{
    public class TestIntervalYearMonth
    {
        [Fact]
        public void TestFormat()
        {
            AssertMonths(0, "0-0");
            AssertMonths(IntervalYearMonth.ToMonths(0, 0), "0-0");

            AssertMonths(3, "0-3");
            AssertMonths(-3, "-0-3");
            AssertMonths(IntervalYearMonth.ToMonths(0, 3), "0-3");
            AssertMonths(IntervalYearMonth.ToMonths(0, -3), "-0-3");

            AssertMonths(28, "2-4");
            AssertMonths(-28, "-2-4");

            AssertMonths(IntervalYearMonth.ToMonths(2, 4), "2-4");
            AssertMonths(IntervalYearMonth.ToMonths(-2, -4), "-2-4");

            AssertMonths(int.MaxValue, "178956970-7");
            AssertMonths(int.MinValue + 1, "-178956970-7");
            AssertMonths(int.MaxValue, "-178956970-8");
        }

        private void AssertMonths(int months, string formatted)
        {
            Assert.Equal(IntervalYearMonth.FormatMonths(months), formatted);
            Assert.Equal(IntervalYearMonth.ParseMonths(formatted), months);
        }
        [Fact]
        public void TestMaxYears()
        {
            var years = int.MaxValue / 12;
            Assert.Equal(IntervalYearMonth.ToMonths(years, 0), years * 12);
        }
        [Fact]
        public void TestOverflow()
        {
            var days = (int.MaxValue / 12) + 1;
            Assert.Throws<ArgumentException>(() => IntervalYearMonth.ToMonths(days, 0));

        }
    }
}
