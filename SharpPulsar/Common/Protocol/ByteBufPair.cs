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
namespace org.apache.pulsar.common.protocol
{
	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;

	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;
	using Sharable = io.netty.channel.ChannelHandler.Sharable;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelOutboundHandlerAdapter = io.netty.channel.ChannelOutboundHandlerAdapter;
	using ChannelPromise = io.netty.channel.ChannelPromise;
	using AbstractReferenceCounted = io.netty.util.AbstractReferenceCounted;
	using Recycler = io.netty.util.Recycler;
	using Handle = io.netty.util.Recycler.Handle;
	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;
	using ReferenceCounted = io.netty.util.ReferenceCounted;

	/// <summary>
	/// ByteBuf holder that contains 2 buffers.
	/// </summary>
	public sealed class ByteBufPair : AbstractReferenceCounted
	{

		private ByteBuf b1;
		private ByteBuf b2;
		private readonly Recycler.Handle<ByteBufPair> recyclerHandle;

		private static readonly Recycler<ByteBufPair> RECYCLER = new RecyclerAnonymousInnerClass();

		private class RecyclerAnonymousInnerClass : Recycler<ByteBufPair>
		{
			protected internal override ByteBufPair newObject(Recycler.Handle<ByteBufPair> handle)
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
		public static ByteBufPair get(ByteBuf b1, ByteBuf b2)
		{
			ByteBufPair buf = RECYCLER.get();
			buf.RefCnt = 1;
			buf.b1 = b1;
			buf.b2 = b2;
			return buf;
		}

		public ByteBuf First
		{
			get
			{
				return b1;
			}
		}

		public ByteBuf Second
		{
			get
			{
				return b2;
			}
		}

		public int readableBytes()
		{
			return b1.readableBytes() + b2.readableBytes();
		}

		/// <returns> a single buffer with the content of both individual buffers </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public static io.netty.buffer.ByteBuf coalesce(ByteBufPair pair)
		public static ByteBuf coalesce(ByteBufPair pair)
		{
			ByteBuf b = Unpooled.buffer(pair.readableBytes());
			b.writeBytes(pair.b1, pair.b1.readerIndex(), pair.b1.readableBytes());
			b.writeBytes(pair.b2, pair.b2.readerIndex(), pair.b2.readableBytes());
			return b;
		}

		protected internal override void deallocate()
		{
			b1.release();
			b2.release();
			b1 = b2 = null;
			recyclerHandle.recycle(this);
		}

		public override ReferenceCounted touch(object hint)
		{
			b1.touch(hint);
			b2.touch(hint);
			return this;
		}

		public static readonly Encoder ENCODER = new Encoder();
		public static readonly CopyingEncoder COPYING_ENCODER = new CopyingEncoder();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Sharable @SuppressWarnings("checkstyle:JavadocType") public static class Encoder extends io.netty.channel.ChannelOutboundHandlerAdapter
		public class Encoder : ChannelOutboundHandlerAdapter
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void write(io.netty.channel.ChannelHandlerContext ctx, Object msg, io.netty.channel.ChannelPromise promise) throws Exception
			public override void write(ChannelHandlerContext ctx, object msg, ChannelPromise promise)
			{
				if (msg is ByteBufPair)
				{
					ByteBufPair b = (ByteBufPair) msg;

					// Write each buffer individually on the socket. The retain() here is needed to preserve the fact that
					// ByteBuf are automatically released after a write. If the ByteBufPair ref count is increased and it
					// gets written multiple times, the individual buffers refcount should be reflected as well.
					try
					{
						ctx.write(b.First.retainedDuplicate(), ctx.voidPromise());
						ctx.write(b.Second.retainedDuplicate(), promise);
					}
					finally
					{
						ReferenceCountUtil.safeRelease(b);
					}
				}
				else
				{
					ctx.write(msg, promise);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Sharable @SuppressWarnings("checkstyle:JavadocType") public static class CopyingEncoder extends io.netty.channel.ChannelOutboundHandlerAdapter
		public class CopyingEncoder : ChannelOutboundHandlerAdapter
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void write(io.netty.channel.ChannelHandlerContext ctx, Object msg, io.netty.channel.ChannelPromise promise) throws Exception
			public override void write(ChannelHandlerContext ctx, object msg, ChannelPromise promise)
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
						ReferenceCountUtil.safeRelease(b);
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