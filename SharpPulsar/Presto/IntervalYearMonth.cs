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
	
	public sealed class IntervalYearMonth
	{
		private const string IntMinValue = "-178956970-8";

		private static readonly Regex Format = new Regex(@"(\d+)-(\d+)");

		private IntervalYearMonth()
		{
		}

		public static int ToMonths(int year, int months)
		{
			try
            {
                return (int)Decimal.Add(Decimal.Multiply(year, 12), months);
			}
			catch (ArithmeticException e)
			{
				throw new System.ArgumentException(e.Message);
			}
		}

		public static string FormatMonths(int months)
		{
			if (months == int.MinValue)
			{
				return IntMinValue;
			}

			string sign = "";
			if (months < 0)
			{
				sign = "-";
				months = -months;
			}

			return $"{sign}{months / 12}-{months % 12}";
		}

		public static int ParseMonths(string value)
		{
			if (value.Equals(IntMinValue))
			{
				return int.MinValue;
			}

			int signum = 1;
			if (value.StartsWith("-", StringComparison.Ordinal))
			{
				signum = -1;
				value = value.Substring(1);
			}

			var matcher = Format.Match(value);
			if (!matcher.Success)
			{
				throw new System.ArgumentException("Invalid year-month interval: " + value);
			}

			int years = int.Parse(matcher.Groups[1].Value);
            int months = int.Parse(matcher.Groups[2].Value);

			return ToMonths(years, months) * signum;
		}
	}

}