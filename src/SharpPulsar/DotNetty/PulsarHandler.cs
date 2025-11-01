
/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Pulsar.Proto;
using SharpPulsar.DotNetty;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.TimeUnit;
namespace SharpPulsar.DotNetty
{

    /// <summary>
    /// Implementation of the channel handler to process inbound Pulsar data.
    /// <para>
    /// Please see <seealso cref="org.apache.pulsar.common.protocol.PulsarDecoder"/> javadoc for important details about handle* method
    /// parameter instance lifecycle.
    /// </para>
    /// </summary>
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public abstract class PulsarHandler : PulsarDecoder
    {
        protected internal IChannelHandlerContext ctx;
        protected internal EndPoint remoteAddress;
        private int remoteEndpointProtocolVersion = (int)ProtocolVersion.V0;
        private readonly long keepAliveIntervalSeconds;
        private bool waitingForPingResponse = false;
        private ScheduledFuture<object> keepAliveTask;

        public virtual int RemoteEndpointProtocolVersion
        {
            get
            {
                return remoteEndpointProtocolVersion;
            }
            set
            {
                this.remoteEndpointProtocolVersion = value;
            }
        }


        public PulsarHandler(int keepAliveInterval, TimeUnit.TimeUnit unit)
        {
            this.keepAliveIntervalSeconds = unit.ToSeconds(keepAliveInterval);
        }

        protected internal override void MessageReceived()
        {
            waitingForPingResponse = false;
        }

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            this.remoteAddress = ctx.Channel.RemoteAddress;
            this.ctx = ctx;

            if (log.isDebugEnabled())
            {
                log.debug("[{}] Scheduling keep-alive task every {} s", this.ToString(), keepAliveIntervalSeconds);
            }
            if (keepAliveIntervalSeconds > 0)
            {
                this.keepAliveTask = ctx.executor().scheduleAtFixedRate(catchingAndLoggingThrowables(this.handleKeepAliveTimeout), keepAliveIntervalSeconds, keepAliveIntervalSeconds, TimeUnit.TimeUnit.SECONDS);
            }
        }

        
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            cancelKeepAliveTask();
        }

        protected internal override void HandlePing(CommandPing ping)
        {
            // Immediately reply success to ping requests
            if (log.isDebugEnabled())
            {
                log.debug("[{}] Replying back to ping message", this.ToString());
            }
            ctx.WriteAndFlush(Commands.NewPong()).addListener(future =>
            {
                if (!future.isSuccess())
                {
                    log.warn("[{}] Forcing connection to close since cannot send a pong message.", ToString(), future.cause());
                    ctx.close();
                }
            });
        }

        protected internal override void HandlePong(CommandPong pong)
        {
        }

        private void HandleKeepAliveTimeout()
        {
            if (!ctx.Channel.Open)
            {
                return;
            }

            if (!HandshakeCompleted)
            {
                log.warn("[{}] Pulsar Handshake was not completed within timeout, closing connection", this.ToString());
                ctx.CloseAsync();
            }
            else if (waitingForPingResponse && ctx.Channel.Configuration.AutoRead)
            {
                // We were waiting for a response and another keep-alive just completed.
                // If auto-read was disabled, it means we stopped reading from the connection, so we might receive the Ping
                // response later and thus not enforce the strict timeout here.
                log.warn("[{}] Forcing connection to close after keep-alive timeout", this.ToString());
                ctx.CloseAsync();
            }
            else if (RemoteEndpointProtocolVersion >= (int)ProtocolVersion.V1)
            {
                // Send keep alive probe to peer only if it supports the ping/pong commands, added in v1
                if (log.isDebugEnabled())
                {
                    log.debug("[{}] Sending ping message", this.ToString());
                }
                waitingForPingResponse = true;
                SendPing();
            }
            else
            {
                if (log.isDebugEnabled())
                {
                    log.debug("[{}] Peer doesn't support keep-alive", this.ToString());
                }
            }
        }

        protected internal virtual ChannelFuture sendPing()
        {
            return ctx.WriteAndFlush(Commands.NewPing()).addListener(future =>
            {
                if (!future.isSuccess())
                {
                    log.warn("[{}] Forcing connection to close since cannot send a ping message.", this.ToString(), future.cause());
                    ctx.close();
                }
            });
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
        protected internal abstract bool HandshakeCompleted { get; }

        /// <summary>
        /// Demo: [id: 0x2561bcd1, L:/10.0.136.103:6650 ! R:/240.240.0.5:58038].
        /// L: local Address.
        /// R: remote address.
        /// </summary>
        public override string ToString()
        {
            IChannelHandlerContext ctx = this.ctx;
            if (ctx == null)
            {
                return "[ctx: null]";
            }
            else
            {
                return ctx.Channel.ToString();
            }
        }

        private static readonly Logger log = LoggerFactory.getLogger(typeof(PulsarHandler));

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
