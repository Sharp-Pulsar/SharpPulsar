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
 * This file is derived from Google ProcolBuffer CodedInputStream class
 * with adaptations to work directly with Netty ByteBuf instances.
 */

namespace SharpPulsar.Util.Protobuf
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Recycler = io.netty.util.Recycler;
	using Handle = io.netty.util.Recycler.Handle;
	using FastThreadLocal = io.netty.util.concurrent.FastThreadLocal;

	using ByteString = org.apache.pulsar.shaded.com.google.protobuf.v241.ByteString;
	using ExtensionRegistryLite = org.apache.pulsar.shaded.com.google.protobuf.v241.ExtensionRegistryLite;
	using InvalidProtocolBufferException = org.apache.pulsar.shaded.com.google.protobuf.v241.InvalidProtocolBufferException;
	using WireFormat = org.apache.pulsar.shaded.com.google.protobuf.v241.WireFormat;

	public class ByteBufCodedInputStream
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("checkstyle:JavadocType") public interface ByteBufMessageBuilder
		public interface ByteBufMessageBuilder
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ByteBufMessageBuilder mergeFrom(ByteBufCodedInputStream input, org.apache.pulsar.shaded.com.google.protobuf.v241.ExtensionRegistryLite ext) throws java.io.IOException;
			ByteBufMessageBuilder mergeFrom(ByteBufCodedInputStream input, ExtensionRegistryLite ext);
		}

		private ByteBuf buf;
		private int lastTag;

		private readonly Recycler.Handle<ByteBufCodedInputStream> recyclerHandle;

		public static ByteBufCodedInputStream get(ByteBuf buf)
		{
			ByteBufCodedInputStream stream = RECYCLER.get();
			stream.buf = buf;
			stream.lastTag = 0;
			return stream;
		}

		private ByteBufCodedInputStream(Recycler.Handle<ByteBufCodedInputStream> handle)
		{
			this.recyclerHandle = handle;
		}

		private static readonly Recycler<ByteBufCodedInputStream> RECYCLER = new RecyclerAnonymousInnerClass();

		private class RecyclerAnonymousInnerClass : Recycler<ByteBufCodedInputStream>
		{
			protected internal ByteBufCodedInputStream newObject(Recycler.Handle<ByteBufCodedInputStream> handle)
			{
				return new ByteBufCodedInputStream(handle);
			}
		}

		public virtual void recycle()
		{
			this.buf = null;
			if (recyclerHandle != null)
			{
				recyclerHandle.recycle(this);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int readTag() throws java.io.IOException
		public virtual int readTag()
		{
			if (AtEnd)
			{
				lastTag = 0;
				return 0;
			}

			lastTag = readRawVarint32();
			if (WireFormat.getTagFieldNumber(lastTag) == 0)
			{
				// If we actually read zero (or any tag number corresponding to field
				// number zero), that's not a valid tag.
				throw new InvalidProtocolBufferException("CodedInputStream encountered a malformed varint.");
			}
			return lastTag;
		}

		/// <summary>
		/// Read a {@code uint32} field value from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int readUInt32() throws java.io.IOException
		public virtual int readUInt32()
		{
			return readRawVarint32();
		}

		/// <summary>
		/// Read an enum field value from the stream. Caller is responsible for converting the numeric value to an actual
		/// enum.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int readEnum() throws java.io.IOException
		public virtual int readEnum()
		{
			return readRawVarint32();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean isAtEnd() throws java.io.IOException
		public virtual bool AtEnd
		{
			get
			{
				return !buf.Readable;
			}
		}

		/// <summary>
		/// Read an embedded message field value from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readMessage(final ByteBufMessageBuilder builder, final org.apache.pulsar.shaded.com.google.protobuf.v241.ExtensionRegistryLite extensionRegistry) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void readMessage(ByteBufMessageBuilder builder, ExtensionRegistryLite extensionRegistry)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int length = readRawVarint32();
			int length = readRawVarint32();

			int writerIdx = buf.writerIndex();
			buf.writerIndex(buf.readerIndex() + length);
			builder.mergeFrom(this, extensionRegistry);
			checkLastTagWas(0);
			buf.writerIndex(writerIdx);
		}

		private static readonly FastThreadLocal<sbyte[]> localByteArray = new FastThreadLocal<sbyte[]>();

		/// <summary>
		/// Read a {@code bytes} field value from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.pulsar.shaded.com.google.protobuf.v241.ByteString readBytes() throws java.io.IOException
		public virtual ByteString readBytes()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int size = readRawVarint32();
			int size = readRawVarint32();
			if (size == 0)
			{
				return ByteString.EMPTY;
			}
			else
			{
				sbyte[] localBuf = localByteArray.get();
				if (localBuf == null || localBuf.Length < size)
				{
					localBuf = new sbyte[Math.Max(size, 1024)];
					localByteArray.set(localBuf);
				}

				buf.readBytes(localBuf, 0, size);
				ByteString res = ByteString.copyFrom(localBuf, 0, size);
				return res;
			}
		}

		internal const int TAG_TYPE_BITS = 3;
		internal static readonly int TAG_TYPE_MASK = (1 << TAG_TYPE_BITS) - 1;

		/// <summary>
		/// Given a tag value, determines the wire type (the lower 3 bits). </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static int getTagWireType(final int tag)
		internal static int getTagWireType(int tag)
		{
			return tag & TAG_TYPE_MASK;
		}

		/// <summary>
		/// Makes a tag value given a field number and wire type. </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static int makeTag(final int fieldNumber, final int wireType)
		internal static int makeTag(int fieldNumber, int wireType)
		{
			return (fieldNumber << TAG_TYPE_BITS) | wireType;
		}

		/// <summary>
		/// Reads and discards a single field, given its tag value.
		/// </summary>
		/// <returns> {@code false} if the tag is an endgroup tag, in which case nothing is skipped. Otherwise, returns
		///         {@code true}. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean skipField(final int tag) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual bool skipField(int tag)
		{
			switch (getTagWireType(tag))
			{
			case WireFormat.WIRETYPE_VARINT:
				readInt32();
				return true;
			case WireFormat.WIRETYPE_FIXED64:
				readRawLittleEndian64();
				return true;
			case WireFormat.WIRETYPE_LENGTH_DELIMITED:
				skipRawBytes(readRawVarint32());
				return true;
			case WireFormat.WIRETYPE_START_GROUP:
				skipMessage();
				checkLastTagWas(makeTag(WireFormat.getTagFieldNumber(tag), WireFormat.WIRETYPE_END_GROUP));
				return true;
			case WireFormat.WIRETYPE_END_GROUP:
				return false;
			case WireFormat.WIRETYPE_FIXED32:
				readRawLittleEndian32();
				return true;
			default:
				throw new InvalidProtocolBufferException("Protocol message tag had invalid wire type.");
			}
		}

		/// <summary>
		/// Verifies that the last call to readTag() returned the given tag value. This is used to verify that a nested group
		/// ended with the correct end tag.
		/// </summary>
		/// <exception cref="InvalidProtocolBufferException">
		///             {@code value} does not match the last tag. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkLastTagWas(final int value) throws org.apache.pulsar.shaded.com.google.protobuf.v241.InvalidProtocolBufferException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void checkLastTagWas(int value)
		{
			if (lastTag != value)
			{
				throw new InvalidProtocolBufferException("Protocol message end-group tag did not match expected tag.");
			}
		}

		/// <summary>
		/// Read a {@code double} field value from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double readDouble() throws java.io.IOException
		public virtual double readDouble()
		{
			return Double.longBitsToDouble(readRawLittleEndian64());
		}

		/// <summary>
		/// Read a {@code float} field value from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public float readFloat() throws java.io.IOException
		public virtual float readFloat()
		{
			return Float.intBitsToFloat(readRawLittleEndian32());
		}

		/// <summary>
		/// Read a {@code uint64} field value from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long readUInt64() throws java.io.IOException
		public virtual long readUInt64()
		{
			return readRawVarint64();
		}

		/// <summary>
		/// Read an {@code int64} field value from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long readInt64() throws java.io.IOException
		public virtual long readInt64()
		{
			return readRawVarint64();
		}

		/// <summary>
		/// Read an {@code int32} field value from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int readInt32() throws java.io.IOException
		public virtual int readInt32()
		{
			return readRawVarint32();
		}

		/// <summary>
		/// Read a {@code fixed64} field value from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long readFixed64() throws java.io.IOException
		public virtual long readFixed64()
		{
			return readRawLittleEndian64();
		}

		/// <summary>
		/// Read a {@code fixed32} field value from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int readFixed32() throws java.io.IOException
		public virtual int readFixed32()
		{
			return readRawLittleEndian32();
		}

		/// <summary>
		/// Read a {@code bool} field value from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean readBool() throws java.io.IOException
		public virtual bool readBool()
		{
			return readRawVarint32() != 0;
		}

		/// <summary>
		/// Read a raw Varint from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long readRawVarint64() throws java.io.IOException
		public virtual long readRawVarint64()
		{
			int shift = 0;
			long result = 0;
			while (shift < 64)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte b = buf.readByte();
				sbyte b = buf.readByte();
				result |= (long)(b & 0x7F) << shift;
				if ((b & 0x80) == 0)
				{
					return result;
				}
				shift += 7;
			}
			throw new InvalidProtocolBufferException("CodedInputStream encountered a malformed varint.");
		}

		/// <summary>
		/// Read a raw Varint from the stream. If larger than 32 bits, discard the upper bits.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int readRawVarint32() throws java.io.IOException
		public virtual int readRawVarint32()
		{
			sbyte tmp = buf.readByte();
			if (tmp >= 0)
			{
				return tmp;
			}
			int result = tmp & 0x7f;
			if ((tmp = buf.readByte()) >= 0)
			{
				result |= tmp << 7;
			}
			else
			{
				result |= (tmp & 0x7f) << 7;
				if ((tmp = buf.readByte()) >= 0)
				{
					result |= tmp << 14;
				}
				else
				{
					result |= (tmp & 0x7f) << 14;
					if ((tmp = buf.readByte()) >= 0)
					{
						result |= tmp << 21;
					}
					else
					{
						result |= (tmp & 0x7f) << 21;
						result |= (tmp = buf.readByte()) << 28;
						if (tmp < 0)
						{
							// Discard upper 32 bits.
							for (int i = 0; i < 5; i++)
							{
								if (buf.readByte() >= 0)
								{
									return result;
								}
							}
							throw new InvalidProtocolBufferException("CodedInputStream encountered a malformed varint.");
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Read a 32-bit little-endian integer from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int readRawLittleEndian32() throws java.io.IOException
		public virtual int readRawLittleEndian32()
		{
			return buf.readIntLE();

		}

		/// <summary>
		/// Read a 64-bit little-endian integer from the stream. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long readRawLittleEndian64() throws java.io.IOException
		public virtual long readRawLittleEndian64()
		{
			return buf.readLongLE();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long readSFixed64() throws java.io.IOException
		public virtual long readSFixed64()
		{
			return readRawLittleEndian64();
		}

		/// <summary>
		/// Reads and discards an entire message. This will read either until EOF or until an endgroup tag, whichever comes
		/// first.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void skipMessage() throws java.io.IOException
		public virtual void skipMessage()
		{
			while (true)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int tag = readTag();
				int tag = readTag();
				if (tag == 0 || !skipField(tag))
				{
					return;
				}
			}
		}

		/// <summary>
		/// Reads and discards {@code size} bytes.
		/// </summary>
		/// <exception cref="InvalidProtocolBufferException">
		///             The end of the stream or the current limit was reached. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void skipRawBytes(final int size) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		public virtual void skipRawBytes(int size)
		{
			if (size < 0)
			{
				throw new InvalidProtocolBufferException("CodedInputStream encountered an embedded string or message " + "which claimed to have negative size.");
			}

			if (size > buf.readableBytes())
			{
				throw new InvalidProtocolBufferException("While parsing a protocol message, the input ended unexpectedly " + "in the middle of a field.  This could mean either than the " + "input has been truncated or that an embedded message " + "misreported its own length.");
			}

			buf.readerIndex(buf.readerIndex() + size);
		}
	}

}