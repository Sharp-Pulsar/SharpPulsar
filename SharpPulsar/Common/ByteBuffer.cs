//-------------------------------------------------------------------------------------------
//	Copyright © 2007 - 2020 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to replicate the java.nio.ByteBuffer class in C#.
//
//	Instances are only obtainable via the static 'allocate' method.
//
//	Some methods are not available:
//		All methods which create shared views of the buffer such as: array,
//		asCharBuffer, asDoubleBuffer, asFloatBuffer, asIntBuffer, asLongBuffer,
//		asReadOnlyBuffer, asShortBuffer, duplicate, slice, & wrap.
//
//		Other methods such as: mark, reset, isReadOnly, order, compareTo,
//		arrayOffset, & the limit setter method.
//-------------------------------------------------------------------------------------------
public class ByteBuffer
{
	//'Mode' is only used to determine whether to return data length or capacity from the 'limit' method:
	private enum Mode
	{
		Read,
		Write
	}
	private Mode mode;

	private System.IO.MemoryStream stream;
	private System.IO.BinaryReader reader;
	private System.IO.BinaryWriter writer;

	private ByteBuffer()
	{
		stream = new System.IO.MemoryStream();
		reader = new System.IO.BinaryReader(stream);
		writer = new System.IO.BinaryWriter(stream);
	}

	~ByteBuffer()
	{
		reader.Close();
		writer.Close();
		stream.Close();
		stream.Dispose();
	}

	public static ByteBuffer Allocate(int capacity)
	{
		ByteBuffer buffer = new ByteBuffer();
		buffer.stream.Capacity = capacity;
		buffer.mode = Mode.Write;
		return buffer;
	}

	public static ByteBuffer AllocateDirect(int capacity)
	{
		//this wrapper class makes no distinction between 'allocate' & 'allocateDirect'
		return Allocate(capacity);
	}

	public int Capacity()
	{
		return stream.Capacity;
	}

	public ByteBuffer Flip()
	{
		mode = Mode.Read;
		stream.SetLength(stream.Position);
		stream.Position = 0;
		return this;
	}

	public ByteBuffer Clear()
	{
		mode = Mode.Write;
		stream.Position = 0;
		return this;
	}

	public ByteBuffer Compact()
	{
		mode = Mode.Write;
		System.IO.MemoryStream newStream = new System.IO.MemoryStream(stream.Capacity);
		stream.CopyTo(newStream);
		stream = newStream;
		return this;
	}

	public ByteBuffer Rewind()
	{
		stream.Position = 0;
		return this;
	}

	public long Limit()
	{
		if (mode == Mode.Write)
			return stream.Capacity;
		else
			return stream.Length;
	}

	public long Position()
	{
		return stream.Position;
	}

	public ByteBuffer Position(long newPosition)
	{
		stream.Position = newPosition;
		return this;
	}

	public long Remaining()
	{
		return this.Limit() - this.Position();
	}

	public bool hasRemaining()
	{
		return this.Remaining() > 0;
	}

	public int Get()
	{
		return stream.ReadByte();
	}

	public ByteBuffer Get(byte[] dst, int offset, int length)
	{
		stream.Read(dst, offset, length);
		return this;
	}

	public ByteBuffer Put(byte b)
	{
		stream.WriteByte(b);
		return this;
	}

	public ByteBuffer Put(byte[] src, int offset, int length)
	{
		stream.Write(src, offset, length);
		return this;
	}

	public bool Equals(ByteBuffer other)
	{
		if (other != null && this.Remaining() == other.Remaining())
		{
			long thisOriginalPosition = this.Position();
			long otherOriginalPosition = other.Position();

			bool differenceFound = false;
			while (stream.Position < stream.Length)
			{
				if (this.Get() != other.Get())
				{
					differenceFound = true;
					break;
				}
			}

			this.Position(thisOriginalPosition);
			other.Position(otherOriginalPosition);

			return ! differenceFound;
		}
		else
			return false;
	}

	//methods using the internal BinaryReader:
	public char GetChar()
	{
		return reader.ReadChar();
	}
	public char GetChar(int index)
	{
		long originalPosition = stream.Position;
		stream.Position = index;
		char value = reader.ReadChar();
		stream.Position = originalPosition;
		return value;
	}
	public double GetDouble()
	{
		return reader.ReadDouble();
	}
	public double GetDouble(int index)
	{
		long originalPosition = stream.Position;
		stream.Position = index;
		double value = reader.ReadDouble();
		stream.Position = originalPosition;
		return value;
	}
	public float GetFloat()
	{
		return reader.ReadSingle();
	}
	public float GetFloat(int index)
	{
		long originalPosition = stream.Position;
		stream.Position = index;
		float value = reader.ReadSingle();
		stream.Position = originalPosition;
		return value;
	}
	public int GetInt()
	{
		return reader.ReadInt32();
	}
	public int GetInt(int index)
	{
		long originalPosition = stream.Position;
		stream.Position = index;
		int value = reader.ReadInt32();
		stream.Position = originalPosition;
		return value;
	}
	public long GetLong()
	{
		return reader.ReadInt64();
	}
	public long GetLong(int index)
	{
		long originalPosition = stream.Position;
		stream.Position = index;
		long value = reader.ReadInt64();
		stream.Position = originalPosition;
		return value;
	}
	public short GetShort()
	{
		return reader.ReadInt16();
	}
	public short GetShort(int index)
	{
		long originalPosition = stream.Position;
		stream.Position = index;
		short value = reader.ReadInt16();
		stream.Position = originalPosition;
		return value;
	}

	//methods using the internal BinaryWriter:
	public ByteBuffer PutChar(char value)
	{
		writer.Write(value);
		return this;
	}
	public ByteBuffer PutChar(int index, char value)
	{
		long originalPosition = stream.Position;
		stream.Position = index;
		writer.Write(value);
		stream.Position = originalPosition;
		return this;
	}
	public ByteBuffer PutDouble(double value)
	{
		writer.Write(value);
		return this;
	}
	public ByteBuffer PutDouble(int index, double value)
	{
		long originalPosition = stream.Position;
		stream.Position = index;
		writer.Write(value);
		stream.Position = originalPosition;
		return this;
	}
	public ByteBuffer PutFloat(float value)
	{
		writer.Write(value);
		return this;
	}
	public ByteBuffer PutFloat(int index, float value)
	{
		long originalPosition = stream.Position;
		stream.Position = index;
		writer.Write(value);
		stream.Position = originalPosition;
		return this;
	}
	public ByteBuffer PutInt(int value)
	{
		writer.Write(value);
		return this;
	}
	public ByteBuffer PutInt(int index, int value)
	{
		long originalPosition = stream.Position;
		stream.Position = index;
		writer.Write(value);
		stream.Position = originalPosition;
		return this;
	}
	public ByteBuffer PutLong(long value)
	{
		writer.Write(value);
		return this;
	}
	public ByteBuffer PutLong(int index, long value)
	{
		long originalPosition = stream.Position;
		stream.Position = index;
		writer.Write(value);
		stream.Position = originalPosition;
		return this;
	}
	public ByteBuffer PutShort(short value)
	{
		writer.Write(value);
		return this;
	}
	public ByteBuffer PutShort(int index, short value)
	{
		long originalPosition = stream.Position;
		stream.Position = index;
		writer.Write(value);
		stream.Position = originalPosition;
		return this;
	}
}