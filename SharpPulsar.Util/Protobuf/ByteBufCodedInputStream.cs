﻿using System;
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
 * This file is derived from Google ProcolBuffer CodedInputStream class
 * with adaptations to work directly with Netty ByteBuf instances.
 */

namespace SharpPulsar.Utility.Protobuf
{
	public class ByteBufCodedInputStream
	{
		public interface ByteBufMessageBuilder
		{
			ByteBufMessageBuilder MergeFrom(ByteBufCodedInputStream input, ExtensionRegistry ext);
		}

		private IByteBuffer _buf;
		private int _lastTag;

		internal static ThreadLocalPool<ByteBufCodedInputStream> Pool = new ThreadLocalPool<ByteBufCodedInputStream>(handle => new ByteBufCodedInputStream(handle), 1, true);

		internal ThreadLocalPool.Handle Handle;
		private ByteBufCodedInputStream(ThreadLocalPool.Handle handle)
		{
			Handle = handle;
		}


		public static ByteBufCodedInputStream Get(IByteBuffer buf)
		{
			var stream = Pool.Take();
			stream._buf = buf;
			stream._lastTag = 0;
			return stream;
		}
		

		public virtual void Recycle()
		{
			this._buf = null;
			if (Handle != null)
			{
				Handle.Release(this);
			}
		}

		public virtual int ReadTag()
		{
			if (AtEnd)
			{
				_lastTag = 0;
				return 0;
			}

			_lastTag = ReadRawVarint32();
			if (WireFormat.GetTagFieldNumber((uint)_lastTag) == 0)
			{
				// If we actually read zero (or any tag number corresponding to field
				// number zero), that's not a valid tag.
				throw new Exception("CodedInputStream encountered a malformed varint.");
			}
			return _lastTag;
		}

		/// <summary>
		/// Read a {@code uint32} field value from the stream. </summary>
		public virtual int ReadUInt32()
		{
			return ReadRawVarint32();
		}

		/// <summary>
		/// Read an enum field value from the stream. Caller is responsible for converting the numeric value to an actual
		/// enum.
		/// </summary>
		public virtual int ReadEnum()
		{
			return ReadRawVarint32();
		}

		public virtual bool AtEnd
		{
			get
			{
				return !_buf.IsReadable();
			}
		}

		/// <summary>
		/// Read an embedded message field value from the stream. </summary>
		public virtual void ReadMessage(ByteBufMessageBuilder builder, ExtensionRegistry extensionRegistry)
		{
			var length = ReadRawVarint32();

			var writerIdx = _buf.WriterIndex;
			_buf.SetWriterIndex(_buf.ReaderIndex + length);
			builder.MergeFrom(this, extensionRegistry);
			CheckLastTagWas(0);
			_buf.SetWriterIndex(writerIdx);
		}

		private static readonly FastThreadLocal<sbyte[]> LocalByteArray = new FastThreadLocal<sbyte[]>();

		/// <summary>
		/// Read a {@code bytes} field value from the stream. </summary>
		public virtual ByteString ReadBytes()
		{
			var size = ReadRawVarint32();
			if (size == 0)
			{
				return ByteString.Empty;
			}
			else
			{
				var localBuf = LocalByteArray.Value;
				if (localBuf == null || localBuf.Length < size)
				{
					localBuf = new sbyte[Math.Max(size, 1024)];
					LocalByteArray.Value = localBuf;
				}

                var local = (byte[]) (object) localBuf;
				_buf.ReadBytes(local, 0, size);
				var res = ByteString.CopyFrom(local, 0, size);
				return res;
			}
		}

		internal const int TagTypeBits = 3;
		internal static readonly int TagTypeMask = (1 << TagTypeBits) - 1;

		/// <summary>
		/// Given a tag value, determines the wire type (the lower 3 bits). </summary>
		internal static int GetTagWireType(int tag)
		{
			return tag & TagTypeMask;
		}

		/// <summary>
		/// Makes a tag value given a field number and wire type. </summary>
		internal static int MakeTag(int fieldNumber, int wireType)
		{
			return (fieldNumber << TagTypeBits) | wireType;
		}

		/// <summary>
		/// Reads and discards a single field, given its tag value.
		/// </summary>
		/// <returns> {@code false} if the tag is an endgroup tag, in which case nothing is skipped. Otherwise, returns
		///         {@code true}. </returns>
		public virtual bool SkipField(int tag)
		{
			switch (GetTagWireType(tag))
			{
			case (int)WireFormat.WireType.Varint:
				ReadInt32();
				return true;
			case (int)WireFormat.WireType.Fixed64:
				ReadRawLittleEndian64();
				return true;
			case (int)WireFormat.WireType.LengthDelimited:
				SkipRawBytes(ReadRawVarint32());
				return true;
			case (int)WireFormat.WireType.StartGroup:
				SkipMessage();
				CheckLastTagWas(MakeTag(WireFormat.GetTagFieldNumber((uint)tag), (int)WireFormat.WireType.EndGroup));
				return true;
			case (int)WireFormat.WireType.EndGroup:
				return false;
			case (int)WireFormat.WireType.Fixed32:
				ReadRawLittleEndian32();
				return true;
			default:
				throw new Exception("Protocol message tag had invalid wire type.");
			}
		}

		/// <summary>
		/// Verifies that the last call to readTag() returned the given tag value. This is used to verify that a nested group
		/// ended with the correct end tag.
		/// </summary>
		/// <exception cref="InvalidProtocolBufferException">
		///             {@code value} does not match the last tag. </exception>
		public virtual void CheckLastTagWas(int value)
		{
			if (_lastTag != value)
			{
				throw new Exception("Protocol message end-group tag did not match expected tag.");
			}
		}

		/// <summary>
		/// Read a {@code double} field value from the stream. </summary>
		public virtual double ReadDouble()
		{
			return Convert.ToDouble(ReadRawLittleEndian64());
		}

		/// <summary>
		/// Read a {@code float} field value from the stream. </summary>
		public virtual float ReadFloat()
		{
			return Convert.ToSingle(ReadRawLittleEndian32());
		}

		/// <summary>
		/// Read a {@code uint64} field value from the stream. </summary>
		public virtual long ReadUInt64()
		{
			return ReadRawVarint64();
		}

		/// <summary>
		/// Read an {@code int64} field value from the stream. </summary>
		public virtual long ReadInt64()
		{
			return ReadRawVarint64();
		}

		/// <summary>
		/// Read an {@code int32} field value from the stream. </summary>
		public virtual int ReadInt32()
		{
			return ReadRawVarint32();
		}

		/// <summary>
		/// Read a {@code fixed64} field value from the stream. </summary>
		public virtual long ReadFixed64()
		{
			return ReadRawLittleEndian64();
		}

		/// <summary>
		/// Read a {@code fixed32} field value from the stream. </summary>
		public virtual int ReadFixed32()
		{
			return ReadRawLittleEndian32();
		}

		/// <summary>
		/// Read a {@code bool} field value from the stream. </summary>
		public virtual bool ReadBool()
		{
			return ReadRawVarint32() != 0;
		}

		/// <summary>
		/// Read a raw Varint from the stream. </summary>
		public virtual long ReadRawVarint64()
		{
			var shift = 0;
			long result = 0;
			while (shift < 64)
			{
				var b = (sbyte)(object)_buf.ReadByte();
				result |= (long)(b & 0x7F) << shift;
				if ((b & 0x80) == 0)
				{
					return result;
				}
				shift += 7;
			}
			throw new Exception("CodedInputStream encountered a malformed varint.");
		}

		/// <summary>
		/// Read a raw Varint from the stream. If larger than 32 bits, discard the upper bits.
		/// </summary>
		public virtual int ReadRawVarint32()
		{
			var tmp = (sbyte)(object)_buf.ReadByte();
			if (tmp >= 0)
			{
				return tmp;
			}
			var result = tmp & 0x7f;
			if ((tmp = (sbyte)(object)_buf.ReadByte()) >= 0)
			{
				result |= tmp << 7;
			}
			else
			{
				result |= (tmp & 0x7f) << 7;
				if ((tmp = (sbyte)(object)_buf.ReadByte()) >= 0)
				{
					result |= tmp << 14;
				}
				else
				{
					result |= (tmp & 0x7f) << 14;
					if ((tmp = (sbyte)(object)_buf.ReadByte()) >= 0)
					{
						result |= tmp << 21;
					}
					else
					{
						result |= (tmp & 0x7f) << 21;
						result |= (tmp = (sbyte)(object)_buf.ReadByte()) << 28;
						if (tmp < 0)
						{
							// Discard upper 32 bits.
							for (var i = 0; i < 5; i++)
							{
								if (_buf.ReadByte() >= 0)
								{
									return result;
								}
							}
							throw new Exception("CodedInputStream encountered a malformed varint.");
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Read a 32-bit little-endian integer from the stream. </summary>
		public virtual int ReadRawLittleEndian32()
		{
			return _buf.ReadIntLE();

		}

		/// <summary>
		/// Read a 64-bit little-endian integer from the stream. </summary>
		public virtual long ReadRawLittleEndian64()
		{
			return _buf.ReadLongLE();
		}
		public virtual long RadSFixed64()
		{
			return ReadRawLittleEndian64();
		}

		/// <summary>
		/// Reads and discards an entire message. This will read either until EOF or until an endgroup tag, whichever comes
		/// first.
		/// </summary>
		public virtual void SkipMessage()
		{
			while (true)
			{
				var tag = ReadTag();
				if (tag == 0 || !SkipField(tag))
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
		public virtual void SkipRawBytes(int size)
		{
			if (size < 0)
			{
				throw new Exception("CodedInputStream encountered an embedded string or message " + "which claimed to have negative size.");
			}

			if (size > _buf.ReadableBytes)
			{
				throw new Exception("While parsing a protocol message, the input ended unexpectedly " + "in the middle of a field.  This could mean either than the " + "input has been truncated or that an embedded message " + "misreported its own length.");
			}

			_buf.SetReaderIndex(_buf.ReaderIndex + size);
		}
	}

}