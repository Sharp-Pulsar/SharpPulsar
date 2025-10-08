//https://github.com/eaba/EasyTestSocket.git
namespace SharpPulsar.Shared.Buf;

public class ByteBuf : IDisposable
{
    /// <summary>
    /// 数据
    /// </summary>
    public byte[] Data { get; protected set; }

    /// <summary>
    /// 读下标
    /// </summary>
    public int ReaderIndex { get; set; }

    /// <summary>
    /// 写下标
    /// </summary>
    public int WriterIndex { get; set; }

    public int ReaderMarker { get; set; } = 0;
    public int WriterMarker { get; set; } = 0;

    /// <summary>
    /// 初始容量
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// 最大容量
    /// </summary>
    public int MaxCapacity { get; }

    /// <summary>
    /// 是否是大端模式
    /// </summary>
    public bool IsBigEndian { get; set; } = true;

    /// <summary>
    /// 引用次数计数器
    /// </summary>
    private int _referenceCount;
    public int ReferenceCount => _referenceCount;

    /// <summary>
    /// deallocate
    /// </summary>
    internal Action<ByteBuf>? Deallocate;

    /// <summary>
    /// 包裹的buf
    /// </summary>
    private List<ByteBuf>? _wrappedByteBufList;

    /// <summary>
    /// 构造函数
    /// </summary>
    public ByteBuf() : this(256)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="capacity"></param>
    /// <param name="maxCapacity"></param>
    public ByteBuf(int capacity, int maxCapacity = int.MaxValue)
    {
        if (capacity < 0 || capacity > maxCapacity)
        {
            throw new ArgumentException($"capacity:{capacity} maxCapacity:{maxCapacity} excepted capacity greater than zero and less than maxCapacity");
        }

        Data = new byte[capacity];
        ReaderIndex = 0;
        WriterIndex = 0;
        Capacity = capacity;
        MaxCapacity = maxCapacity;
        _referenceCount = 1;
    }

    public ByteBuf(byte[] data, int maxCapacity = int.MaxValue)
    {
        Data = data;
        ReaderIndex = 0;
        WriterIndex = data.Length;
        Capacity = data.Length;
        MaxCapacity = maxCapacity;
        _referenceCount = 1;
    }

    public ByteBuf(byte[] data, int dataLen, int maxCapacity = int.MaxValue)
    {
        Data = data;
        ReaderIndex = 0;
        WriterIndex = dataLen;
        Capacity = dataLen;
        MaxCapacity = maxCapacity;
        _referenceCount = 1;
    }


    /// <summary>
    /// 可读字节数
    /// </summary>
    public int ReadableBytes => this.WriterIndex - this.ReaderIndex;

    /// <summary>
    /// 可写字节数
    /// </summary>
    public int WritableBytes => this.Capacity - this.WriterIndex;

    /// <summary>
    /// 跳过指定长度字节
    /// </summary>
    /// <param name="len"></param>
    public void SkipBytes(int len)
    {
        ReaderIndex += len;
        if (ReaderIndex > Capacity)
        {
            ReaderIndex = Capacity;
        }
    }

    /// <summary>
    /// 写bool值
    /// </summary>
    /// <param name="value"></param>
    public void WriteBool(bool value)
    {
        WriteByte(value ? 1 : 0);
    }

    /// <summary>
    /// 写Short
    /// </summary>
    /// <param name="value"></param>
    public void WriteShort(int value)
    {
        EnsureAccessible();
        EnsureWritable(2);
        if (IsBigEndian)
        {
            ByteUtil.SetShort(Data, WriterIndex, value);
        }
        else
        {
            ByteUtil.SetShortLE(Data, WriterIndex, value);
        }
        WriterIndex += 2;
    }

    /// <summary>
    /// 写Int
    /// </summary>
    /// <param name="value"></param>
    public void WriteInt(int value)
    {
        EnsureAccessible();
        EnsureWritable(4);
        if (IsBigEndian)
        {
            ByteUtil.SetInt(Data, WriterIndex, value);
        }
        else
        {
            ByteUtil.SetIntLE(Data, WriterIndex, value);
        }
        WriterIndex += 4;
    }

    /// <summary>
    /// 写Long
    /// </summary>
    /// <param name="value"></param>
    public void WriteLong(long value)
    {
        EnsureAccessible();
        EnsureWritable(8);
        if (IsBigEndian)
        {
            ByteUtil.SetLong(Data, WriterIndex, value);
        }
        else
        {
            ByteUtil.SetLongLE(Data, WriterIndex, value);
        }
        WriterIndex += 8;
    }

    /// <summary>
    /// 写Float型
    /// </summary>
    /// <param name="value"></param>
    public void WriteFloat(float value)
    {
        WriteInt(BitConverter.SingleToInt32Bits(value));
    }

    /// <summary>
    /// 写Double
    /// </summary>
    /// <param name="value"></param>
    public void WriteDouble(double value)
    {
        WriteLong(BitConverter.DoubleToInt64Bits(value));
    }

    /// <summary>
    /// 写字节
    /// </summary>
    /// <param name="value"></param>
    public void WriteByte(int value)
    {
        EnsureAccessible();
        EnsureWritable(1);
        ByteUtil.SetByte(Data, WriterIndex, value);
        WriterIndex += 1;
    }

    /// <summary>
    /// 写字节数组
    /// </summary>
    /// <param name="array"></param>
    public void WriteBytes(byte[] array)
    {
        WriteBytes(array, 0, array.Length);
    }

    /// <summary>
    /// 写字节数组
    /// </summary>
    /// <param name="array"></param>
    /// <param name="srcIndex"></param>
    /// <param name="len"></param>
    public void WriteBytes(byte[] array, int srcIndex, int len)
    {
        EnsureAccessible();
        EnsureWritable(len);

        Array.Copy(array, srcIndex, Data, WriterIndex, len);
        WriterIndex += len;
    }

    public void WriteEmpty(int len)
    {
        EnsureAccessible();
        EnsureWritable(len);
        for (var i = 0; i < len; i++)
        {
            Data[i + WriterIndex] = 0;
        }
        WriterIndex += len;
    }

    /// <summary>
    /// 写字节数组
    /// </summary>
    /// <param name="buff"></param>
    public void WriteBytes(ByteBuf buff)
    {
        WriteBytes(buff, buff.ReaderIndex, buff.ReadableBytes);
    }

    /// <summary>
    /// 写字节数组
    /// </summary>
    /// <param name="buff"></param>
    /// <param name="srcIndex"></param>
    /// <param name="len"></param>
    public void WriteBytes(ByteBuf buff, int srcIndex, int len)
    {
        WriteBytes(buff.Data, srcIndex, len);
    }


    /// <summary>
    /// 设置bool值
    /// </summary>
    /// <param name="value"></param>
    public void SetBool(int startIndex, bool value)
    {
        SetByte(startIndex, value ? 1 : 0);
    }

    /// <summary>
    /// 设置Short
    /// </summary>
    /// <param name="value"></param>
    public void SetShort(int startIndex, int value)
    {
        EnsureAccessible();
        CheckIndex(startIndex, 2);
        if (IsBigEndian)
        {
            ByteUtil.SetShort(Data, startIndex, value);
        }
        else
        {
            ByteUtil.SetShortLE(Data, startIndex, value);
        }
    }

    /// <summary>
    /// 设置Int
    /// </summary>
    /// <param name="value"></param>
    public void SetInt(int startIndex, int value)
    {
        EnsureAccessible();
        CheckIndex(startIndex, 4);
        if (IsBigEndian)
        {
            ByteUtil.SetInt(Data, startIndex, value);
        }
        else
        {
            ByteUtil.SetIntLE(Data, startIndex, value);
        }
    }

    /// <summary>
    /// 设置Long
    /// </summary>
    /// <param name="value"></param>
    public void SetLong(int startIndex, long value)
    {
        EnsureAccessible();
        CheckIndex(startIndex, 8);
        if (IsBigEndian)
        {
            ByteUtil.SetLong(Data, startIndex, value);
        }
        else
        {
            ByteUtil.SetLongLE(Data, startIndex, value);
        }
    }

    /// <summary>
    /// 设置Float型
    /// </summary>
    /// <param name="value"></param>
    public void SetFloat(int startIndex, float value)
    {
        SetInt(startIndex, BitConverter.SingleToInt32Bits(value));
    }

    /// <summary>
    /// 设置Double
    /// </summary>
    /// <param name="value"></param>
    public void SetDouble(int startIndex, double value)
    {
        SetLong(startIndex, BitConverter.DoubleToInt64Bits(value));
    }

    /// <summary>
    /// 设置字节
    /// </summary>
    /// <param name="value"></param>
    public void SetByte(int startIndex, int value)
    {
        EnsureAccessible();
        CheckIndex(startIndex, 1);

        ByteUtil.SetByte(Data, startIndex, value);
    }

    /// <summary>
    /// 设置字节数组
    /// </summary>
    /// <param name="array"></param>
    public void SetBytes(int startIndex, byte[] array)
    {
        SetBytes(startIndex, array, 0, array.Length);
    }

    /// <summary>
    /// 设置字节数组
    /// </summary>
    /// <param name="array"></param>
    /// <param name="srcIndex"></param>
    /// <param name="len"></param>
    public void SetBytes(int startIndex, byte[] array, int srcIndex, int len)
    {
        EnsureAccessible();
        CheckIndex(startIndex, len);

        Array.Copy(array, srcIndex, Data, startIndex, len);
    }

    /// <summary>
    /// 设置字节数组
    /// </summary>
    /// <param name="buff"></param>
    public void SetBytes(int startIndex, ByteBuf buff)
    {
        SetBytes(startIndex, buff, buff.ReaderIndex, buff.ReadableBytes);
    }

    /// <summary>
    /// 设置节数组
    /// </summary>
    /// <param name="buff"></param>
    /// <param name="srcIndex"></param>
    /// <param name="len"></param>
    public void SetBytes(int startIndex, ByteBuf buff, int srcIndex, int len)
    {
        SetBytes(startIndex, buff.Data, srcIndex, len);
    }

    /// <summary>
    /// 读取bool值
    /// </summary>
    /// <returns></returns>
    public bool ReadBool()
    {
        return ReadByte() != 0;
    }

    /// <summary>
    /// 读取byte字节
    /// </summary>
    /// <returns></returns>
    public byte ReadByte()
    {
        var v = GetByte();
        ReaderIndex += 1;

        return v;
    }

    /// <summary>
    /// 读取int值
    /// </summary>
    /// <returns></returns>
    public short ReadShort()
    {
        var v = GetShort();
        ReaderIndex += 2;

        return v;
    }

    /// <summary>
    /// 读取int值
    /// </summary>
    /// <returns></returns>
    public ushort ReadUShort()
    {
        var v = GetUShort();
        ReaderIndex += 2;

        return v;
    }

    /// <summary>
    /// 读取int值
    /// </summary>
    /// <returns></returns>
    public int ReadInt()
    {
        var v = GetInt();
        ReaderIndex += 4;

        return v;
    }

    /// <summary>
    /// 读取Long值
    /// </summary>
    /// <returns></returns>
    public long ReadLong()
    {
        var v = GetLong();
        ReaderIndex += 8;

        return v;
    }

    /// <summary>
    /// 读取Double
    /// </summary>
    /// <returns></returns>
    public double ReadDouble()
    {
        return BitConverter.Int64BitsToDouble(ReadLong());
    }

    /// <summary>
    /// 读取Float
    /// </summary>
    /// <returns></returns>
    public float ReadFloat()
    {
        return BitConverter.Int32BitsToSingle(ReadInt());
    }

    /// <summary>
    /// 读取字节
    /// </summary>
    /// <param name="dest"></param>
    public void ReadBytes(byte[] dest)
    {
        ReadBytes(dest, 0, dest.Length);
    }

    /// <summary>
    /// 读取字节
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="dstIndex"></param>
    /// <param name="len"></param>
    public void ReadBytes(byte[] dest, int dstIndex, int len)
    {
        GetBytes(dest, dstIndex, len);
        ReaderIndex += len;
    }

    /// <summary>
    /// 读取字节
    /// </summary>
    /// <param name="len"></param>
    public ByteBuf ReadBytes(int len)
    {
        var buff = new ByteBuf(len);
        ReadBytes(buff);

        return buff;
    }

    /// <summary>
    /// 读取字节
    /// </summary>
    /// <param name="dest"></param>
    public void ReadBytes(ByteBuf dest)
    {
        var len = dest.WritableBytes;
        Array.Copy(Data, ReaderIndex, dest.Data, dest.WriterIndex, dest.WritableBytes);
        dest.WriterIndex += len;
        ReaderIndex += len;
    }

    /// <summary>
    /// 读取bool值
    /// </summary>
    /// <returns></returns>
    public bool GetBool()
    {
        return GetByte() != 0;
    }

    /// <summary>
    /// 读取bool值
    /// </summary>
    /// <returns></returns>
    public bool GetBool(int index)
    {
        return GetByte(index) != 0;
    }

    /// <summary>
    /// 读取byte字节
    /// </summary>
    /// <returns></returns>
    public byte GetByte()
    {
        return ByteUtil.GetByte(Data, ReaderIndex);
    }

    /// <summary>
    /// 读取byte字节
    /// </summary>
    /// <returns></returns>
    public byte GetByte(int index)
    {
        return ByteUtil.GetByte(Data, index);
    }

    /// <summary>
    /// 读取int值
    /// </summary>
    /// <returns></returns>
    public int GetInt()
    {
        return ByteUtil.GetInt(Data, ReaderIndex);
    }

    /// <summary>
    /// 读取int值
    /// </summary>
    /// <returns></returns>
    public int GetInt(int index)
    {
        return ByteUtil.GetInt(Data, index);
    }

    /// <summary>
    /// 读取short值
    /// </summary>
    /// <returns></returns>
    public short GetShort()
    {
        return ByteUtil.GetShort(Data, ReaderIndex);
    }

    /// <summary>
    /// 读取short值
    /// </summary>
    /// <returns></returns>
    public short GetShort(int index)
    {
        return ByteUtil.GetShort(Data, index);
    }

    /// <summary>
    /// 读取ushort值
    /// </summary>
    /// <returns></returns>
    public ushort GetUShort()
    {
        return ByteUtil.GetUShort(Data, ReaderIndex);
    }

    /// <summary>
    /// 读取ushort值
    /// </summary>
    /// <returns></returns>
    public ushort GetUShort(int index)
    {
        return ByteUtil.GetUShort(Data, index);
    }

    /// <summary>
    /// 读取Long值
    /// </summary>
    /// <returns></returns>
    public long GetLong()
    {
        return ByteUtil.GetLong(Data, ReaderIndex);
    }

    /// <summary>
    /// 读取Long值
    /// </summary>
    /// <returns></returns>
    public long GetLong(int index)
    {
        return ByteUtil.GetLong(Data, index);
    }

    /// <summary>
    /// 读取Double
    /// </summary>
    /// <returns></returns>
    public double GetDouble()
    {
        return BitConverter.Int64BitsToDouble(GetLong());
    }
    public void MarkReaderIndex()
    {
        ReaderMarker = ReaderIndex;
    }

    public void MarkWriterIndex()
    {
        WriterMarker = WriterIndex;
    }
    /// <summary>
    /// 读取Double
    /// </summary>
    /// <returns></returns>
    public double GetDouble(int index)
    {
        return BitConverter.Int64BitsToDouble(GetLong(index));
    }

    /// <summary>
    /// 读取Float
    /// </summary>
    /// <returns></returns>
    public float GetFloat()
    {
        return BitConverter.Int32BitsToSingle(GetInt());
    }

    /// <summary>
    /// 读取Float
    /// </summary>
    /// <returns></returns>
    public float GetFloat(int index)
    {
        return BitConverter.Int32BitsToSingle(GetInt(index));
    }

    /// <summary>
    /// 读取字节
    /// </summary>
    /// <param name="dest"></param>
    public void GetBytes(byte[] dest)
    {
        GetBytes(dest, 0, dest.Length);
    }

    /// <summary>
    /// 读取字节
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="dstIndex"></param>
    /// <param name="len"></param>
    public void GetBytes(byte[] dest, int dstIndex, int len)
    {
        Array.Copy(Data, ReaderIndex, dest, dstIndex, len);
    }

    /// <summary>
    /// 读取字节
    /// </summary>
    /// <param name="len"></param>
    public ByteBuf GetBytes(int len)
    {
        var buff = new ByteBuf(len);
        GetBytes(buff);

        return buff;
    }

    /// <summary>
    /// 读取字节
    /// </summary>
    /// <param name="buff"></param>
    public void GetBytes(ByteBuf buff)
    {
        Array.Copy(Data, ReaderIndex, buff.Data, buff.WriterIndex, buff.WritableBytes);
    }


    /// <summary>
    /// 读取字节
    /// </summary>
    /// <param name="dest"></param>
    public void GetBytes(int startIndex, byte[] dest)
    {
        GetBytes(startIndex, dest, 0, dest.Length);
    }

    /// <summary>
    /// 读取字节
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="dstIndex"></param>
    /// <param name="len"></param>
    public void GetBytes(int startIndex, byte[] dest, int dstIndex, int len)
    {
        Array.Copy(Data, startIndex, dest, dstIndex, len);
    }

    /// <summary>
    /// 读取字节
    /// </summary>
    /// <param name="len"></param>
    public ByteBuf GetBytes(int startIndex, int len)
    {
        var buff = new ByteBuf(len);
        GetBytes(startIndex, buff);

        return buff;
    }

    /// <summary>
    /// 读取字节
    /// </summary>
    /// <param name="buff"></param>
    public void GetBytes(int startIndex, ByteBuf buff)
    {
        Array.Copy(Data, startIndex, buff.Data, buff.WriterIndex, buff.WritableBytes);
    }

    /// <summary>
    /// 引用+1
    /// </summary>
    public virtual void Retain()
    {
        Interlocked.Increment(ref _referenceCount);
    }

    public virtual void Release()
    {
        if (_wrappedByteBufList != null)
        {
            foreach (var buf in _wrappedByteBufList)
            {
                buf.Release();
            }

            _wrappedByteBufList = null;
        }

        var v = Interlocked.Decrement(ref _referenceCount);
        if (v == 0)
        {
            Reset();
            Deallocate?.Invoke(this);
        }
    }

    /// <summary>
    /// 检查容量大小
    /// </summary>
    /// <param name="capacity"></param>
    public void EnsureCapacity(int capacity)
    {
        EnsureWritable(capacity);
    }

    /// <summary>
    /// 重置buff
    /// </summary>
    public void Reset()
    {
        ReaderIndex = 0;
        WriterIndex = 0;
    }

    /// <summary>
    /// buff回收
    /// </summary>
    public void Dispose()
    {
        Release();
    }

    /// <summary>
    /// 检查可访问性
    /// </summary>
    protected void EnsureAccessible()
    {
        if (this.ReferenceCount == 0)
        {
            throw new Exception($"illegal reference count {ReferenceCount}");
        }
    }

    /// <summary>
    /// 检查Index合法性
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="dataLen"></param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    protected void CheckIndex(int startIndex, int dataLen)
    {
        if (startIndex < 0 || dataLen < 0 || startIndex + dataLen > Capacity)
        {
            throw new IndexOutOfRangeException($"startIndex:{startIndex} dataLen:{dataLen} excepted startIndex and dataLen greater than zero and less than capacity");
        }
    }

    /// <summary>
    /// 检查是否可写
    /// </summary>
    /// <param name="minWritableBytes"></param>
    protected void EnsureWritable(int minWritableBytes)
    {
        if (minWritableBytes < 0)
        {
            throw new ArgumentException($"minWritableBytes {minWritableBytes}, excepted >= 0");
        }

        if (minWritableBytes <= WritableBytes)
        {
            return;
        }

        if (minWritableBytes > MaxCapacity - WriterIndex)
        {
            throw new IndexOutOfRangeException($"writerIndex({WriterIndex}) + minWritableBytes({minWritableBytes}) exceeds maxCapacity({MaxCapacity})");
        }

        var minNewCapacity = WriterIndex + minWritableBytes;
        var newCapacity = calculateNewCapacity(minNewCapacity);
        var fastCapacity = WriterIndex + WritableBytes;
        if (newCapacity > fastCapacity && minNewCapacity <= fastCapacity)
        {
            newCapacity = fastCapacity;
        }

        AdjustCapacity(newCapacity);
    }

    /// <summary>
    /// 重新调整容量大小
    /// </summary>
    /// <param name="newCapacity"></param>
    private void AdjustCapacity(int newCapacity)
    {
        var oldCapacity = Capacity;
        var oldArray = Data;
        if (newCapacity > oldCapacity)
        {
            var newArray = new byte[newCapacity];
            Array.Copy(oldArray, newArray, oldCapacity);
            Data = newArray;
            Capacity = newCapacity;
        }
    }

    /// <summary>
    /// 计算新大小
    /// </summary>
    /// <param name="minNewCapacity"></param>
    /// <returns></returns>
    private int calculateNewCapacity(int minNewCapacity)
    {
        if (minNewCapacity > MaxCapacity)
        {
            throw new ArgumentException($"minNewCapacity:{minNewCapacity}, excepted not greater than maxCapacity:{MaxCapacity}");
        }

        var newCapacity = 64;
        while (newCapacity < minNewCapacity)
        {
            newCapacity <<= 1;
        }

        return Math.Min(newCapacity, MaxCapacity);
    }

    /// <summary>
    /// 未读数据都移动到最前面
    /// </summary>
    public void Compact()
    {
        var readableBytes = ReadableBytes;
        if (readableBytes <= 0)
        {
            WriterIndex = 0;
            ReaderIndex = 0;
            return;
        }
        if (ReaderIndex > 0)
        {
            Array.Copy(Data, ReaderIndex, Data, 0, readableBytes);
            ReaderIndex = 0;
            WriterIndex = readableBytes;
        }
    }

    /// <summary>
    /// 转换为Span
    /// </summary>
    /// <returns></returns>
    public Span<byte> AsSpan()
    {
        return Data.AsSpan(ReaderIndex, ReadableBytes);
    }

    /// <summary>
    /// 包裹一个buf
    /// </summary>
    /// <param name="buf"></param>
    /// <exception cref="Exception"></exception>
    public void WrapByteBuf(ByteBuf buf)
    {
        _wrappedByteBufList ??= new List<ByteBuf>(1);
        _wrappedByteBufList.Add(buf);
    }

    /// <summary>
    /// 获取包裹的ByteBufList
    /// </summary>
    public List<ByteBuf>? WrappedByteBufList => _wrappedByteBufList;
}
