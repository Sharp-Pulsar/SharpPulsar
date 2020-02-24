using DotNetty.Buffers;
using DotNetty.Common;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Net;
using System.Threading.Tasks;
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
		internal static ThreadLocalPool<ByteBufPair> Recycler = new ThreadLocalPool<ByteBufPair>(handle => new ByteBufPair(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private ByteBufPair(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
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
			ByteBufPair buf = Recycler.Take();
			buf.RefCnt = 1;
			buf.First = b1;
			buf.Second = b2;
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
			_handle.Release(this);
		}

		public override IReferenceCounted Touch(object hint)
		{
			First.Touch(hint);
			Second.Touch(hint);
			return this;
		}

		public static readonly Encoder ENCODER = new Encoder();
		public static readonly CopyingEncoder COPYINGENCODER = new CopyingEncoder();
		public class Encoder : IChannelHandler
		{
			public Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
                 return Task.CompletedTask;
			}

			public void ChannelActive(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void ChannelInactive(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void ChannelRead(IChannelHandlerContext context, object message)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void ChannelReadComplete(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void ChannelRegistered(IChannelHandlerContext context)
			{
                
			}

			public void ChannelUnregistered(IChannelHandlerContext context)
			{
                Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void ChannelWritabilityChanged(IChannelHandlerContext context)
			{
                Console.WriteLine(context.Channel.RemoteAddress);
			}

			public Task CloseAsync(IChannelHandlerContext context)
			{
                Console.WriteLine(context.Channel.RemoteAddress);
                return Task.CompletedTask;
			}

			public Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
			{
                Console.WriteLine(context.Channel.RemoteAddress);
                return Task.CompletedTask;
			}

			public Task DeregisterAsync(IChannelHandlerContext context)
			{
                Console.WriteLine(context.Channel.RemoteAddress);
                return Task.CompletedTask;
			}

			public Task DisconnectAsync(IChannelHandlerContext context)
			{
                Console.WriteLine(context.Channel.RemoteAddress);
                return Task.CompletedTask;
			}

			public void ExceptionCaught(IChannelHandlerContext context, Exception exception)
			{
                Console.WriteLine(exception.Message);
			}

			public void Flush(IChannelHandlerContext context)
			{
                Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void HandlerAdded(IChannelHandlerContext context)
			{
                Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void HandlerRemoved(IChannelHandlerContext context)
			{
                Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void Read(IChannelHandlerContext context)
			{
                Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void UserEventTriggered(IChannelHandlerContext context, object evt)
			{
                Console.WriteLine(context.Channel.RemoteAddress);
			}

			
			public async Task WriteAsync(IChannelHandlerContext context, object msg)
			{
				if (msg is ByteBufPair b)
				{
                    // Write each buffer individually on the socket. The retain() here is needed to preserve the fact that
					// ByteBuf are automatically released after a write. If the ByteBufPair ref count is increased and it
					// gets written multiple times, the individual buffers refcount should be reflected as well.
					try
					{
						await context.WriteAsync(b.First.RetainedDuplicate());
						await context.WriteAsync(b.Second.RetainedDuplicate());
					}
					finally
					{
						ReferenceCountUtil.SafeRelease(b);
					}
				}
				else
				{
					await context.WriteAsync(msg);
				}
				
			}
		}

		public class CopyingEncoder : IChannelHandler
		{
			public Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
                 return Task.CompletedTask;
			}

			public void ChannelActive(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void ChannelInactive(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void ChannelRead(IChannelHandlerContext context, object message)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void ChannelReadComplete(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void ChannelRegistered(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void ChannelUnregistered(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void ChannelWritabilityChanged(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public Task CloseAsync(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
                 return Task.CompletedTask;
			}

			public Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
                 return Task.CompletedTask;
			}

			public Task DeregisterAsync(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
                 return Task.CompletedTask;
			}

			public Task DisconnectAsync(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
                 return Task.CompletedTask;
			}

			public void ExceptionCaught(IChannelHandlerContext context, Exception exception)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void Flush(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void HandlerAdded(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void HandlerRemoved(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void Read(IChannelHandlerContext context)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}

			public void UserEventTriggered(IChannelHandlerContext context, object evt)
			{
				 Console.WriteLine(context.Channel.RemoteAddress);
			}


			public async Task WriteAsync(IChannelHandlerContext context, object msg)
			{
				if (msg is ByteBufPair b)
				{
                    // Some handlers in the pipeline will modify the bytebufs passed in to them (i.e. SslHandler).
					// For these handlers, we need to pass a copy of the buffers as the source buffers may be cached
					// for multiple requests.
					try
					{
						await context.WriteAsync(b.First.Copy());
						await context.WriteAsync(b.Second.Copy());
					}
					finally
					{
						ReferenceCountUtil.SafeRelease(b);
					}
				}
				else
				{
					await context.WriteAsync(msg);
				}
			}
		}

	}

}