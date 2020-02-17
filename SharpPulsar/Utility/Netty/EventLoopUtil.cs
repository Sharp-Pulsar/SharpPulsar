using System;
using DotNetty.Transport.Channels;
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
namespace SharpPulsar.Utility.Netty
{
	public class EventLoopUtil
	{

		/// <returns> an EventLoopGroup suitable for the current platform </returns>
		public static MultithreadEventLoopGroup NewEventLoopGroup(int nThreads)
		{
			return new MultithreadEventLoopGroup(nThreads);
		}

		/// <summary>
		/// Return a SocketChannel class suitable for the given EventLoopGroup implementation.
		/// </summary>
		/// <param name="eventLoopGroup">
		/// @return </param>
		public static Type GetClientSocketChannelClass(MultithreadEventLoopGroup eventLoopGroup)
		{
			return typeof(NioSocketChannel);
		}

		public static Type GetServerSocketChannelClass(MultithreadEventLoopGroup eventLoopGroup)
		{
			/*if (eventLoopGroup is EpollEventLoopGroup)
			{
				return typeof(EpollServerSocketChannel);
			}
			else
			{
				return typeof(NioServerSocketChannel);
			}*/
			return typeof(NioServerSocketChannel);
		}

		public static Type GetDatagramChannelClass(IEventLoopGroup eventLoopGroup)
		{
			/*if (eventLoopGroup is EpollEventLoopGroup)
			{
				return typeof(EpollDatagramChannel);
			}
			else
			{
				return typeof(NioDatagramChannel);
			}*/
			return typeof(NioDatagramChannel);
		}

		public static void EnableTriggeredMode(ServerBootstrap bootstrap)
		{
			/*if (Epoll.Available)
			{
				bootstrap.ChildOption(ChannelOption.mo..EPOLL_MODE, EpollMode.LEVEL_TRIGGERED);
			}*/
		}
	}

}