using System;
using System.Collections.Generic;
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
namespace SharpPulsar.Protocol.Schema
{

	/// <summary>
	/// Bytes schema version.
	/// </summary>
	public class BytesSchemaVersion : SchemaVersion, IComparable<BytesSchemaVersion>
	{

		private static readonly char[] HEX_CHARS_UPPER = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

		private readonly sbyte[] bytes;
		// cache the hash code for the string, default to 0
		private int _hashCode;

		private BytesSchemaVersion(sbyte[] Bytes)
		{
			this.bytes = Bytes;
		}

		public sbyte[] Bytes()
		{
			return bytes;
		}

		public static BytesSchemaVersion Of(sbyte[] Bytes)
		{
			return Bytes != null ? new BytesSchemaVersion(Bytes) : null;
		}

		/// <summary>
		/// Get the data from the Bytes. </summary>
		/// <returns> The underlying byte array </returns>
		public virtual sbyte[] Get()
		{
			return this.bytes;
		}

		/// <summary>
		/// The hashcode is cached except for the case where it is computed as 0, in which
		/// case we compute the hashcode on every call.
		/// </summary>
		/// <returns> the hashcode </returns>
		public override int GetHashCode()
		{
			if (_hashCode == 0)
			{
				_hashCode = bytes.GetHashCode();
			}
			return _hashCode;
		}

		public override bool Equals(object Other)
		{
			if (this == Other)
			{
				return true;
			}
			if (Other == null)
			{
				return false;
			}

			// we intentionally use the function to compute hashcode here
			if (this.GetHashCode() != Other.GetHashCode())
			{
				return false;
			}

			if (Other is BytesSchemaVersion)
			{
				return Array.Equals(this.bytes, ((BytesSchemaVersion) Other).Get());
			}

			return false;
		}

		public int CompareTo(BytesSchemaVersion That)
		{
			return BytesLexicoComparator.Compare(this.bytes, That.bytes);
		}

		public override string ToString()
		{
			return BytesSchemaVersion.toString(bytes, 0, bytes.Length);
		}

		/// <summary>
		/// Write a printable representation of a byte array. Non-printable
		/// characters are hex escaped in the format \\x%02X, eg:
		/// \x00 \x05 etc.
		/// 
		/// <para>This function is brought from org.apache.hadoop.hbase.util.Bytes
		/// 
		/// </para>
		/// </summary>
		/// <param name="b"> array to write out </param>
		/// <param name="off"> offset to start at </param>
		/// <param name="len"> length to write </param>
		/// <returns> string output </returns>
		private static string toString(in sbyte[] B, int Off, int Len)
		{
			StringBuilder Result = new StringBuilder();

			if (B == null)
			{
				return Result.ToString();
			}

			// just in case we are passed a 'len' that is > buffer length...
			if (Off >= B.Length)
			{
				return Result.ToString();
			}

			if (Off + Len > B.Length)
			{
				Len = B.Length - Off;
			}

			for (int I = Off; I < Off + Len; ++I)
			{
				int Ch = B[I] & 0xFF;
				if (Ch >= ' ' && Ch <= '~' && Ch != '\\')
				{
					Result.Append((char) Ch);
				}
				else
				{
					Result.Append(@"\x");
					Result.Append(HEX_CHARS_UPPER[Ch / 0x10]);
					Result.Append(HEX_CHARS_UPPER[Ch % 0x10]);
				}
			}
			return Result.ToString();
		}

		/// <summary>
		/// A byte array comparator based on lexicograpic ordering.
		/// </summary>
		public static readonly ByteArrayComparator BytesLexicoComparator = new LexicographicByteArrayComparator();

		/// <summary>
		/// This interface helps to compare byte arrays.
		/// </summary>
		public interface ByteArrayComparator : IComparer<sbyte[]>
		{

			int Compare(in sbyte[] Buffer1, int Offset1, int Length1, in sbyte[] Buffer2, int Offset2, int Length2);
		}

		[Serializable]
		public class LexicographicByteArrayComparator : ByteArrayComparator
		{

			internal const long SerialVersionUID = -1915703761143534937L;

			public int Compare(sbyte[] Buffer1, sbyte[] Buffer2)
			{
				return Compare(Buffer1, 0, Buffer1.Length, Buffer2, 0, Buffer2.Length);
			}

			public virtual int Compare(in sbyte[] Buffer1, int Offset1, int Length1, in sbyte[] Buffer2, int Offset2, int Length2)
			{

				// short circuit equal case
				if (Buffer1 == Buffer2 && Offset1 == Offset2 && Length1 == Length2)
				{
					return 0;
				}

				// similar to Arrays.compare() but considers offset and length
				int End1 = Offset1 + Length1;
				int End2 = Offset2 + Length2;
				for (int I = Offset1, j = Offset2; I < End1 && j < End2; I++, j++)
				{
					int A = Buffer1[I] & 0xff;
					int B = Buffer2[j] & 0xff;
					if (A != B)
					{
						return A - B;
					}
				}
				return Length1 - Length2;
			}
		}
	}

}