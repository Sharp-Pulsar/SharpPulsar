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
namespace SharpPulsar.Protocol
{
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ScheduledFuture = io.netty.util.concurrent.ScheduledFuture;
	using CommandPing = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandPing;
	using CommandPong = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandPong;
	using ProtocolVersion = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.ProtocolVersion;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;
    using SharpPulsar.Protocol;

    /// <summary>
    /// Implementation of the channel handler to process inbound Pulsar data.
    /// </summary>
    public abstract class PulsarHandler : PulsarDecoder
	{
		protected internal ChannelHandlerContext Ctx;
		protected internal SocketAddress RemoteAddress;
		//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal int RemoteEndpointProtocolVersionConflict = ProtocolVersion.v0.Number;
		private readonly long keepAliveIntervalSeconds;
		private bool waitingForPingResponse = false;
		//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		//ORIGINAL LINE: private io.netty.util.concurrent.ScheduledFuture<?> keepAliveTask;
		private ScheduledFuture<object> keepAliveTask;

		public virtual int RemoteEndpointProtocolVersion
		{
			get
			{
				return RemoteEndpointProtocolVersionConflict;
			}
		}

		public PulsarHandler(int KeepAliveInterval, TimeUnit Unit)
		{
			this.keepAliveIntervalSeconds = Unit.toSeconds(KeepAliveInterval);
		}

		public override void MessageReceived()
		{
			waitingForPingResponse = false;
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void channelActive(io.netty.channel.ChannelHandlerContext ctx) throws Exception
		public override void ChannelActive(ChannelHandlerContext Ctx)
		{
			this.RemoteAddress = Ctx.channel().remoteAddress();
			this.Ctx = Ctx;

			if (log.DebugEnabled)
			{
				log.debug("[{}] Scheduling keep-alive task every {} s", Ctx.channel(), keepAliveIntervalSeconds);
			}
			if (keepAliveIntervalSeconds > 0)
			{
				this.keepAliveTask = Ctx.executor().scheduleAtFixedRate(this.handleKeepAliveTimeout, keepAliveIntervalSeconds, keepAliveIntervalSeconds, TimeUnit.SECONDS);
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: @Override public void channelInactive(io.netty.channel.ChannelHandlerContext ctx) throws Exception
		public override void ChannelInactive(ChannelHandlerContext Ctx)
		{
			CancelKeepAliveTask();
		}

		public override void HandlePing(CommandPing Ping)
		{
			// Immediately reply success to ping requests
			if (log.DebugEnabled)
			{
				log.debug("[{}] Replying back to ping message", Ctx.channel());
			}
			Ctx.writeAndFlush(Commands.NewPong());
		}

		public override void HandlePong(CommandPong Pong)
		{
		}

		private void HandleKeepAliveTimeout()
		{
			if (!Ctx.channel().Open)
			{
				return;
			}

			if (!HandshakeCompleted)
			{
				log.warn("[{}] Pulsar Handshake was not completed within timeout, closing connection", Ctx.channel());
				Ctx.close();
			}
			else if (waitingForPingResponse && Ctx.channel().config().AutoRead)
			{
				// We were waiting for a response and another keep-alive just completed.
				// If auto-read was disabled, it means we stopped reading from the connection, so we might receive the Ping
				// response later and thus not enforce the strict timeout here.
				log.warn("[{}] Forcing connection to close after keep-alive timeout", Ctx.channel());
				Ctx.close();
			}
			else if (RemoteEndpointProtocolVersionConflict >= ProtocolVersion.v1.Number)
			{
				// Send keep alive probe to peer only if it supports the ping/pong commands, added in v1
				if (log.DebugEnabled)
				{
					log.debug("[{}] Sending ping message", Ctx.channel());
				}
				waitingForPingResponse = true;
				Ctx.writeAndFlush(Commands.NewPing());
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] Peer doesn't support keep-alive", Ctx.channel());
				}
			}
		}

		public virtual void CancelKeepAliveTask()
		{
			if (keepAliveTask != null)
			{
				keepAliveTask.cancel(false);
				keepAliveTask = null;
			}
		}

		/// <returns> true if the connection is ready to use, meaning the Pulsar handshake was already completed </returns>
		public abstract bool HandshakeCompleted { get; }

		private static readonly Logger log = LoggerFactory.getLogger(typeof(PulsarHandler));
	}

}