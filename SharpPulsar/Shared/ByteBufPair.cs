using DotNetty.Buffers;
using DotNetty.Common;
using DotNetty.Common.Utilities;
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
namespace SharpPulsar.Shared
{

	/// <summary>
	/// ByteBuf holder that contains 2 buffers.
	/// </summary>
	public sealed class ByteBufPair : AbstractReferenceCounted
	{

		public  IByteBuffer First;
		public  IByteBuffer Second;
		public int RefCnt;
		
		/// <summary>
		/// Get a new <seealso cref="ByteBufPair"/> from the pool and assign 2 buffers to it.
		/// 
		/// <para>The buffers b1 and b2 lifecycles are now managed by the ByteBufPair:
		/// when the <seealso cref="ByteBufPair"/> is deallocated, b1 and b2 will be released as well.
		/// 
		/// </para>
		/// </summary>
		/// <param name="b1"> </param>
		/// <param name="b2">
		/// @return </param>
		public static ByteBufPair Get(IByteBuffer b1, IByteBuffer b2)
		{
            ByteBufPair buf = new ByteBufPair {RefCnt = 1, First = b1, Second = b2};
            return buf;
		}



		public int ReadableBytes()
		{
			return First.ReadableBytes + Second.ReadableBytes;
		}

		/// <returns> a single buffer with the content of both individual buffers </returns>
		public static IByteBuffer Coalesce(ByteBufPair pair)
		{
			IByteBuffer b = Unpooled.Buffer(pair.ReadableBytes());
			b.WriteBytes(pair.First, pair.First.ReaderIndex, pair.First.ReadableBytes);
			b.WriteBytes(pair.Second, pair.Second.ReaderIndex, pair.Second.ReadableBytes);
			return b;
		}

		protected override void Deallocate()
		{
			First.Release();
			Second.Release();
			First = Second = null;
		}

		public override IReferenceCounted Touch(object hint)
		{
			First.Touch(hint);
			Second.Touch(hint);
			return this;
		}
		

	}

}