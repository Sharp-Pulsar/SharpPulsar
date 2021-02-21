//-----------------------------------------------------------------------
// <copyright file="TcpIncomingConnection.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using Akka.Actor;
using System;
using System.Linq;
using Akka.IO.Buffers;
using Akka.Util;
using Akka.IO;
using System.Net.Sockets;

namespace SharpPulsar.PulsarSocket
{
    /// <summary>
    /// An actor handling the connection state machine for an incoming, already connected SocketChannel.
    /// </summary>
    internal sealed class PulsarTcpIncomingConnection : PulsarTcpConnection
    {
        private readonly IActorRef _bindHandler;
        private readonly IEnumerable<Inet.SocketOption> _options;

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="tcp">TBD</param>
        /// <param name="socket">TBD</param>
        /// <param name="bindHandler">TBD</param>
        /// <param name="options">TBD</param>
        /// <param name="readThrottling">TBD</param>
        public PulsarTcpIncomingConnection(TcpExt tcp,
                                     Socket socket,
                                     IActorRef bindHandler,
                                     IEnumerable<Inet.SocketOption> options,
                                     bool readThrottling)
            : base(tcp, socket, readThrottling, Option<int>.None)
        {
            _bindHandler = bindHandler;
            _options = options;
            var poolOption = _options.OfType<ByteBufferPoolSize>()
                .FirstOrDefault();
            BufferPool = poolOption != null
                ? new PulsarDisabledBufferPool(poolOption.ByteBufferPoolSizeBytes)
                : Tcp.BufferPool;
            Context.Watch(bindHandler); // sign death pact
        }

        protected override void PreStart()
        {
            AcquireSocketAsyncEventArgs();

            CompleteConnect(_bindHandler, _options);
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="message">TBD</param>
        /// <returns>TBD</returns>
        protected override bool Receive(object message)
        {
            throw new NotSupportedException();
        }

        protected override IBufferPool BufferPool { get; }
    }
}
