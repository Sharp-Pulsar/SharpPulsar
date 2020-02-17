using System;
using DotNetty.Buffers;
using DotNetty.Common;
using Google.Protobuf;

// Protocol Buffers - Google's data interchange format
// Copyright 2008 Google Inc.  All rights reserved.
// http://code.google.com/p/protobuf/
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//     * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
//     * Neither the name of Google Inc. nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

/*
 * This file is derived from Google ProcolBuffer CodedOutputStream class
 * with adaptations to work directly with Netty ByteBuf instances.
 */

namespace SharpPulsar.Utility.Protobuf
{
	public class ByteBufCodedOutputStream
	{
		public interface ByteBufGeneratedMessage
		{
			int SerializedSize {get;}
			void WriteTo(ByteBufCodedOutputStream output);
		}

		private IByteBuffer _buf;
		internal static ThreadLocalPool<ByteBufCodedOutputStream> _pool = new ThreadLocalPool<ByteBufCodedOutputStream>(handle => new ByteBufCodedOutputStream(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private ByteBufCodedOutputStream(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}

		
		public static ByteBufCodedOutputStream Get(IByteBuffer buf)
		{
			ByteBufCodedOutputStream stream =_pool.Take();
			stream._buf = buf;
			return stream;
		}

		public virtual void Recycle()
		{
			_buf = null;
			_handle.Release(this);
		}


		/// <summary>
		/// Write a single byte. </summary>
		public virtual void WriteRawByte(int value)
		{
			_buf.WriteByte(value);
		}

		/// <summary>
		/// Encode and write a varint. {@code value} is treated as unsigned, so it won't be sign-extended if negative.
		/// </summary>
		public virtual void WriteRawVarint32(int value)
		{
			while (true)
			{
				if ((value & ~0x7F) == 0)
				{
					WriteRawByte(value);
					return;
				}
				else
				{
					WriteRawByte((value & 0x7F) | 0x80);
					value = (int)((uint)value >> 7);
				}
			}
		}

		/// <summary>
		/// Encode and write a tag. </summary>
		public virtual void WriteTag(int fieldNumber, int wireType)
		{
			WriteRawVarint32(MakeTag(fieldNumber, wireType));
		}

		/// <summary>
		/// Write an {@code int32} field, including tag, to the stream. </summary>
		public virtual void WriteInt32(int fieldNumber, int value)
		{
			WriteTag(fieldNumber, (int)WireFormat.WireType.Varint);
			WriteInt32NoTag(value);
		}

		public virtual void WriteInt64(int fieldNumber, long value)
		{
			WriteTag(fieldNumber, (int)WireFormat.WireType.Varint);
			WriteInt64NoTag(value);
		}
		public virtual void WriteUInt64(int fieldNumber, long value)
		{
			WriteTag(fieldNumber, (int)WireFormat.WireType.Varint);
			WriteUInt64NoTag(value);
		}

		/// <summary>
		/// Write a {@code bool} field, including tag, to the stream. </summary>
		public virtual void WriteBool(int fieldNumber, bool value)
		{
			WriteTag(fieldNumber, (int)WireFormat.WireType.Varint);
			WriteBoolNoTag(value);
		}

		/// <summary>
		/// Write a {@code bool} field to the stream. </summary>
		public virtual void WriteBoolNoTag(bool value)
		{
		  WriteRawByte(value ? 1 : 0);
		}

		/// <summary>
		/// Write a {@code uint64} field to the stream. </summary>
		public virtual void WriteInt64NoTag(long value)
		{
			WriteRawVarint64(value);
		}

		/// <summary>
		/// Write a {@code uint64} field to the stream. </summary>
		public virtual void WriteUInt64NoTag(long value)
		{
			WriteRawVarint64(value);
		}

		/// <summary>
		/// Encode and write a varint. </summary>
		public virtual void WriteRawVarint64(long value)
		{
			while (true)
			{
				if ((value & ~0x7FL) == 0)
				{
					WriteRawByte((int) value);
					return;
				}
				else
				{
					WriteRawByte(((int) value & 0x7F) | 0x80);
					value = (long)((ulong)value >> 7);
				}
			}
		}

		/// <summary>
		/// Write a {@code bytes} field, including tag, to the stream. </summary>
		public virtual void WriteBytes(int fieldNumber, ByteString value)
		{
			WriteTag(fieldNumber, (int)WireFormat.WireType.LengthDelimited);
			WriteBytesNoTag(value);
		}

		/// <summary>
		/// Write a {@code bytes} field to the stream. </summary>
		public virtual void WriteBytesNoTag(ByteString value)
		{
			WriteRawVarint32(value.Length);
			WriteRawBytes(value);
		}


		private static readonly FastThreadLocal<sbyte[]> localByteArray = new FastThreadLocal<sbyte[]>();

		/// <summary>
		/// Write a byte string. </summary>
		public virtual void WriteRawBytes(ByteString value)
		{
			sbyte[] localBuf = localByteArray.Value;
			if (localBuf == null || localBuf.Length < value.Length)
			{
				localBuf = new sbyte[Math.Max(value.Length, 1024)];
				localByteArray.Value = localBuf;
			}

			value.CopyTo((byte[])(object)localBuf, 0);
			_buf.WriteBytes((byte[])(object)localBuf, 0, value.Length);
		}

		public virtual void WriteEnum(int fieldNumber, int value)
		{
			WriteTag(fieldNumber, (int)WireFormat.WireType.Varint);
			WriteEnumNoTag(value);
		}

		/// <summary>
		/// Write a {@code uint32} field, including tag, to the stream. </summary>
		public virtual void WriteUInt32(int fieldNumber, int value)
		{
			WriteTag(fieldNumber, (int)WireFormat.WireType.Varint);
			WriteUInt32NoTag(value);
		}

		/// <summary>
		/// Write a {@code uint32} field to the stream. </summary>
		public virtual void WriteUInt32NoTag(int value)
		{
			WriteRawVarint32(value);
		}

		public virtual void WriteSFixed64(int fieldNumber, long value)
		{
			WriteTag(fieldNumber, (int)WireFormat.WireType.Fixed64);
			WriteSFixed64NoTag(value);
		}
		public virtual void WriteSFixed64NoTag(long value)
		{
			_buf.WriteLongLE(value);
		}

		/// <summary>
		/// Write an enum field to the stream. Caller is responsible for converting the enum value to its numeric value.
		/// </summary>
		/// 
		public virtual void WriteEnumNoTag(int value)
		{
			WriteInt32NoTag(value);
		}

		/// <summary>
		/// Write an {@code int32} field to the stream. </summary>
		/// 
		public virtual void WriteInt32NoTag(int value)
		{
			if (value >= 0)
			{
				WriteRawVarint32(value);
			}
			else
			{
				// Must sign-extend.
				WriteRawVarint64(value);
			}
		}

		/// <summary>
		/// Write an embedded message field, including tag, to the stream. </summary>
		/// 
		public virtual void WriteMessage(int fieldNumber, ByteBufGeneratedMessage value)
		{
			WriteTag(fieldNumber, (int)WireFormat.WireType.LengthDelimited);
			WriteMessageNoTag(value);
		}

		/// <summary>
		/// Write an embedded message field to the stream. </summary>
		public virtual void WriteMessageNoTag(ByteBufGeneratedMessage value)
		{
			WriteRawVarint32(value.SerializedSize);
			value.WriteTo(this);
		}

		internal const int TAG_TYPE_BITS = 3;

		/// <summary>
		/// Makes a tag value given a field number and wire type. </summary>
		internal static int MakeTag(int fieldNumber, int wireType)
		{
			return (fieldNumber << TAG_TYPE_BITS) | wireType;
		}

		/// <summary>
		/// Write an double field, including tag, to the stream. </summary>
		/// 
		public virtual void WriteDouble(int fieldNumber, double value)
		{
			WriteTag(fieldNumber, (int)WireFormat.WireType.Fixed64);
			_buf.WriteLongLE(BitConverter.DoubleToInt64Bits(value));
		}
	}

}