using BAMCIS.Util.Concurrent;
using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Util;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static SharpPulsar.Common.Proto.Api.PulsarApi;
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

	/// <summary>
	/// Implementation of the channel handler to process inbound Pulsar data.
	/// </summary>
	public abstract class PulsarHandler
	{
		private IChannelHandlerContext _ctx;
		private EndPoint _remoteAddress;
		private int _remoteEndpointProtocolVersion = (int)ProtocolVersion.V0;
		private readonly long keepAliveIntervalSeconds;
		private bool waitingForPingResponse = false;
		private Timer _keepAliveTask;

		public virtual int RemoteEndpointProtocolVersion
		{
			get
			{
				return _remoteEndpointProtocolVersion;
			}
			set
			{
				_remoteEndpointProtocolVersion = value;
			}
		}

		public PulsarHandler(int KeepAliveInterval, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			this.keepAliveIntervalSeconds = Unit.ToSecs(KeepAliveInterval);
		}

		public void MessageReceived()
		{
			waitingForPingResponse = false;
		}

		public void ChannelActive(EndPoint ctx)
		{
			_remoteAddress = ctx;

			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("[{}] Scheduling keep-alive task every {} s", ctx, keepAliveIntervalSeconds);
			}
			if (keepAliveIntervalSeconds > 0)
			{
				_keepAliveTask = new Timer(this.HandleKeepAliveTimeout, keepAliveIntervalSeconds, keepAliveIntervalSeconds, TimeUnit.SECONDS);
			}
		}

		public void ChannelInactive(IChannelHandlerContext Ctx)
		{
			CancelKeepAliveTask();
		}

		public void HandlePing(CommandPing Ping)
		{
			// Immediately reply success to ping requests
			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("[{}] Replying back to ping message", _ctx.Channel);
			}
			_ctx.WriteAndFlushAsync(Commands.NewPong());
		}

		public void HandlePong(CommandPong Pong)
		{
		}

		private void HandleKeepAliveTimeout()
		{
			if (!_ctx.)
			{
				return;
			}

			if (!HandshakeCompleted)
			{
				log.LogWarning("[{}] Pulsar Handshake was not completed within timeout, closing connection", _ctx.Channel);
				_ctx.CloseAsync();
			}
			else if (waitingForPingResponse && _ctx.Channel.Configuration.AutoRead)
			{
				// We were waiting for a response and another keep-alive just completed.
				// If auto-read was disabled, it means we stopped reading from the connection, so we might receive the Ping
				// response later and thus not enforce the strict timeout here.
				log.LogWarning("[{}] Forcing connection to close after keep-alive timeout", _ctx.Channel);
				_ctx.CloseAsync();
			}
			else if (_remoteEndpointProtocolVersion >= ProtocolVersion.v1.Number)
			{
				// Send keep alive probe to peer only if it supports the ping/pong commands, added in v1
				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("[{}] Sending ping message", _ctx.Channel);
				}
				waitingForPingResponse = true;
				_ctx.WriteAndFlushAsync(Commands.NewPing());
			}
			else
			{
				if (log.IsEnabled(LogLevel.Debug))
				{
					log.LogDebug("[{}] Peer doesn't support keep-alive", _ctx.Channel);
				}
			}
		}

		public virtual void CancelKeepAliveTask()
		{
			if (_keepAliveTask != null)
			{
				_keepAliveTask.Cancel();
				_keepAliveTask = null;
			}
		}

		/// <returns> true if the connection is ready to use, meaning the Pulsar handshake was already completed </returns>
		public abstract bool HandshakeCompleted {get;}

		private static readonly ILogger log = new LoggerFactory().CreateLogger<PulsarHandler>();
	}

}