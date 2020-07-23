using System;
using System.Text.RegularExpressions;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace SharpPulsar.Presto
{

	public sealed class IntervalDayTime
	{
		private const long MillisInSecond = 1000;
		private static readonly long MillisInMinute = 60 * MillisInSecond;
		private static readonly long MillisInHour = 60 * MillisInMinute;
		private static readonly long MillisInDay = 24 * MillisInHour;

		private const string LongMinValue = "-106751991167 07:12:55.808";

		private static readonly Regex Format = new Regex(@"(\d+) (\d+):(\d+):(\d+).(\d+)");

		private IntervalDayTime()
		{
		}

		public static long ToMillis(long day, long hour, long minute, long second, long millis)
		{
			try
			{
				var value = millis;
				value = (int)decimal.Add(value, decimal.Multiply(day, MillisInDay));
				value = (int)decimal.Add(value, decimal.Multiply(hour, MillisInHour));
				value = (int)decimal.Add(value, decimal.Multiply(minute, MillisInMinute));
				value = (int)decimal.Add(value, decimal.Multiply(second, MillisInSecond));
				return value;
			}
			catch (ArithmeticException e)
			{
				throw new System.ArgumentException(e.Message);
			}
		}

		public static string FormatMillis(long millis)
		{
			if (millis == long.MinValue)
			{
				return LongMinValue;
			}
			var sign = "";
			if (millis < 0)
			{
				sign = "-";
				millis = -millis;
			}

			var day = millis / MillisInDay;
			millis %= MillisInDay;
			var hour = millis / MillisInHour;
			millis %= MillisInHour;
			var minute = millis / MillisInMinute;
			millis %= MillisInMinute;
			var second = millis / MillisInSecond;
			millis %= MillisInSecond;

            return $"{sign}{day} {hour}:{minute}:{second}.{millis}";
		}

		public static long ParseMillis(string value)
		{
			if (value.Equals(LongMinValue))
			{
				return long.MinValue;
			}

			long signum = 1;
			if (value.StartsWith("-", StringComparison.Ordinal))
			{
				signum = -1;
				value = value.Substring(1);
			}

			var matcher = Format.Match(value);
			if (!matcher.Success)
			{
				throw new System.ArgumentException("Invalid day-time interval: " + value);
			}

			var days = long.Parse(matcher.Groups[1].Value);
			var hours = long.Parse(matcher.Groups[2].Value);
			var minutes = long.Parse(matcher.Groups[3].Value);
			var seconds = long.Parse(matcher.Groups[4].Value);
			var millis = long.Parse(matcher.Groups[5].Value);

			return ToMillis(days, hours, minutes, seconds, millis) * signum;
		}
	}

}