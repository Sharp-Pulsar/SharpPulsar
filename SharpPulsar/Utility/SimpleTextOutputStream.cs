using System;
using DotNetty.Buffers;

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
namespace SharpPulsar.Utility
{
	
	/// <summary>
	/// Format strings and numbers into a ByteBuf without any memory allocation.
	/// 
	/// </summary>
	public class SimpleTextOutputStream
	{
		private readonly IByteBuffer buffer;
		private static readonly char[] hexChars = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};

		public SimpleTextOutputStream(IByteBuffer buffer)
		{
			this.buffer = buffer;
		}

		public virtual SimpleTextOutputStream Write(sbyte[] a)
        {
            var ab = Unpooled.WrappedBuffer((byte[])(object)a, 0, a.Length);
			buffer.WriteBytes(ab);
			return this;
		}

		public virtual SimpleTextOutputStream Write(sbyte[] a, int offset, int len)
		{
            var ab = Unpooled.WrappedBuffer((byte[])(object)a, offset, len);
			buffer.WriteBytes(ab);
			return this;
		}

		public virtual SimpleTextOutputStream Write(char c)
		{
			buffer.WriteByte((sbyte) c);
			return this;
		}

		public virtual SimpleTextOutputStream Write(string s)
		{
			if (string.ReferenceEquals(s, null))
			{
				return this;
			}
			int len = s.Length;
			for (int i = 0; i < len; i++)
			{
				buffer.WriteByte((sbyte) s[i]);
			}

			return this;
		}

		public virtual SimpleTextOutputStream Write(object n)
		{
			if (n is int?)
			{
				return Write((int)n);
			}
			else if (n is long?)
			{
				return Write((long)n);
			}
			else if (n is double?)
			{
				return Write((double)n);
			}
			else
			{
				return Write((string)n);
			}
		}

		public virtual SimpleTextOutputStream WriteEncoded(string s)
		{
			if (string.ReferenceEquals(s, null))
			{
				return this;
			}

			int len = s.Length;
			for (int i = 0; i < len; i++)
			{

				char c = s[i];
				if (c < (char)32 || c > (char)126)
				{ // below 32 and above 126 in ascii table
					buffer.WriteByte((sbyte) '\\');
					buffer.WriteByte((sbyte) 'u');
					buffer.WriteByte(hexChars[(c & 0xf000) >> 12]);
					buffer.WriteByte(hexChars[(c & 0x0f00) >> 8]);
					buffer.WriteByte(hexChars[(c & 0x00f0) >> 4]);
					buffer.WriteByte(hexChars[c & 0x000f]);
					continue;
				}

				if (c == '\\' || c == '"')
				{
					buffer.WriteByte((sbyte) '\\');
				}
				buffer.WriteByte((sbyte) c);
			}
			return this;
		}

		public virtual SimpleTextOutputStream Write(bool b)
		{
			Write(b ? "true" : "false");
			return this;
		}

		public virtual SimpleTextOutputStream Write(long n)
		{
			NumberFormat.Format(this.buffer, n);
			return this;
		}

		public virtual SimpleTextOutputStream Write(double d)
		{
			long i = (long) d;
			Write(i);

			long r = Math.Abs((long)(1000 * (d - i)));
			Write('.');
			if (r == 0)
			{
				Write('0');
				return this;
			}

			if (r < 100)
			{
				Write('0');
			}

			if (r < 10)
			{
				Write('0');
			}

			Write(r);
			return this;
		}
	}

}