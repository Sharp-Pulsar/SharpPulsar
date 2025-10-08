//https://github.com/eaba/EasyTestSocket.git
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SharpPulsar.Shared.Buf;

public sealed class ByteBufPool
{
    /// <summary>
    /// 对象工厂类
    /// </summary>
    /// <returns></returns>
    private readonly Func<ByteBuf> _func;

    /// <summary>
    /// 存储对象数据结构
    /// </summary>
    private readonly ConcurrentQueue<ByteBuf> _queue;

    /// <summary>
    /// 最大容量
    /// </summary>
    private readonly int _maxSize;

    /// <summary>
    /// 当前数量
    /// </summary>
    private int _count;

    /// <summary>
    /// 回收器
    /// </summary>
    private readonly Action<ByteBuf> _deallocate;

    public ByteBufPool(Func<ByteBuf> func, int minSize, int maxSize)
    {
        this._func = func;
        this._maxSize = maxSize;
        this._queue = new ConcurrentQueue<ByteBuf>();
        this._deallocate = Free;
        for (var i = 0; i < minSize; i++)
        {
            var byteBuf = func();

            byteBuf.Deallocate = _deallocate;
            byteBuf.Release();
        }
    }

    /// <summary>
    /// 分配一个对象
    /// </summary>
    /// <returns></returns>
    public ByteBuf Allocate()
    {
        if (this._queue.TryDequeue(out var byteBuf))
        {
            Interlocked.Decrement(ref _count);
            byteBuf.Retain();

            return byteBuf;
        }

        byteBuf = _func();
        byteBuf.Deallocate = _deallocate;

        return byteBuf;
    }

    /// <summary>
    /// 释放对象
    /// </summary>
    /// <param name="buf"></param>
    public void Free(ByteBuf? buf)
    {
        if (null == buf)
        {
            return;
        }

        this._queue.Enqueue(buf);
        Interlocked.Increment(ref _count);

        while (_count > _maxSize)
        {
            // 释放一些对象
            this._queue.TryDequeue(out _);
            Interlocked.Decrement(ref _count);
        }
    }
}
