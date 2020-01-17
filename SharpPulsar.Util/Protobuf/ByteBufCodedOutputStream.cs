using System;

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

namespace org.apache.pulsar.common.util.protobuf
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Recycler = io.netty.util.Recycler;
	using Handle = io.netty.util.Recycler.Handle;
	using FastThreadLocal = io.netty.util.concurrent.FastThreadLocal;

	using ByteString = org.apache.pulsar.shaded.com.google.protobuf.v241.ByteString;
	using WireFormat = org.apache.pulsar.shaded.com.google.protobuf.v241.WireFormat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("checkstyle:JavadocType") public class ByteBufCodedOutputStream
	public class ByteBufCodedOutputStream
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("checkstyle:JavadocType") public interface ByteBufGeneratedMessage
		public interface ByteBufGeneratedMessage
		{
			int SerializedSize {get;}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeTo(ByteBufCodedOutputStream output) throws java.io.IOException;
			void writeTo(ByteBufCodedOutputStream output);
		}

		private ByteBuf buf;

		private readonly Recycler.Handle<ByteBufCodedOutputStream> recyclerHandle;

		public static ByteBufCodedOutputStream get(ByteBuf buf)
		{
			ByteBufCodedOutputStream stream = RECYCLER.get();
			stream.buf = buf;
			return stream;
		}

		public virtual void recycle()
		{
			buf = null;
			recyclerHandle.recycle(this);
		}

		private ByteBufCodedOutputStream(Recycler.Handle<ByteBufCodedOutputStream> handle)
		{
			this.recyclerHandle = handle;
		}

		private static readonly Recycler<ByteBufCodedOutputStream> RECYCLER = new RecyclerAnonymousInnerClass();

		private class RecyclerAnonymousInnerClass : Recycler<ByteBufCodedOutputStream>
		{
			protected internal ByteBufCodedOutputStream newObject(Recycler.Handle<ByteBufCodedOutputStream> handle)
			{
				return new ByteBufCodedOutputStream(handle);
			}
		}

		/// <summary>
		/// Write a single byte. </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void writeRawByte(final int value)
		public virtual void writeRawByte(int value)
		{
			buf.writeByte(value);
		}

		/// <summary>
		/// Encode and write a varint. {@code value} is treated as unsigned, so it won't be sign-extended if negative.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeRawVarint32(int value) throws java.io.IOException
		public virtual void writeRawVarint32(int value)
		{
			while (true)
			{
				if ((value & ~0x7F) == 0)
				{
					writeRawByte(value);
					return;
				}
				else
				{
					writeRawByte((value & 0x7F) | 0x80);
					value = (int)((uint)value >> 7);
				}
			}
		}

		/// <summary>
		/// Encode and write a tag. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeTag(final int fieldNumber, final int wireType) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeTag(int fieldNumber, int wireType)
		{
			writeRawVarint32(makeTag(fieldNumber, wireType));
		}

		/// <summary>
		/// Write an {@code int32} field, including tag, to the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInt32(final int fieldNumber, final int value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeInt32(int fieldNumber, int value)
		{
			writeTag(fieldNumber, WireFormat.WIRETYPE_VARINT);
			writeInt32NoTag(value);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInt64(final int fieldNumber, final long value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeInt64(int fieldNumber, long value)
		{
			writeTag(fieldNumber, WireFormat.WIRETYPE_VARINT);
			writeInt64NoTag(value);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeUInt64(final int fieldNumber, final long value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeUInt64(int fieldNumber, long value)
		{
			writeTag(fieldNumber, WireFormat.WIRETYPE_VARINT);
			writeUInt64NoTag(value);
		}

		/// <summary>
		/// Write a {@code bool} field, including tag, to the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeBool(final int fieldNumber, final boolean value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeBool(int fieldNumber, bool value)
		{
			writeTag(fieldNumber, WireFormat.WIRETYPE_VARINT);
			writeBoolNoTag(value);
		}

		/// <summary>
		/// Write a {@code bool} field to the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeBoolNoTag(final boolean value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeBoolNoTag(bool value)
		{
		  writeRawByte(value ? 1 : 0);
		}

		/// <summary>
		/// Write a {@code uint64} field to the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInt64NoTag(final long value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeInt64NoTag(long value)
		{
			writeRawVarint64(value);
		}

		/// <summary>
		/// Write a {@code uint64} field to the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeUInt64NoTag(final long value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeUInt64NoTag(long value)
		{
			writeRawVarint64(value);
		}

		/// <summary>
		/// Encode and write a varint. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeRawVarint64(long value) throws java.io.IOException
		public virtual void writeRawVarint64(long value)
		{
			while (true)
			{
				if ((value & ~0x7FL) == 0)
				{
					writeRawByte((int) value);
					return;
				}
				else
				{
					writeRawByte(((int) value & 0x7F) | 0x80);
					value = (long)((ulong)value >> 7);
				}
			}
		}

		/// <summary>
		/// Write a {@code bytes} field, including tag, to the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeBytes(final int fieldNumber, final org.apache.pulsar.shaded.com.google.protobuf.v241.ByteString value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeBytes(int fieldNumber, ByteString value)
		{
			writeTag(fieldNumber, WireFormat.WIRETYPE_LENGTH_DELIMITED);
			writeBytesNoTag(value);
		}

		/// <summary>
		/// Write a {@code bytes} field to the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeBytesNoTag(final org.apache.pulsar.shaded.com.google.protobuf.v241.ByteString value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeBytesNoTag(ByteString value)
		{
			writeRawVarint32(value.size());
			writeRawBytes(value);
		}


		private static readonly FastThreadLocal<sbyte[]> localByteArray = new FastThreadLocal<sbyte[]>();

		/// <summary>
		/// Write a byte string. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeRawBytes(final org.apache.pulsar.shaded.com.google.protobuf.v241.ByteString value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeRawBytes(ByteString value)
		{
			sbyte[] localBuf = localByteArray.get();
			if (localBuf == null || localBuf.Length < value.size())
			{
				localBuf = new sbyte[Math.Max(value.size(), 1024)];
				localByteArray.set(localBuf);
			}

			value.copyTo(localBuf, 0);
			buf.writeBytes(localBuf, 0, value.size());
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeEnum(final int fieldNumber, final int value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeEnum(int fieldNumber, int value)
		{
			writeTag(fieldNumber, WireFormat.WIRETYPE_VARINT);
			writeEnumNoTag(value);
		}

		/// <summary>
		/// Write a {@code uint32} field, including tag, to the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeUInt32(final int fieldNumber, final int value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeUInt32(int fieldNumber, int value)
		{
			writeTag(fieldNumber, WireFormat.WIRETYPE_VARINT);
			writeUInt32NoTag(value);
		}

		/// <summary>
		/// Write a {@code uint32} field to the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeUInt32NoTag(final int value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeUInt32NoTag(int value)
		{
			writeRawVarint32(value);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeSFixed64(final int fieldNumber, long value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeSFixed64(int fieldNumber, long value)
		{
			writeTag(fieldNumber, WireFormat.WIRETYPE_FIXED64);
			writeSFixed64NoTag(value);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeSFixed64NoTag(long value) throws java.io.IOException
		public virtual void writeSFixed64NoTag(long value)
		{
			buf.writeLongLE(value);
		}

		/// <summary>
		/// Write an enum field to the stream. Caller is responsible for converting the enum value to its numeric value.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeEnumNoTag(final int value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeEnumNoTag(int value)
		{
			writeInt32NoTag(value);
		}

		/// <summary>
		/// Write an {@code int32} field to the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeInt32NoTag(final int value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeInt32NoTag(int value)
		{
			if (value >= 0)
			{
				writeRawVarint32(value);
			}
			else
			{
				// Must sign-extend.
				writeRawVarint64(value);
			}
		}

		/// <summary>
		/// Write an embedded message field, including tag, to the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeMessage(final int fieldNumber, final ByteBufGeneratedMessage value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeMessage(int fieldNumber, ByteBufGeneratedMessage value)
		{
			writeTag(fieldNumber, WireFormat.WIRETYPE_LENGTH_DELIMITED);
			writeMessageNoTag(value);
		}

		/// <summary>
		/// Write an embedded message field to the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeMessageNoTag(final ByteBufGeneratedMessage value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeMessageNoTag(ByteBufGeneratedMessage value)
		{
			writeRawVarint32(value.SerializedSize);
			value.writeTo(this);
		}

		internal const int TAG_TYPE_BITS = 3;

		/// <summary>
		/// Makes a tag value given a field number and wire type. </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static int makeTag(final int fieldNumber, final int wireType)
		internal static int makeTag(int fieldNumber, int wireType)
		{
			return (fieldNumber << TAG_TYPE_BITS) | wireType;
		}

		/// <summary>
		/// Write an double field, including tag, to the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeDouble(final int fieldNumber, double value) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void writeDouble(int fieldNumber, double value)
		{
			writeTag(fieldNumber, WireFormat.WIRETYPE_FIXED64);
			buf.writeLongLE(System.BitConverter.DoubleToInt64Bits(value));
		}
	}

}