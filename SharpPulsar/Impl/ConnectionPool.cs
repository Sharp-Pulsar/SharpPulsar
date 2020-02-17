using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using SharpPulsar.Exception;
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
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, Task<ClientCnx>>> _pool;

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
			_pool = new ConcurrentDictionary<string, ConcurrentDictionary<int, Task<ClientCnx>>>();
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
				map.Values.ToList().ForEach(task =>
			{
				if (task.IsCompleted)
				{
					if (task.IsCompletedSuccessfully)
					{
						task.Result.Close();
					}
					else
					{
					}
				}
				else
				{
					task.ContinueWith(x => x.Result.Close()/*ClientCnx.Close()*/);
				}
			});
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
			if (_maxConnectionsPerHosts == 0)
			{
				
				// Disable pooling
				return CreateConnection(logicalAddress, physicalAddress, -1);
			}

			var randomKey = SignSafeMod(Random.Next(), _maxConnectionsPerHosts);
			var client = _pool.GetOrAdd(host, x => new ConcurrentDictionary<int, Task<ClientCnx>>()).GetOrAdd(randomKey, k => CreateConnection(logicalAddress, physicalAddress, randomKey).AsTask());
			return new ValueTask<ClientCnx>(client);
		}

		private ValueTask<ClientCnx> CreateConnection(IPEndPoint logicalAddress, IPEndPoint physicalAddress, int connectionKey)
		{
			if(Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("Connection for {} not found in cache", logicalAddress);
			}
			var cxnTask = new TaskCompletionSource<ClientCnx>();
			// Trigger async connect to broker
			CreateConnection(physicalAddress).AsTask().ContinueWith(connectionTask =>
			{
				if(connectionTask.Status != TaskStatus.Faulted)
				{
					Log.LogInformation("[{}] Connected to server", connectionTask.Result);
					var channel = connectionTask.Result;
					//How do I implement this in C#?
					/*
					 * channel.closeFuture().addListener(v =>
					{
					if (log.DebugEnabled)
					{
						log.debug("Removing closed connection from pool: {}", v);
					}
					CleanupConnection(LogicalAddress, ConnectionKey, CnxFuture);
					});
					 
					*/
					var cnx = (ClientCnx)channel.Pipeline.Get("handler");
					if (!channel.Active || cnx == null)
					{
						if(Log.IsEnabled(LogLevel.Debug))
						{
							Log.LogDebug("[{}] Connection was already closed by the time we got notified", channel);
						}						
						cxnTask.SetException(new ChannelException("Connection already closed"));
						return;
					}
					if (!logicalAddress.Equals(physicalAddress))
					{
						cnx.TargetBroker = logicalAddress;
					}
					cnx.RemoteHostName = Dns.GetHostEntry(physicalAddress.Address.ToString()).HostName;
					cnx.ConnectionTask().Task.ContinueWith(cnnx =>
					{
						if (cnnx.Status != TaskStatus.Faulted)
						{
							if (Log.IsEnabled(LogLevel.Debug))
							{
								Log.LogDebug("[{}] Connection handshake completed", cnnx.Result.Channel());
							}
							cxnTask.SetResult(cnnx.Result);
						}
						else
						{
							Log.LogWarning("[{}] Connection handshake failed: {}", cnnx.Result.Channel(), cnnx.Exception.Message);
							cxnTask.SetException(cnnx.Exception);
							CleanupConnection(cnx.RemoteHostName, connectionKey);
							cnnx.Result.Ctx().CloseAsync();
						}

					});
				}
				else
				{
					Log.LogWarning("Failed to open connection to {} : {}", physicalAddress, connectionTask.Exception.Message);
					CleanupConnection(connectionTask.Result.RemoteAddress.ToString(), connectionKey);
					cxnTask.SetException(new PulsarClientException(connectionTask.Exception.Message));
				}
			});

			return new ValueTask<ClientCnx>(cxnTask.Task);
		}

		/// <summary>
		/// Resolve DNS asynchronously and attempt to connect to any IP address returned by DNS server
		/// </summary>
		private ValueTask<IChannel> CreateConnection(IPEndPoint unresolvedAddress)
		{
			var hostname = unresolvedAddress.Address.ToString();
			var port = unresolvedAddress.Port;

			// Resolve DNS --> Attempt to connect to all IP addresses until once succeeds
			var channel = ResolveName(hostname).AsTask().ContinueWith(task => 
			ConnectToResolvedAddresses(task.Result.GetEnumerator(), port));
			return channel.Result;
		}

		/// <summary>
		/// Try to connect to a sequence of IP addresses until a successfull connection can be made, or fail if no address is
		/// working
		/// </summary>
		private ValueTask<IChannel> ConnectToResolvedAddresses(IEnumerator<IPAddress> unresolvedAddresses, int port)
		{
			var channelTask = new TaskCompletionSource<IChannel>();
			var connected = false;
			while(unresolvedAddresses.MoveNext() && !connected)
			{
				ConnectToAddress(unresolvedAddresses.Current.ToString(), port).AsTask().ContinueWith(task =>
				{
					if (task.Status != TaskStatus.Faulted)
					{
						channelTask.SetResult(task.Result);
						connected = true;
					}
					else if (unresolvedAddresses.MoveNext())
					{
						ConnectToAddress(unresolvedAddresses.Current.ToString(), port).AsTask().ContinueWith(task2 =>
						{
							if (task2.Status != TaskStatus.Faulted)
							{
								channelTask.SetResult(task2.Result);
								connected = true;
							}
							else
							{
								channelTask.SetException(task2.Exception);
							}
						});
					}
					else
					{
						channelTask.SetException(task.Exception);
					}
				});
			}
			return new ValueTask<IChannel>(channelTask.Task.Result);
		}

		public  ValueTask<IList<IPAddress>> ResolveName(string hostname)
		{
			return new ValueTask<IList<IPAddress>>(Task.FromResult<IList<IPAddress>>(Dns.GetHostEntry(hostname).AddressList.ToList()));
			
		}

		/// <summary>
		/// Attempt to establish a TCP connection to an already resolved single IP address
		/// </summary>
		private ValueTask<IChannel> ConnectToAddress(string server, int port)
		{
			var channelTask = new TaskCompletionSource<IChannel>();

			_bootstrap.ConnectAsync(server, port).ContinueWith(task =>
			{
				if (task.Status == TaskStatus.RanToCompletion)
				{
					channelTask.SetResult(task.Result);
				}
				else
				{
					channelTask.SetException(task.Exception);
				}
			});

			return new ValueTask<IChannel>(channelTask.Task.Result);
		}

		public void Close()
		{
			try
			{
				_eventLoopGroup.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)).Wait();
			}
			catch (System.Exception e)
			{
				Log.LogWarning("EventLoopGroup shutdown was interrupted", e);
			}

		}

		private Task<ClientCnx> CleanupConnection(string server, int connectionKey)
		{
			Task<ClientCnx> connectionTask = null;
			var map = _pool[server];
			if (map != null)
			{
				map.Remove(connectionKey, out connectionTask);
			}
			return connectionTask;
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
			throw new NotImplementedException();
		}

		private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(ConnectionPool));
	}

}