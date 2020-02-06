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
    using Microsoft.Extensions.Logging;
    using SharpPulsar.Util;
    using System.Net;
    using DotNetty.Transport.Channels;
    using System;
    using DotNetty.Common.Concurrency;
    using SharpPulsar.Protocol.Proto;

    /// <summary>
    /// Implementation of the channel handler to process inbound Pulsar data.
    /// </summary>
    public abstract class PulsarHandler : PulsarDecoder
	{
		//protected internal ChannelHandlerContext Ctx;
		protected internal EndPoint RemoteAddress;
		protected internal int _remoteEndpointProtocolVersion = (int)ProtocolVersion.V0;
		private readonly long keepAliveIntervalSeconds;
		private bool waitingForPingResponse = false;
		private IScheduledTask keepAliveTask;
		private IChannelHandlerContext _context;

		public virtual int RemoteEndpointProtocolVersion
		{
			set 
			{
				_remoteEndpointProtocolVersion = value;
			}
			get
			{
				return _remoteEndpointProtocolVersion;
			}
		}

		public PulsarHandler(int keepAliveInterval, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			this.keepAliveIntervalSeconds = unit.ToSecs(keepAliveInterval);
		}

		public override void MessageReceived()
		{
			waitingForPingResponse = false;
		}

		public void ChannelActive(IChannelHandlerContext ctx)
		{
			RemoteAddress = ctx.Channel.RemoteAddress;
			_context = ctx;

			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("[{}] Scheduling keep-alive task every {} s", ctx.Channel, keepAliveIntervalSeconds);
			}
			if (keepAliveIntervalSeconds > 0)
			{
				keepAliveTask = ctx.Executor.Schedule(() => { HandleKeepAliveTimeout(); }, TimeSpan.FromSeconds(keepAliveIntervalSeconds));
			}
		}

		public void ChannelInactive(IChannelHandlerContext ctx)
		{
			CancelKeepAliveTask();
		}

		public void HandlePing(CommandPing ping)
		{
			// Immediately reply success to ping requests
			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("[{}] Replying back to ping message", _context.Channel);
			}
			_context.WriteAndFlushAsync(Commands.NewPong());
		}

		public override void HandlePong(CommandPong Pong)
		{
		}

		private void HandleKeepAliveTimeout()
		{
			if (!_context.Channel.Open)
			{
				return;
			}

			if (!HandshakeCompleted)
			{
				log.LogWarning("[{}] Pulsar Handshake was not completed within timeout, closing connection", _context.Channel);
				_context.CloseAsync();
			}
			else if (waitingForPingResponse && _context.Channel.Configuration.AutoRead)
			{
				// We were waiting for a response and another keep-alive just completed.
				// If auto-read was disabled, it means we stopped reading from the connection, so we might receive the Ping
				// response later and thus not enforce the strict timeout here.
				log.LogWarning("[{}] Forcing connection to close after keep-alive timeout", _context.Channel);
				_context.CloseAsync();
			}
			else if (_remoteEndpointProtocolVersion >= (int)ProtocolVersion.V1)
			{
				// Send keep alive probe to peer only if it supports the ping/pong commands, added in v1
				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("[{}] Sending ping message", _context.Channel);
				}
				waitingForPingResponse = true;
				_context.WriteAndFlushAsync(Commands.NewPing());
			}
			else
			{
				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("[{}] Peer doesn't support keep-alive", _context.Channel);
				}
			}
		}

		public virtual void CancelKeepAliveTask()
		{
			if (keepAliveTask != null)
			{
				keepAliveTask.Cancel();
				keepAliveTask = null;
			}
		}

		/// <returns> true if the connection is ready to use, meaning the Pulsar handshake was already completed </returns>
		public abstract bool HandshakeCompleted { get; }

		private static readonly ILogger log = new LoggerFactory().CreateLogger(typeof(PulsarHandler));
	}

}