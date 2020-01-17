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
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
    using BAMCIS.Util.Concurrent;
    using SharpPulsar.Common.Protocol;
    using System.Net;
    using SharpPulsar.Common.PulsarApi;
    using System;

    /// <summary>
    /// Implementation of the channel handler to process inbound Pulsar data.
    /// </summary>
    public abstract class PulsarHandler : PulsarDecoder
	{
		protected internal ChannelHandlerContext ctx;
		protected internal SocketAddress remoteAddress;
		protected internal int remoteEndpointProtocolVersion = Convert.ToInt32(ProtocolVersion.V0);
		private readonly long keepAliveIntervalSeconds;
		private bool waitingForPingResponse = false;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private io.netty.util.concurrent.ScheduledFuture<?> keepAliveTask;
		private ScheduledFuture<object> keepAliveTask;

		public virtual int RemoteEndpointProtocolVersion
		{
			get
			{
				return remoteEndpointProtocolVersion;
			}
		}

		public PulsarHandler(int keepAliveInterval, TimeUnit unit)
		{
			this.keepAliveIntervalSeconds = unit.ToSeconds(keepAliveInterval);
		}

		protected internal override sealed void messageReceived()
		{
			waitingForPingResponse = false;
		}

		public override void channelActive(ChannelHandlerContext ctx)
		{
			this.remoteAddress = ctx.channel().remoteAddress();
			this.ctx = ctx;

			if (log.DebugEnabled)
			{
				log.debug("[{}] Scheduling keep-alive task every {} s", ctx.channel(), keepAliveIntervalSeconds);
			}
			if (keepAliveIntervalSeconds > 0)
			{
				this.keepAliveTask = ctx.executor().scheduleAtFixedRate(this.handleKeepAliveTimeout, keepAliveIntervalSeconds, keepAliveIntervalSeconds, TimeUnit.SECONDS);
			}
		}

		public override void channelInactive(ChannelHandlerContext ctx)
		{
			cancelKeepAliveTask();
		}

		protected internal override sealed void handlePing(CommandPing ping)
		{
			// Immediately reply success to ping requests
			if (log.DebugEnabled)
			{
				log.debug("[{}] Replying back to ping message", ctx.channel());
			}
			ctx.writeAndFlush(Commands.newPong());
		}

		protected internal override sealed void handlePong(CommandPong pong)
		{
		}

		private void handleKeepAliveTimeout()
		{
			if (!ctx.channel().Open)
			{
				return;
			}

			if (!HandshakeCompleted)
			{
				log.warn("[{}] Pulsar Handshake was not completed within timeout, closing connection", ctx.channel());
				ctx.close();
			}
			else if (waitingForPingResponse && ctx.channel().config().AutoRead)
			{
				// We were waiting for a response and another keep-alive just completed.
				// If auto-read was disabled, it means we stopped reading from the connection, so we might receive the Ping
				// response later and thus not enforce the strict timeout here.
				log.warn("[{}] Forcing connection to close after keep-alive timeout", ctx.channel());
				ctx.close();
			}
			else if (remoteEndpointProtocolVersion >= ProtocolVersion.v1.Number)
			{
				// Send keep alive probe to peer only if it supports the ping/pong commands, added in v1
				if (log.DebugEnabled)
				{
					log.debug("[{}] Sending ping message", ctx.channel());
				}
				waitingForPingResponse = true;
				ctx.writeAndFlush(Commands.newPing());
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] Peer doesn't support keep-alive", ctx.channel());
				}
			}
		}

		protected internal virtual void cancelKeepAliveTask()
		{
			if (keepAliveTask != null)
			{
				keepAliveTask.cancel(false);
				keepAliveTask = null;
			}
		}

		/// <returns> true if the connection is ready to use, meaning the Pulsar handshake was already completed </returns>
		protected internal abstract bool HandshakeCompleted {get;}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(PulsarHandler));
	}

}