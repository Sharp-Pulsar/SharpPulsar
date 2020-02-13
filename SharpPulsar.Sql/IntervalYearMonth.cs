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
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.addExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.multiplyExact;

	public sealed class IntervalYearMonth
	{
		private const string IntMinValue = "-178956970-8";

		private static readonly Pattern FORMAT = Pattern.compile(@"(\d+)-(\d+)");

		private IntervalYearMonth()
		{
		}

		public static int ToMonths(int Year, int Months)
		{
			try
			{
				return addExact(multiplyExact(Year, 12), Months);
			}
			catch (ArithmeticException E)
			{
				throw new System.ArgumentException(E);
			}
		}

		public static string FormatMonths(int Months)
		{
			if (Months == int.MinValue)
			{
				return IntMinValue;
			}

			string Sign = "";
			if (Months < 0)
			{
				Sign = "-";
				Months = -Months;
			}

			return format("%s%d-%d", Sign, Months / 12, Months % 12);
		}

		public static int ParseMonths(string Value)
		{
			if (Value.Equals(IntMinValue))
			{
				return int.MinValue;
			}

			int Signum = 1;
			if (Value.StartsWith("-", StringComparison.Ordinal))
			{
				Signum = -1;
				Value = Value.Substring(1);
			}

			Matcher Matcher = FORMAT.matcher(Value);
			if (!Matcher.matches())
			{
				throw new System.ArgumentException("Invalid year-month interval: " + Value);
			}

			int Years = parseInt(Matcher.group(1));
			int Months = parseInt(Matcher.group(2));

			return ToMonths(Years, Months) * Signum;
		}
	}

}