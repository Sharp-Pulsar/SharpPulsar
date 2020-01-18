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
namespace SharpPulsar.Common.Protocol
{
	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;

	using ByteBuf = io.netty.buffer.ByteBuf;
	//using Unpooled = io.netty.buffer.Unpooled;
	using Sharable = io.netty.channel.ChannelHandler.Sharable;
	//using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	//using ChannelOutboundHandlerAdapter = io.netty.channel.ChannelOutboundHandlerAdapter;
	//using ChannelPromise = io.netty.channel.ChannelPromise;
	//using AbstractReferenceCounted = io.netty.util.AbstractReferenceCounted;
	//using Recycler = io.netty.util.Recycler;
	using Handle = io.netty.util.Recycler.Handle;
    using DotNetty.Common.Utilities;
    using DotNetty.Buffers;
    using DotNetty.Common;
    using DotNetty.Transport.Channels;
    using DotNetty.Common.Concurrency;

    //using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;
    //using ReferenceCounted = io.netty.util.ReferenceCounted;

    /// <summary>
    /// ByteBuf holder that contains 2 buffers.
    /// </summary>
    public sealed class ByteBufPair : AbstractReferenceCounted
	{

		private ByteBuf b1;
		private ByteBuf b2;
		private readonly ThreadLocalPool<ByteBufPair> recyclerHandle;

		private static readonly ThreadLocalPool<ByteBufPair> RECYCLER = new RecyclerAnonymousInnerClass();

		private class RecyclerAnonymousInnerClass : ThreadLocalPool<ByteBufPair>
		{
			protected internal override ByteBufPair NewObject(ThreadLocalPool<ByteBufPair> handle)
			{
				return new ByteBufPair(handle);
			}
		}

		private ByteBufPair(Recycler.Handle<ByteBufPair> recyclerHandle)
		{
			this.recyclerHandle = recyclerHandle;
		}

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
			ByteBufPair buf = RECYCLER.get();
			buf.RefCnt = 1;
			buf.b1 = b1;
			buf.b2 = b2;
			return buf;
		}

		public IByteBuffer First
		{
			get
			{
				return b1;
			}
		}

		public IByteBuffer Second
		{
			get
			{
				return b2;
			}
		}

		public int ReadableBytes()
		{
			return b1.readableBytes() + b2.readableBytes();
		}

		public static IByteBuffer Coalesce(ByteBufPair pair)
		{
			ByteBuf b = Unpooled.Buffer(pair.ReadableBytes());
			b.writeBytes(pair.b1, pair.b1.readerIndex(), pair.b1.readableBytes());
			b.writeBytes(pair.b2, pair.b2.readerIndex(), pair.b2.readableBytes());
			return b;
		}

		protected internal override void Deallocate()
		{
			b1.release();
			b2.release();
			b1 = b2 = null;
			recyclerHandle.recycle(this);
		}

		public override IReferenceCounted Touch(object hint)
		{
			b1.touch(hint);
			b2.touch(hint);
			return this;
		}

		

		public static readonly Encoder ENCODER = new Encoder();
		public static readonly CopyingEncoder COPYING_ENCODER = new CopyingEncoder();


		public class Encoder : ChannelOutboundHandlerAdapter
		{
			public override void Write(IChannelHandlerContext ctx, object msg, TaskCompletionSource promise)
			{
				if (msg is ByteBufPair)
				{
					ByteBufPair b = (ByteBufPair) msg;

					// Write each buffer individually on the socket. The retain() here is needed to preserve the fact that
					// ByteBuf are automatically released after a write. If the ByteBufPair ref count is increased and it
					// gets written multiple times, the individual buffers refcount should be reflected as well.
					try
					{
						ctx.Write(b.First.retainedDuplicate(), ctx.voidPromise());
						ctx.write(b.Second.retainedDuplicate(), promise);
					}
					finally
					{
						ReferenceCountUtil.SafeRelease(b);
					}
				}
				else
				{
					ctx.write(msg, promise);
				}
			}
		}


		public class CopyingEncoder : ChannelOutboundHandlerAdapter
		{
			public override void Write(IChannelHandlerContext ctx, object msg, ChannelPromise promise)
			{
				if (msg is ByteBufPair)
				{
					ByteBufPair b = (ByteBufPair) msg;

					// Some handlers in the pipeline will modify the bytebufs passed in to them (i.e. SslHandler).
					// For these handlers, we need to pass a copy of the buffers as the source buffers may be cached
					// for multiple requests.
					try
					{
						ctx.write(b.First.copy(), ctx.voidPromise());
						ctx.write(b.Second.copy(), promise);
					}
					finally
					{
						ReferenceCountUtil.SafeRelease(b);
					}
				}
				else
				{
					ctx.write(msg, promise);
				}
			}
		}

	}

}