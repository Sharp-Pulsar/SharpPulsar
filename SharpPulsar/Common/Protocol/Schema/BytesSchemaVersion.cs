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
namespace org.apache.pulsar.common.protocol.schema
{

	/// <summary>
	/// Bytes schema version.
	/// </summary>
	public class BytesSchemaVersion : SchemaVersion, IComparable<BytesSchemaVersion>
	{

		private static readonly char[] HEX_CHARS_UPPER = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly sbyte[] bytes_Conflict;
		// cache the hash code for the string, default to 0
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private int hashCode_Conflict;

		private BytesSchemaVersion(sbyte[] bytes)
		{
			this.bytes_Conflict = bytes;
		}

		public virtual sbyte[] bytes()
		{
			return bytes_Conflict;
		}

		public static BytesSchemaVersion of(sbyte[] bytes)
		{
			return bytes != null ? new BytesSchemaVersion(bytes) : null;
		}

		/// <summary>
		/// Get the data from the Bytes. </summary>
		/// <returns> The underlying byte array </returns>
		public virtual sbyte[] get()
		{
			return this.bytes_Conflict;
		}

		/// <summary>
		/// The hashcode is cached except for the case where it is computed as 0, in which
		/// case we compute the hashcode on every call.
		/// </summary>
		/// <returns> the hashcode </returns>
		public override int GetHashCode()
		{
			if (hashCode_Conflict == 0)
			{
				hashCode_Conflict = Arrays.hashCode(bytes_Conflict);
			}
			return hashCode_Conflict;
		}

		public override bool Equals(object other)
		{
			if (this == other)
			{
				return true;
			}
			if (other == null)
			{
				return false;
			}

			// we intentionally use the function to compute hashcode here
			if (this.GetHashCode() != other.GetHashCode())
			{
				return false;
			}

			if (other is BytesSchemaVersion)
			{
				return Arrays.equals(this.bytes_Conflict, ((BytesSchemaVersion) other).get());
			}

			return false;
		}

		public virtual int CompareTo(BytesSchemaVersion that)
		{
			return BYTES_LEXICO_COMPARATOR.Compare(this.bytes_Conflict, that.bytes_Conflict);
		}

		public override string ToString()
		{
			return BytesSchemaVersion.toString(bytes_Conflict, 0, bytes_Conflict.Length);
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
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static String toString(final byte[] b, int off, int len)
		private static string toString(sbyte[] b, int off, int len)
		{
			StringBuilder result = new StringBuilder();

			if (b == null)
			{
				return result.ToString();
			}

			// just in case we are passed a 'len' that is > buffer length...
			if (off >= b.Length)
			{
				return result.ToString();
			}

			if (off + len > b.Length)
			{
				len = b.Length - off;
			}

			for (int i = off; i < off + len; ++i)
			{
				int ch = b[i] & 0xFF;
				if (ch >= ' ' && ch <= '~' && ch != '\\')
				{
					result.Append((char) ch);
				}
				else
				{
					result.Append("\\x");
					result.Append(HEX_CHARS_UPPER[ch / 0x10]);
					result.Append(HEX_CHARS_UPPER[ch % 0x10]);
				}
			}
			return result.ToString();
		}

		/// <summary>
		/// A byte array comparator based on lexicograpic ordering.
		/// </summary>
		public static readonly ByteArrayComparator BYTES_LEXICO_COMPARATOR = new LexicographicByteArrayComparator();

		/// <summary>
		/// This interface helps to compare byte arrays.
		/// </summary>
		public interface ByteArrayComparator : IComparer<sbyte[]>
		{

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: int compare(final byte[] buffer1, int offset1, int length1, final byte[] buffer2, int offset2, int length2);
			int compare(sbyte[] buffer1, int offset1, int length1, sbyte[] buffer2, int offset2, int length2);
		}

		[Serializable]
		private class LexicographicByteArrayComparator : ByteArrayComparator
		{

			internal const long serialVersionUID = -1915703761143534937L;

			public virtual int Compare(sbyte[] buffer1, sbyte[] buffer2)
			{
				return compare(buffer1, 0, buffer1.Length, buffer2, 0, buffer2.Length);
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public int compare(final byte[] buffer1, int offset1, int length1, final byte[] buffer2, int offset2, int length2)
			public virtual int Compare(sbyte[] buffer1, int offset1, int length1, sbyte[] buffer2, int offset2, int length2)
			{

				// short circuit equal case
				if (buffer1 == buffer2 && offset1 == offset2 && length1 == length2)
				{
					return 0;
				}

				// similar to Arrays.compare() but considers offset and length
				int end1 = offset1 + length1;
				int end2 = offset2 + length2;
				for (int i = offset1, j = offset2; i < end1 && j < end2; i++, j++)
				{
					int a = buffer1[i] & 0xff;
					int b = buffer2[j] & 0xff;
					if (a != b)
					{
						return a - b;
					}
				}
				return length1 - length2;
			}
		}
	}

}