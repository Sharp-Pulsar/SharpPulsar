using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using SharpPulsar.Common.Allocator;
using SharpPulsar.Exception;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Util;
using SharpPulsar.Util.Netty;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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

	public class ConnectionPool : IDisposable
	{
		protected internal readonly ConcurrentDictionary<string, ConcurrentDictionary<int, Task<ClientCnx>>> Pool;

		private readonly Bootstrap bootstrap;
		private readonly MultithreadEventLoopGroup eventLoopGroup;
		private readonly int maxConnectionsPerHosts;
		private DefaultNameResolver _dnsResolver;
		private ClientConfigurationData _conf;

		public ConnectionPool(ClientConfigurationData Conf, MultithreadEventLoopGroup eventLoopGroup) : this(Conf, eventLoopGroup, () => new ClientCnx(Conf, eventLoopGroup))
		{
		}

		public ConnectionPool(ClientConfigurationData Conf, MultithreadEventLoopGroup eventLoopGroup, Func<ClientCnx> ClientCnxSupplier)
		{
			this.eventLoopGroup = eventLoopGroup;
			this.maxConnectionsPerHosts = Conf.ConnectionsPerBroker;
			_conf = Conf;
			Pool = new ConcurrentDictionary<string, ConcurrentDictionary<int, Task<ClientCnx>>>();
			bootstrap = new Bootstrap();
			bootstrap.Group(eventLoopGroup);
			bootstrap.Channel<TcpSocketChannel>();

			bootstrap.Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(Conf.ConnectionTimeoutMs));
			bootstrap.Option(ChannelOption.TcpNodelay, Conf.UseTcpNoDelay);
			bootstrap.Option(ChannelOption.Allocator, PulsarByteBufAllocator.DEFAULT);

			try
			{
				bootstrap.Handler(new PulsarChannelInitializer(Conf, ClientCnxSupplier));
			}
			catch (System.Exception e)
			{
				log.LogError("Failed to create channel initializer");
				throw new PulsarClientException(e.Message);
			}

			_dnsResolver = new DefaultNameResolver();
			//this.DnsResolver = (new DnsNameResolverBuilder(eEventLoopGroup. .next())).traceEnabled(true).channelType(EventLoopUtil.getDatagramChannelClass(EventLoopGroup)).build();
		}

		private static readonly Random random = new Random();

		public virtual ValueTask<ClientCnx> GetConnection(in IPEndPoint address)
		{
			return GetConnection(address, address);
		}

		public virtual void CloseAllConnections()
		{
			Pool.Values.ToList().ForEach(map =>
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
		public virtual ValueTask<ClientCnx> GetConnection(IPEndPoint logicalAddress, IPEndPoint physicalAddress)
		{
			var host = Dns.GetHostEntry(logicalAddress.Address.ToString()).HostName;
			if (maxConnectionsPerHosts == 0)
			{
				
				// Disable pooling
				return CreateConnection(logicalAddress, physicalAddress, -1);
			}

			int randomKey = SignSafeMod(random.Next(), maxConnectionsPerHosts);
			var client = Pool.GetOrAdd(host, x => new ConcurrentDictionary<int, Task<ClientCnx>>()).GetOrAdd(randomKey, k => CreateConnection(logicalAddress, physicalAddress, randomKey).AsTask());
			return new ValueTask<ClientCnx>(client);
		}

		private ValueTask<ClientCnx> CreateConnection(IPEndPoint logicalAddress, IPEndPoint physicalAddress, int ConnectionKey)
		{
			if(log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("Connection for {} not found in cache", logicalAddress);
			}
			TaskCompletionSource<ClientCnx> cxnTask = new TaskCompletionSource<ClientCnx>();
			// Trigger async connect to broker
			CreateConnection(physicalAddress).AsTask().ContinueWith(connectionTask =>
			{
				if(connectionTask.Status != TaskStatus.Faulted)
				{
					log.LogInformation("[{}] Connected to server", connectionTask.Result);
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
					ClientCnx cnx = (ClientCnx)channel.Pipeline.Get("handler");
					if (!channel.Active || cnx == null)
					{
						if(log.IsEnabled(LogLevel.Debug))
						{
							log.LogDebug("[{}] Connection was already closed by the time we got notified", channel);
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
							if (log.IsEnabled(LogLevel.Debug))
							{
								log.LogDebug("[{}] Connection handshake completed", cnnx.Result.Channel());
							}
							cxnTask.SetResult(cnnx.Result);
						}
						else
						{
							log.LogWarning("[{}] Connection handshake failed: {}", cnnx.Result.Channel(), cnnx.Exception.Message);
							cxnTask.SetException(cnnx.Exception);
							CleanupConnection(cnx.RemoteHostName, ConnectionKey);
							cnnx.Result.Ctx().CloseAsync();
						}

					});
				}
				else
				{
					log.LogWarning("Failed to open connection to {} : {}", physicalAddress, connectionTask.Exception.Message);
					CleanupConnection(connectionTask.Result.RemoteAddress.ToString(), ConnectionKey);
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
			string hostname = unresolvedAddress.Address.ToString();
			int port = unresolvedAddress.Port;

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
			TaskCompletionSource<IChannel> channelTask = new TaskCompletionSource<IChannel>();
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

		public virtual ValueTask<IList<IPAddress>> ResolveName(string hostname)
		{
			return new ValueTask<IList<IPAddress>>(Task.FromResult<IList<IPAddress>>(Dns.GetHostEntry(hostname).AddressList.ToList()));
			
		}

		/// <summary>
		/// Attempt to establish a TCP connection to an already resolved single IP address
		/// </summary>
		private ValueTask<IChannel> ConnectToAddress(string server, int port)
		{
			TaskCompletionSource<IChannel> channelTask = new TaskCompletionSource<IChannel>();

			bootstrap.ConnectAsync(server, port).ContinueWith(task =>
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
				eventLoopGroup.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)).Wait();
			}
			catch (System.Exception e)
			{
				log.LogWarning("EventLoopGroup shutdown was interrupted", e);
			}

		}

		private Task<ClientCnx> CleanupConnection(string server, int connectionKey)
		{
			Task<ClientCnx> connectionTask = null;
			ConcurrentDictionary<int, Task<ClientCnx>> map = Pool[server];
			if (map != null)
			{
				map.Remove(connectionKey, out connectionTask);
			}
			return connectionTask;
		}

		public static int SignSafeMod(long Dividend, int Divisor)
		{
			int Mod = (int)(Dividend % (long) Divisor);
			if (Mod < 0)
			{
				Mod += Divisor;
			}
			return Mod;
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		private static readonly ILogger log = new LoggerFactory().CreateLogger(typeof(ConnectionPool));
	}

}