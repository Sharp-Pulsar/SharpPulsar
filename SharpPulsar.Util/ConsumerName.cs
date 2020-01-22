using System;
using System.Security.Cryptography;
using System.Text;
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

	public static class ConsumerName
	{
		public static string GenerateRandomName()
		{
			return SHA1Hex(Guid.NewGuid().ToString()).Substring(0, 5);
		}
		/// <summary>
		/// Compute hash for string encoded as UTF8
		/// </summary>
		/// <param name="s">String to be hashed</param>
		/// <returns>40-character hex string</returns>
		public static string SHA1Hex(string s)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(s);

			var sha1 = SHA1.Create();
			byte[] hashBytes = sha1.ComputeHash(bytes);

			return HexStringFromBytes(hashBytes);
		}
		/// <summary>
		/// Convert an array of bytes to a string of hex digits
		/// </summary>
		/// <param name="bytes">array of bytes</param>
		/// <returns>String of hex digits</returns>
		public static string HexStringFromBytes(byte[] bytes)
		{
			var sb = new StringBuilder();
			foreach (byte b in bytes)
			{
				var hex = b.ToString("x2");
				sb.Append(hex);
			}
			return sb.ToString();
		}
	}

}