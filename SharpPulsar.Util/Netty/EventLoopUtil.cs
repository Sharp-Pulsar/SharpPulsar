using System;
using ServerBootstrap = DotNetty.Transport.Bootstrapping.ServerBootstrap;
//using Epoll = DotNetty.Transport.Channels.Embedded..epoll.Epoll;
//using EpollChannelOption = io.netty.channel.epoll.EpollChannelOption;
//using EpollDatagramChannel = io.netty.channel.epoll.EpollDatagramChannel;
//using EpollEventLoopGroup = io.netty.channel.epoll.EpollEventLoopGroup;
//using EpollMode = io.netty.channel.epoll.EpollMode;
//using EpollServerSocketChannel = io.netty.channel.epoll.EpollServerSocketChannel;
//using EpollSocketChannel = DotNetty.Transport.Channels.Pool. io.netty.channel.epoll.EpollSocketChannel;
using NioDatagramChannel = DotNetty.Transport.Channels.Sockets.SocketDatagramChannel;
using NioServerSocketChannel = DotNetty.Transport.Channels.Sockets.TcpServerSocketChannel;
using NioSocketChannel = DotNetty.Transport.Channels.Sockets.TcpSocketChannel;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;

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
namespace SharpPulsar.Util.Netty
{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("checkstyle:JavadocType") public class EventLoopUtil
	public class EventLoopUtil
	{

		/// <returns> an EventLoopGroup suitable for the current platform </returns>
		public static EventLoopGroup NewEventLoopGroup(int nThreads, ThreadFactory threadFactory)
		{
			if (Epoll.Available)
			{
				return new EventLoopGroup(nThreads, threadFactory);
			}
			else
			{
				// Fallback to NIO
				return new NioEventLoopGroup(nThreads, threadFactory);
			}
		}

		/// <summary>
		/// Return a SocketChannel class suitable for the given EventLoopGroup implementation.
		/// </summary>
		/// <param name="eventLoopGroup">
		/// @return </param>
		public static Type GetClientSocketChannelClass(EventLoopGroup eventLoopGroup)
		{
			if (eventLoopGroup is EpollEventLoopGroup)
			{
				return typeof(EpollSocketChannel);
			}
			else
			{
				return typeof(NioSocketChannel);
			}
		}

		public static Type getServerSocketChannelClass(EventLoopGroup eventLoopGroup)
		{
			if (eventLoopGroup is EpollEventLoopGroup)
			{
				return typeof(EpollServerSocketChannel);
			}
			else
			{
				return typeof(NioServerSocketChannel);
			}
		}

		public static Type getDatagramChannelClass(EventLoopGroup eventLoopGroup)
		{
			if (eventLoopGroup is EpollEventLoopGroup)
			{
				return typeof(EpollDatagramChannel);
			}
			else
			{
				return typeof(NioDatagramChannel);
			}
		}

		public static void EnableTriggeredMode(ServerBootstrap bootstrap)
		{
			if (Epoll.Available)
			{
				bootstrap.ChildOption(ChannelOption..EPOLL_MODE, EpollMode.LEVEL_TRIGGERED);
			}
		}
	}

}