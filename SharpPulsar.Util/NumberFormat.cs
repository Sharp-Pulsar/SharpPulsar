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

using DotNetty.Buffers;

namespace SharpPulsar.Utility
{
    /// <summary>
	/// Custom number formatter for {@code io.netty.buffer.ByteBuf}.
	/// </summary>
	public class NumberFormat
	{

		internal static void Format(IByteBuffer @out, long num)
		{
			if (num == 0)
			{
				@out.WriteByte('0');
				return;
			}

			// Long.MIN_VALUE needs special handling since abs(Long.MIN_VALUE) = abs(Long.MAX_VALUE) + 1
			bool encounteredMinValue = (num == long.MinValue);
			if (num < 0)
			{
				@out.WriteByte('-');
				num += encounteredMinValue ? 1 : 0;
				num *= -1;
			}

			// Putting the number in bytebuf in reverse order
			int start = @out.WriterIndex;
			formatHelper(@out, num);
			int end = @out.WriterIndex;

			if (encounteredMinValue)
			{
				@out.SetByte(start, @out.GetByte(start) + 1);
			}

			// Reversing the digits
			end--;
			for (int i = 0; i <= (end - start) / 2; i++)
			{
				byte tmp = @out.GetByte(end - i);
				@out.GetByte(start + i);
				@out.SetByte(start + i, tmp);
			}
		}

		internal static void formatHelper(IByteBuffer @out, long num)
		{
			while (num != 0)
			{
				@out.WriteByte((int)('0' + num % 10));
				num /= 10;
			}
		}
	}

}