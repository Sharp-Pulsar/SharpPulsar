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

	/// <summary>
	/// Date-time String formatter utility class.
	/// </summary>
	public class DateFormatter
	{

		private static readonly DateTimeFormatter DATE_FORMAT = DateTimeFormatter.ISO_OFFSET_DATE_TIME.withZone(ZoneId.systemDefault());

		/// <returns> a String representing the current datetime </returns>
		public static string now()
		{
			return format(Instant.now());
		}

		/// <returns> a String representing a particular timestamp (in milliseconds) </returns>
		public static string format(long timestamp)
		{
			return format(Instant.ofEpochMilli(timestamp));
		}

		/// <returns> a String representing a particular time instant </returns>
		public static string format(Instant instant)
		{
			return DATE_FORMAT.format(instant);
		}

		/// <param name="datetime"> </param>
		/// <returns> the parsed timestamp (in milliseconds) of the provided datetime </returns>
		/// <exception cref="DateTimeParseException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static long parse(String datetime) throws java.time.format.DateTimeParseException
		public static long parse(string datetime)
		{
			Instant instant = Instant.from(DATE_FORMAT.parse(datetime));

			return instant.toEpochMilli();
		}

		private DateFormatter()
		{
		}
	}

}