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

	public sealed class KerberosUtil
	{
		private const string FilePrefix = "FILE:";

		private KerberosUtil()
		{
		}

		public static Optional<string> DefaultCredentialCachePath()
		{
			string Value = nullToEmpty(Environment.GetEnvironmentVariable("KRB5CCNAME"));
			if (Value.StartsWith(FilePrefix, StringComparison.Ordinal))
			{
				Value = Value.Substring(FilePrefix.Length);
			}
			return Optional.ofNullable(emptyToNull(Value));
		}
	}

}