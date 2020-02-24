using DotNetty.Buffers;
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

		public ConnectionPool(ClientConfigurationData conf, MultithreadEventLoopGroup eventLoopGroup) : this(conf, eventLoopGroup, () => new ClientCnx(conf, eventLoopGroup))
		{
		}

		public ConnectionPool(ClientConfigurationData conf, IEventLoopGroup eventLoopGroup, Func<ClientCnx> clientCnxSupplier)
		{
			this._eventLoopGroup = eventLoopGroup;
			this._maxConnectionsPerHosts = conf.ConnectionsPerBroker;
			_conf = conf;
			_pool = new ConcurrentDictionary<string, ConcurrentDictionary<int, ClientCnx>>();
			_bootstrap = new Bootstrap();
			_bootstrap.Group(eventLoopGroup);
			_bootstrap.Channel<TcpSocketChannel>();

			_bootstrap.Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(conf.ConnectionTimeoutMs));
			_bootstrap.Option(ChannelOption.TcpNodelay, conf.UseTcpNoDelay);
			_bootstrap.Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default);

			try
			{
				_bootstrap.Handler(new PulsarChannelInitializer(conf, clientCnxSupplier));
			}
			catch (System.Exception e)
			{
				Log.LogError("Failed to create channel initializer");
				throw new PulsarClientException(e.Message);
			}

			_dnsResolver = new DefaultNameResolver();
			//this.DnsResolver = (new DnsNameResolverBuilder(eEventLoopGroup. .next())).traceEnabled(true).channelType(EventLoopUtil.getDatagramChannelClass(EventLoopGroup)).build();
		}

		private static readonly Random Random = new Random();

		public ValueTask<ClientCnx> GetConnection(in IPEndPoint address)
		{
			return GetConnection(address, address);
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
		public async ValueTask<ClientCnx> GetConnection(IPEndPoint logicalAddress, IPEndPoint physicalAddress)
		{
			var host = Dns.GetHostEntry(logicalAddress.Address.ToString()).HostName;
            ClientCnx clientCnx = null;
			if (_maxConnectionsPerHosts == 0)
			{
                // Disable pooling
                await CreateConnection(logicalAddress, physicalAddress, -1).AsTask().ContinueWith(x =>
                {
                    try
                    {
                        clientCnx = x.Result;
                    }
                    catch (Exception e)
                    {
						Log.LogDebug(e,e.Message);
					}
                });
                return clientCnx;
            }
            else
            {
                var randomKey = SignSafeMod(Random.Next(), _maxConnectionsPerHosts);
				clientCnx = await CreateConnection(logicalAddress, physicalAddress, randomKey);
                var client = _pool.GetOrAdd(host, x => new ConcurrentDictionary<int, ClientCnx>()).GetOrAdd(randomKey, k => clientCnx);
                return client;

			}

        }

		private ValueTask<ClientCnx> CreateConnection(IPEndPoint logicalAddress, IPEndPoint physicalAddress, int connectionKey)
		{
			if(Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("Connection for {} not found in cache", logicalAddress);
			}
            TaskCompletionSource<ClientCnx> cnxTask = new TaskCompletionSource<ClientCnx>();
			// Trigger async connect to broker
			try
            {
                CreateConnection(physicalAddress).AsTask().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Log.LogWarning("Connection handshake failed: {}", task.Exception?.Message);
                        cnxTask.SetException(task.Exception ?? throw new InvalidOperationException());
                    }
                    else
                    {
						var channel = task.Result;
						channel.CloseCompletion.ContinueWith(c =>
						{
							if (Log.IsEnabled(LogLevel.Debug))
							{
								Log.LogDebug("Removing closed connection from pool: {}", c);
							}
							CleanupConnection(logicalAddress.ToString(), connectionKey);
						});
						var cnx = (ClientCnx)channel.Pipeline.Get("handler");

                        
						if (!channel.Open || cnx == null)
						{
							if (Log.IsEnabled(LogLevel.Debug))
							{
								Log.LogDebug("[{}] Connection was already closed by the time we got notified", channel);
							}
							cnxTask.SetException(new ChannelException("Connection already closed"));
						}
						else
                        {
                            cnx.PulsarChannel = channel;
							Log.LogInformation($"[{channel?.LocalAddress}] Connected to server");
							if (!logicalAddress.Equals(physicalAddress))
							{
								cnx.TargetBroker = logicalAddress;
							}
							cnx.RemoteHostName = Dns.GetHostEntry(physicalAddress.Address.ToString()).HostName;
							try
                            {
                                channel.WriteAndFlushAsync(cnx.NewConnectCommand());
								if (Log.IsEnabled(LogLevel.Debug))
                                {
                                    Log.LogDebug("[{}] Connection handshake completed", cnx.RemoteHostName);
                                }
                                cnxTask.SetResult(cnx);
							}
							catch (Exception e)
							{
								Log.LogWarning("[{}] Connection handshake failed: {}", cnx.Channel(), e.Message);
                                Log.LogWarning(e, e.Message);
								CleanupConnection(cnx.RemoteHostName, connectionKey);
								cnxTask.SetException(e);
							}

						}
					}
				});
				
				
            }
            catch (Exception e)
            {
                Log.LogWarning("Failed to open connection to {} : {}", physicalAddress, e.Message);
                //CleanupConnection(cxn.RemoteAddress.ToString(), connectionKey);
                return new ValueTask<ClientCnx>(Task.FromException<ClientCnx>(new PulsarClientException(e.Message)));
			}
			return new ValueTask<ClientCnx>(cnxTask.Task);
        }

		/// <summary>
		/// Resolve DNS asynchronously and attempt to connect to any IP address returned by DNS server
		/// </summary>
		private async ValueTask<IChannel> CreateConnection(IPEndPoint unresolvedAddress)
		{
			var hostname = unresolvedAddress.Address.ToString();
			var port = unresolvedAddress.Port;
            IChannel channel = null;
			// Resolve DNS --> Attempt to connect to all IP addresses until once succeeds
            try
			{
				var dns = await ResolveName(hostname);
                //Log.LogInformation($"DNS: {dns}");
				var c = await ConnectToResolvedAddresses(dns, port);
                channel = c;
                Log.LogInformation($"Connection created for Channel Id: {channel.Id}");
            }
            catch (Exception e)
            {
                Log.LogError(e, e.Message);
            }
			
			return channel;
		}

		/// <summary>
		/// Try to connect to a sequence of IP addresses until a successfull connection can be made, or fail if no address is
		/// working
		/// </summary>
		private async ValueTask<IChannel> ConnectToResolvedAddresses(IList<IPAddress> unresolvedAddresses, int port)
		{
			foreach (var address in unresolvedAddresses)
			{
				Log.LogInformation($"Resolved address:{address}");
			}
            foreach (var address in unresolvedAddresses)
            {
				try
				{
					Log.LogInformation($"Resolved address:{address}");
                    IChannel channel = null;
                    await ConnectToAddress(address.ToString(), port)
                        .AsTask().ContinueWith(x =>
                        {
                            try
                            {
                                channel = x.Result;
                            }
                            catch (Exception e)
                            {
                                Log.LogError(e, e.Message);
                            }
                        });
                    if(channel.Open)
                        return channel;
				}
                catch (Exception e)
                {
                    Log.LogError(e.ToString());
                }
			}
			
			return Task.FromException<IChannel>(new Exception("Could not connect to server!")).Result;
		}

		public  ValueTask<IList<IPAddress>> ResolveName(string hostname)
		{
			return new ValueTask<IList<IPAddress>>(Task.FromResult<IList<IPAddress>>(Dns.GetHostEntry(hostname).AddressList.ToList()));
			
		}

		/// <summary>
		/// Attempt to establish a TCP connection to an already resolved single IP address
		/// </summary>
		private async ValueTask<IChannel> ConnectToAddress(string server, int port)
        {
            IChannel channel = null;
            await _bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6650)).ContinueWith(x =>
            {
                try
                {
                    channel = x.Result;
                    Log.LogInformation($"channel is registered: {channel.Registered}, with Id: {channel.Id.AsLongText()}");
				}
                catch (Exception e)
                {
                    Log.LogInformation(e, e.Message);
				}
            });
			
			return channel;
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
			var mod = (int)(dividend % (long) divisor);
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