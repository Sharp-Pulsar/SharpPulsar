using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using DotNetty.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharpPulsar.Sql.Precondition;

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
namespace SharpPulsar.Sql.Facebook.Type
{
    // <summary>
    /// From com.facebook.presto.spi.type.TimeZoneKey.java
    /// </summary>
    [JsonConverter(typeof(TimeZoneKeyConverter))]
	public  class TimeZoneKey
	{
		public static readonly TimeZoneKey UtcKey = new TimeZoneKey("UTC", (short) 0);
		public static readonly short MaxTimeZoneKey;
		public static readonly IDictionary<string, TimeZoneKey> ZoneIdToKey;

        private static readonly TimeZoneKey[] _timeZoneKeys;

		private const short OffsetTimeZoneMin = -14 * 60;
		private const short OffsetTimeZoneMax = 14 * 60;
		private static readonly TimeZoneKey[] OffsetTimeZoneKeys = new TimeZoneKey[OffsetTimeZoneMax - OffsetTimeZoneMin + 1];

		static TimeZoneKey()
		{
			try
			{
                IDictionary<string, TimeZoneKey> zoneIdToKey = new SortedDictionary<string, TimeZoneKey>();
                ZoneIdToKey[UtcKey.Id.ToLower(CultureInfo.GetCultureInfo("en-US"))] = UtcKey;
                short maxZoneKey = 0;
				var lines = File.ReadLines("zone-index.properties");
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var l = line.Trim();
                    var sl = l.Split(' ');
                    var key = Convert.ToInt16(sl[0].Trim());
					if(key == 0) continue;
                    maxZoneKey = Math.Max(maxZoneKey, key);
					var tzk = new TimeZoneKey(sl[1].Trim(), Convert.ToInt16(sl[0].Trim()));
                    zoneIdToKey[sl[0].ToLower(CultureInfo.GetCultureInfo("en-US"))] = tzk;
                }
				
                MaxTimeZoneKey = maxZoneKey;
                ZoneIdToKey = new Dictionary<string, TimeZoneKey>(zoneIdToKey);
                TimeZoneKeys = new HashSet<TimeZoneKey>(zoneIdToKey.Values);

                for (var offset = OffsetTimeZoneMin; offset <= OffsetTimeZoneMax; offset++)
                {
                    if (offset == 0)
                    {
                        continue;
                    }
                    var zoneId = ZoneIdForOffset(offset);
                    var zoneKey = ZoneIdToKey[zoneId];
                    OffsetTimeZoneKeys[offset - OffsetTimeZoneMin] = zoneKey;
                }

			}
			catch (IOException e)
			{
                Log.LogError("Error loading time zone index file ");
                throw;
            }
		}

		public static ISet<TimeZoneKey> TimeZoneKeys { get; }

        public static TimeZoneKey GetTimeZoneKey(short timeZoneKey)
		{
			CheckArgument(timeZoneKey < TimeZoneKeys.Count && TimeZoneKeys.ElementAt(timeZoneKey) != null, "Invalid time zone key %d", timeZoneKey);
			return TimeZoneKeys.ElementAt(timeZoneKey);
		}

		public static TimeZoneKey GetTimeZoneKey(string zoneId)
		{
            ParameterCondition.RequireNonNull(zoneId, "ZoneId", "Zone id is null");
			CheckArgument(zoneId.Length > 0, "Zone id is an empty string");
			
			var zoneKey = ZoneIdToKey[zoneId.ToLower(CultureInfo.GetCultureInfo("en-US"))] ?? ZoneIdToKey[NormalizeZoneId(zoneId)];
            if (zoneKey == null)
			{
				throw new KeyNotFoundException(zoneId);
			}
			return zoneKey;
		}

		public static TimeZoneKey GetTimeZoneKeyForOffset(long offsetMinutes)
		{
			if (offsetMinutes == 0)
			{
				return UtcKey;
			}

			if (!(offsetMinutes >= OffsetTimeZoneMin && offsetMinutes <= OffsetTimeZoneMax))
			{
				throw new ClientException($"Invalid offset minutes {offsetMinutes}");
			}
			var timeZoneKey = OffsetTimeZoneKeys[((int) offsetMinutes) - OffsetTimeZoneMin];
			if (timeZoneKey == null)
			{
				throw new KeyNotFoundException(ZoneIdForOffset(offsetMinutes));
			}
			return timeZoneKey;
		}

		public string Id {get;}

		public short Key {get;}

        [JsonConstructor]
		public TimeZoneKey(string id, short key)
		{
			Id = ParameterCondition.RequireNonNull(id, "id", "id is null");
			if (key < 0)
			{
				throw new ArgumentException("key is negative");
			}
			Key = key;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Id, Key);
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			var other = (TimeZoneKey) obj;
			return Equals(Id, other.Id) && Equals(Key, other.Key);
		}

		public override string ToString()
		{
			return Id;
		}

		public static bool IsUtcZoneId(string zoneId)
		{
			return NormalizeZoneId(zoneId).Equals("utc");
		}

		private static string NormalizeZoneId(string originalZoneId)
		{
			var zoneId = originalZoneId.ToLower(CultureInfo.GetCultureInfo("en-US"));

			var startsWithEtc = zoneId.StartsWith("etc/", StringComparison.Ordinal);
			if (startsWithEtc)
			{
				zoneId = zoneId.Substring(4);
			}

			if (IsUtcEquivalentName(zoneId))
			{
				return "utc";
			}

			//
			// Normalize fixed offset time zones.
			//

			// In some zones systems, these will start with UTC, GMT or UT.
			var length = zoneId.Length;
			var startsWithEtcGmt = false;
			if (length > 3 && (zoneId.StartsWith("utc", StringComparison.Ordinal) || zoneId.StartsWith("gmt", StringComparison.Ordinal)))
			{
				if (startsWithEtc && zoneId.StartsWith("gmt", StringComparison.Ordinal))
				{
					startsWithEtcGmt = true;
				}
				zoneId = zoneId.Substring(3);
				length = zoneId.Length;
			}
			else if (length > 2 && zoneId.StartsWith("ut", StringComparison.Ordinal))
			{
				zoneId = zoneId.Substring(2);
				length = zoneId.Length;
			}

			// (+/-)00:00 is UTC
			if ("+00:00".Equals(zoneId) || "-00:00".Equals(zoneId))
			{
				return "utc";
			}

			// if zoneId matches XXX:XX, it is likely +HH:mm, so just return it
			// since only offset time zones will contain a `:` character
			if (length == 6 && zoneId[3] == ':')
			{
				return zoneId;
			}

			//
			// Rewrite (+/-)H[H] to (+/-)HH:00
			//
			if (length != 2 && length != 3)
			{
				return originalZoneId;
			}

			// zone must start with a plus or minus sign
			var signChar = zoneId[0];
			if (signChar != '+' && signChar != '-')
			{
				return originalZoneId;
			}
			if (startsWithEtcGmt)
			{
				// Flip sign for Etc/GMT(+/-)H[H]
				signChar = signChar == '-' ? '+' : '-';
			}

			// extract the tens and ones characters for the hour
			char hourTens;
			char hourOnes;
			if (length == 2)
			{
				hourTens = '0';
				hourOnes = zoneId[1];
			}
			else
			{
				hourTens = zoneId[1];
				hourOnes = zoneId[2];
			}

			// do we have a valid hours offset time zone?
			if (!char.IsDigit(hourTens) || !char.IsDigit(hourOnes))
			{
				return originalZoneId;
			}

			// is this offset 0 (e.g., UTC)?
			if (hourTens == '0' && hourOnes == '0')
			{
				return "utc";
			}

			return "" + signChar + hourTens + hourOnes + ":00";
		}

		private static bool IsUtcEquivalentName(string zoneId)
		{
			return zoneId.Equals("utc") || zoneId.Equals("z") || zoneId.Equals("ut") || zoneId.Equals("uct") || zoneId.Equals("ut") || zoneId.Equals("gmt") || zoneId.Equals("gmt0") || zoneId.Equals("greenwich") || zoneId.Equals("universal") || zoneId.Equals("zulu");
		}

		private static string ZoneIdForOffset(long offset)
		{
			return $"{(offset < 0 ? "-" : "+")}{Math.Abs(offset / 60):D2}:{Math.Abs(offset % 60):D2}";
		}

		private static void CheckArgument(bool check, string message, params object[] args)
		{
			if (!check)
			{
				throw new ArgumentException(string.Format(message, args));
			}
		}

        private static ILogger Log = new LoggerFactory().CreateLogger<TimeZoneKey>();
    }

}