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
namespace SharpPulsar.Util
{


	/// <summary>
	/// Parser for relative time.
	/// </summary>
	public class RelativeTimeUtil
	{
		public static long ParseRelativeTimeInSeconds(string relativeTime)
		{
			if (relativeTime.Length == 0)
			{
				throw new System.ArgumentException("exipiry time cannot be empty");
			}

			int lastIndex = relativeTime.Length - 1;
			char lastChar = relativeTime[lastIndex];
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
				return BAMCIS.Util.Concurrent.TimeUnit.MINUTES.ToSeconds(duration);
			case 'h':
				return BAMCIS.Util.Concurrent.TimeUnit.HOURS.ToSeconds(duration);
			case 'd':
				return BAMCIS.Util.Concurrent.TimeUnit.DAYS.ToSeconds(duration);
			case 'w':
				return 7 * BAMCIS.Util.Concurrent.TimeUnit.DAYS.ToSeconds(duration);
			// No unit for months
			case 'y':
				return 365 * BAMCIS.Util.Concurrent.TimeUnit.DAYS.ToSeconds(duration);
			default:
				throw new System.ArgumentException("Invalid time unit '" + lastChar + "'");
			}
		}
	}

}