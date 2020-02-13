using System;

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
namespace SharpPulsar.Sql
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.parseLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.addExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.multiplyExact;

	public sealed class IntervalDayTime
	{
		private const long MillisInSecond = 1000;
		private static readonly long MILLIS_IN_MINUTE = 60 * MillisInSecond;
		private static readonly long MILLIS_IN_HOUR = 60 * MILLIS_IN_MINUTE;
		private static readonly long MILLIS_IN_DAY = 24 * MILLIS_IN_HOUR;

		private const string LongMinValue = "-106751991167 07:12:55.808";

		private static readonly Pattern FORMAT = Pattern.compile(@"(\d+) (\d+):(\d+):(\d+).(\d+)");

		private IntervalDayTime()
		{
		}

		public static long ToMillis(long Day, long Hour, long Minute, long Second, long Millis)
		{
			try
			{
				long Value = Millis;
				Value = addExact(Value, multiplyExact(Day, MILLIS_IN_DAY));
				Value = addExact(Value, multiplyExact(Hour, MILLIS_IN_HOUR));
				Value = addExact(Value, multiplyExact(Minute, MILLIS_IN_MINUTE));
				Value = addExact(Value, multiplyExact(Second, MillisInSecond));
				return Value;
			}
			catch (ArithmeticException E)
			{
				throw new System.ArgumentException(E);
			}
		}

		public static string FormatMillis(long Millis)
		{
			if (Millis == long.MinValue)
			{
				return LongMinValue;
			}
			string Sign = "";
			if (Millis < 0)
			{
				Sign = "-";
				Millis = -Millis;
			}

			long Day = Millis / MILLIS_IN_DAY;
			Millis %= MILLIS_IN_DAY;
			long Hour = Millis / MILLIS_IN_HOUR;
			Millis %= MILLIS_IN_HOUR;
			long Minute = Millis / MILLIS_IN_MINUTE;
			Millis %= MILLIS_IN_MINUTE;
			long Second = Millis / MillisInSecond;
			Millis %= MillisInSecond;

			return format("%s%d %02d:%02d:%02d.%03d", Sign, Day, Hour, Minute, Second, Millis);
		}

		public static long ParseMillis(string Value)
		{
			if (Value.Equals(LongMinValue))
			{
				return long.MinValue;
			}

			long Signum = 1;
			if (Value.StartsWith("-", StringComparison.Ordinal))
			{
				Signum = -1;
				Value = Value.Substring(1);
			}

			Matcher Matcher = FORMAT.matcher(Value);
			if (!Matcher.matches())
			{
				throw new System.ArgumentException("Invalid day-time interval: " + Value);
			}

			long Days = parseLong(Matcher.group(1));
			long Hours = parseLong(Matcher.group(2));
			long Minutes = parseLong(Matcher.group(3));
			long Seconds = parseLong(Matcher.group(4));
			long Millis = parseLong(Matcher.group(5));

			return ToMillis(Days, Hours, Minutes, Seconds, Millis) * Signum;
		}
	}

}