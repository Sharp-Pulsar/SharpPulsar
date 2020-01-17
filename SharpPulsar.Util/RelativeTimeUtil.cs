/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace org.apache.pulsar.common.util
{

	using UtilityClass = lombok.experimental.UtilityClass;

	/// <summary>
	/// Parser for relative time.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UtilityClass public class RelativeTimeUtil
	public class RelativeTimeUtil
	{
		public static long parseRelativeTimeInSeconds(string relativeTime)
		{
			if (relativeTime.Length == 0)
			{
				throw new System.ArgumentException("exipiry time cannot be empty");
			}

			int lastIndex = relativeTime.Length - 1;
			char lastChar = relativeTime[lastIndex];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final char timeUnit;
			char timeUnit;

			if (!Character.isAlphabetic(lastChar))
			{
				// No unit specified, assume seconds
				timeUnit = 's';
				lastIndex = relativeTime.Length;
			}
			else
			{
				timeUnit = char.ToLower(lastChar);
			}

			long duration = long.Parse(relativeTime.Substring(0, lastIndex));

			switch (timeUnit)
			{
			case 's':
				return duration;
			case 'm':
				return TimeUnit.MINUTES.toSeconds(duration);
			case 'h':
				return TimeUnit.HOURS.toSeconds(duration);
			case 'd':
				return TimeUnit.DAYS.toSeconds(duration);
			case 'w':
				return 7 * TimeUnit.DAYS.toSeconds(duration);
			// No unit for months
			case 'y':
				return 365 * TimeUnit.DAYS.toSeconds(duration);
			default:
				throw new System.ArgumentException("Invalid time unit '" + lastChar + "'");
			}
		}
	}

}