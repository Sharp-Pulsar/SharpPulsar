﻿using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using SharpPulsar.Impl.Conf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avro;
using DotNetty.Codecs.Protobuf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using PulsarClientException = SharpPulsar.Exceptions.PulsarClientException;

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
namespace SharpPulsar.Impl
{

	public sealed class ConnectionPool : IDisposable
	{
		private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, ClientCnx>> _pool;

		private readonly Bootstrap _bootstrap;
		private readonly IEventLoopGroup _eventLoopGroup;
		private readonly int _maxConnectionsPerHosts;
		private DefaultNameResolver _dnsResolver;
		private ClientConfigurationData _conf;
        private PulsarServiceNameResolver _serviceNameResolver;

		public ConnectionPool(ClientConfigurationData conf, MultithreadEventLoopGroup eventLoopGroup, PulsarServiceNameResolver serviceNameResolver) : this(conf, eventLoopGroup, () => new ClientCnx(conf, eventLoopGroup), serviceNameResolver)
		{
		}

		public ConnectionPool(ClientConfigurationData conf, MultithreadEventLoopGroup eventLoopGroup, Func<ClientCnx> clientCnxSupplier, PulsarServiceNameResolver serviceNameResolver)
        {
            _serviceNameResolver = serviceNameResolver;
			this._eventLoopGroup = eventLoopGroup;
			this._maxConnectionsPerHosts = conf.ConnectionsPerBroker;
			_conf = conf;
			_pool = new ConcurrentDictionary<string, ConcurrentDictionary<int, ClientCnx>>();
			_bootstrap = new Bootstrap()
                .Group(eventLoopGroup)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(conf.ConnectionTimeoutMs))
			    .Option(ChannelOption.TcpNodelay, conf.UseTcpNoDelay)
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Option(ChannelOption.SoKeepalive, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(new ProtobufVarint32FrameDecoder());
                    pipeline.AddLast(new ProtobufDecoder(BaseCommand.Parser));

                    pipeline.AddLast(new ProtobufVarint32LengthFieldPrepender());
                    pipeline.AddLast(new ProtobufEncoder());
                    pipeline.AddLast("handler", clientCnxSupplier.Invoke());
                    //pipeline.AddLast(new ClientCnx(conf, Commands.CurrentProtocolVersion, eventLoopGroup));
                }));

			_dnsResolver = new DefaultNameResolver();
			//this.DnsResolver = (new DnsNameResolverBuilder(eEventLoopGroup. .next())).traceEnabled(true).channelType(EventLoopUtil.getDatagramChannelClass(EventLoopGroup)).build();
		}

		private static readonly Random Random = new Random();

		public ValueTask<ClientCnx> GetConnection(in IPEndPoint address)
		{
			return GetConnection(address, address);
		}

        public IList<IPEndPoint> GetAddresses()
        {
            return _serviceNameResolver.AddressList();
        }

        public Bootstrap GetBootstrap()
        {
            return _bootstrap;
        }

        public DefaultNameResolver GetDefaultNameResolver()
        {
            return _dnsResolver;
        }

        public void GetOrAddConnection(string host, ClientCnx ctx)
        {
            var randomKey = SignSafeMod(Random.Next(), _maxConnectionsPerHosts);
			_pool.GetOrAdd(host, x => new ConcurrentDictionary<int, ClientCnx>()).GetOrAdd(randomKey, k => ctx);
		}
		public async ValueTask CreateConnections()
        {
            var services = _serviceNameResolver.AddressList();
            foreach (var s in services)
            {
                var service = s;
                if (!_dnsResolver.IsResolved(s))
                    service = (IPEndPoint)await _dnsResolver.ResolveAsync(s);
				var host = Dns.GetHostEntry(service.Address).HostName;
				Log.LogInformation($"Creating connection to {host}");
                for (var i = 0; i < _maxConnectionsPerHosts; i++)
                {
                    var randomKey = SignSafeMod(Random.Next(), _maxConnectionsPerHosts);
                    var channel = await _bootstrap.ConnectAsync(service);
                    var cnx = (ClientCnx)channel.Pipeline.Get("handler");
                    _pool.GetOrAdd(host, x => new ConcurrentDictionary<int, ClientCnx>()).GetOrAdd(randomKey, k => cnx);
				}
            }
            
		}
		public void CloseAllConnections()
		{
			_pool.Values.ToList().ForEach(map =>
			{
				map.Values.ToList().ForEach(async client => { await client.CloseAsync(client.Context); });
			});
		}

		/// <summary>
		/// Get a connection from the pool.
		/// <para>
		/// The connection can either be created or be coming from the pool itself.
		/// </para>
		/// <para>
		/// When specifying multiple addresses, the logicalAddress is used as a tag for the broker, while the physicalAddress
		/// is where the connection is actually happening.
		/// </para>
		/// <para>
		/// These two addresses can be different when the client is forced to connect through a proxy layer. Essentially, the
		/// pool is using the logical address as a way to decide whether to reuse a particular connection.
		/// 
		/// </para>
		/// </summary>
		/// <param name="logicalAddress">
		///            the address to use as the broker tag </param>
		/// <param name="physicalAddress">
		///            the real address where the TCP connection should be made </param>
		/// <returns> a future that will produce the ClientCnx object </returns>
		public ValueTask<ClientCnx> GetConnection(IPEndPoint logicalAddress, IPEndPoint physicalAddress)
		{
			var host = Dns.GetHostEntry(logicalAddress.Address.ToString()).HostName;
			var range = new Random();
            var index = range.Next(0, _maxConnectionsPerHosts);
            var co = _pool[host];
            return new ValueTask<ClientCnx>(co[index]);
        }

		public void Close()
		{
			try
			{
				_eventLoopGroup.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)).GetAwaiter();
				CloseAllConnections();
			}
			catch (Exception e)
			{
				Log.LogWarning("EventLoopGroup shutdown was interrupted", e);
			}

		}

		private void CleanupConnection(string server, int connectionKey)
		{
			try
			{
				var map = _pool[server];
				map?.Remove(connectionKey, out var clientCnx);
			}
			catch (Exception e)
			{
				Log.LogError(e, e.Message);
			}
		}

		public static int SignSafeMod(long dividend, int divisor)
		{
			var mod = (int)(dividend % (long)divisor);
			if (mod < 0)
			{
				mod += divisor;
			}
			return mod;
		}

		public void Dispose()
		{
			Close();
		}

		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(ConnectionPool));
	}

}