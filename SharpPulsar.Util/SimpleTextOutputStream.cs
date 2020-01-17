using System;

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
	using ByteBuf = io.netty.buffer.ByteBuf;

	/// <summary>
	/// Format strings and numbers into a ByteBuf without any memory allocation.
	/// 
	/// </summary>
	public class SimpleTextOutputStream
	{
		private readonly ByteBuf buffer;
		private static readonly char[] hexChars = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};

		public SimpleTextOutputStream(ByteBuf buffer)
		{
			this.buffer = buffer;
		}

		public virtual SimpleTextOutputStream write(sbyte[] a)
		{
			buffer.writeBytes(a, 0, a.Length);
			return this;
		}

		public virtual SimpleTextOutputStream write(sbyte[] a, int offset, int len)
		{
			buffer.writeBytes(a, offset, len);
			return this;
		}

		public virtual SimpleTextOutputStream write(char c)
		{
			buffer.writeByte((sbyte) c);
			return this;
		}

		public virtual SimpleTextOutputStream write(string s)
		{
			if (string.ReferenceEquals(s, null))
			{
				return this;
			}
			int len = s.Length;
			for (int i = 0; i < len; i++)
			{
				buffer.writeByte((sbyte) s[i]);
			}

			return this;
		}

		public virtual SimpleTextOutputStream write(Number n)
		{
			if (n is int?)
			{
				return write(n.intValue());
			}
			else if (n is long?)
			{
				return write(n.longValue());
			}
			else if (n is double?)
			{
				return write(n.doubleValue());
			}
			else
			{
				return write(n.ToString());
			}
		}

		public virtual SimpleTextOutputStream writeEncoded(string s)
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
					buffer.writeByte((sbyte) '\\');
					buffer.writeByte((sbyte) 'u');
					buffer.writeByte(hexChars[(c & 0xf000) >> 12]);
					buffer.writeByte(hexChars[(c & 0x0f00) >> 8]);
					buffer.writeByte(hexChars[(c & 0x00f0) >> 4]);
					buffer.writeByte(hexChars[c & 0x000f]);
					continue;
				}

				if (c == '\\' || c == '"')
				{
					buffer.writeByte((sbyte) '\\');
				}
				buffer.writeByte((sbyte) c);
			}
			return this;
		}

		public virtual SimpleTextOutputStream write(bool b)
		{
			write(b ? "true" : "false");
			return this;
		}

		public virtual SimpleTextOutputStream write(long n)
		{
			NumberFormat.format(this.buffer, n);
			return this;
		}

		public virtual SimpleTextOutputStream write(double d)
		{
			long i = (long) d;
			write(i);

			long r = Math.Abs((long)(1000 * (d - i)));
			write('.');
			if (r == 0)
			{
				write('0');
				return this;
			}

			if (r < 100)
			{
				write('0');
			}

			if (r < 10)
			{
				write('0');
			}

			write(r);
			return this;
		}
	}

}