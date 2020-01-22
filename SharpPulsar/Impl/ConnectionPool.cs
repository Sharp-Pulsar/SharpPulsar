using SharpPulsar.Configuration;
using SharpPulsar.Exception;
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
	using Bootstrap = DotNetty.Transport.Bootstrapping.Bootstrap;
	using Channel = DotNetty.Transport.Channels;
	using ChannelException = DotNetty.Transport.Channels.ChannelException;
	using ChannelOption = DotNetty.Transport.Channels.ChannelOption;
	using EventLoopGroup = DotNetty.Transport.Channels.AffinitizedEventLoopGroup;
	using DnsNameResolver = DotNetty.Transport.Bootstrapping.DefaultNameResolver;
	//using DnsNameResolverBuilder = DotNetty.Transport.Bootstrapping.DnsNameResolverBuilder;
	

	public class ConnectionPool : IDisposable
	{
		protected internal readonly ConcurrentDictionary<IPEndPoint, ConcurrentDictionary<int, ValueTask<ClientConnection>>> pool;

		private readonly Bootstrap bootstrap;
		private readonly EventLoopGroup eventLoopGroup;
		private readonly int maxConnectionsPerHosts;

		protected internal readonly DnsNameResolver dnsResolver;
		public ConnectionPool(ClientConfigurationData conf, EventLoopGroup eventLoopGroup) : this(conf, eventLoopGroup, () -> new ClientCnx(conf, eventLoopGroup))
		{
		}
		public ConnectionPool(ClientConfigurationData conf, EventLoopGroup eventLoopGroup, Func<ClientConnection> clientCnxSupplier)
		{
			this.eventLoopGroup = eventLoopGroup;
			this.maxConnectionsPerHosts = conf.ConnectionsPerBroker;

			pool = new ConcurrentDictionary<IPEndPoint, ConcurrentDictionary<int, ValueTask<ClientConnection>>>();
			bootstrap = new Bootstrap();
			bootstrap.Group(eventLoopGroup);
			bootstrap.Channel(EventLoopUtil.GetClientSocketChannelClass(eventLoopGroup));

			bootstrap.Option(ChannelOption.ConnectTimeout, conf.ConnectionTimeoutMs);
			bootstrap.Option(ChannelOption.TcpNodelay, conf.UseTcpNoDelay);
			bootstrap.Option(ChannelOption.Allocator, PulsarByteBufAllocator.DEFAULT);

			try
			{
				bootstrap.Handler(new PulsarChannelInitializer(conf, clientCnxSupplier));
			}
			catch (System.Exception e)
			{
				log.error("Failed to create channel initializer");
				throw new PulsarClientException(e);
			}

			dnsResolver = (new DnsNameResolverBuilder(eventLoopGroup.GetNext())).traceEnabled(true).channelType(EventLoopUtil.getDatagramChannelClass(eventLoopGroup)).build();
		}

		private static readonly Random random = new Random();
		public virtual ValueTask<ClientConnection> GetConnection(IPEndPoint address)
		{
			return GetConnection(address, address);
		}

		internal virtual void CloseAllConnections()
		{
			pool.Values.ToList().ForEach(map =>
			{
				map.Values.ToList().ForEach(cnx =>
				{
					if (cnx.IsCompleted)
					{
						if (!cnx.IsFaulted)
						{
							cnx.Result.Close();
						}
						else
						{
						}
					}
					else
					{
						cnx.AsTask().ContinueWith(x=> x.Result.Close());
						//future.thenAccept(ClientCnx.close);
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
		public virtual ValueTask<ClientConnection> GetConnection(IPEndPoint logicalAddress, IPEndPoint physicalAddress)
		{
			if (maxConnectionsPerHosts == 0)
			{
				// Disable pooling
				return CreateConnection(logicalAddress, physicalAddress, -1);
			}

			int randomKey = signSafeMod(random.Next(), maxConnectionsPerHosts);

			return pool.ComputeIfAbsent(logicalAddress, a => new ConcurrentDictionary<>()).computeIfAbsent(randomKey, k => createConnection(logicalAddress, physicalAddress, randomKey));
		}

		private ValueTask<ClientConnection> CreateConnection(IPEndPoint logicalAddress, IPEndPoint physicalAddress, int connectionKey)
		{
			
			if (log.DebugEnabled)
			{
				log.debug("Connection for {} not found in cache", logicalAddress);
			}

			ValueTask<ClientConnection> cnxFuture = new ValueTask<ClientConnection>();

			// Trigger async connect to broker
			CreateConnection(physicalAddress).thenAccept(channel =>
			{
				log.info("[{}] Connected to server", channel);
				channel.closeFuture().addListener(v =>
				{
					if (log.DebugEnabled)
					{
						log.debug("Removing closed connection from pool: {}", v);
					}
					cleanupConnection(logicalAddress, connectionKey, cnxFuture);
				});
			ClientCnx cnx = (ClientCnx) channel.pipeline().get("handler");
			if (!channel.Active || cnx == null)
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] Connection was already closed by the time we got notified", channel);
				}
				cnxFuture.completeExceptionally(new ChannelException("Connection already closed"));
				return;
			}
			if (!logicalAddress.Equals(physicalAddress))
			{
				cnx.TargetBroker = logicalAddress;
			}
			cnx.RemoteHostName = physicalAddress.HostName;
			cnx.connectionFuture().thenRun(() =>
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] Connection handshake completed", cnx.channel());
				}
				cnxFuture.complete(cnx);
			}).exceptionally(exception =>
			{
				log.warn("[{}] Connection handshake failed: {}", cnx.channel(), exception.Message);
				cnxFuture.completeExceptionally(exception);
				cleanupConnection(logicalAddress, connectionKey, cnxFuture);
				cnx.ctx().close();
				return null;
			});
			}).exceptionally(exception =>
			{
			eventLoopGroup.execute(() =>
			{
				log.warn("Failed to open connection to {} : {}", physicalAddress, exception.Message);
				cleanupConnection(logicalAddress, connectionKey, cnxFuture);
				cnxFuture.completeExceptionally(new PulsarClientException(exception));
			});
			return null;
		});

			return cnxFuture;
		}

		/// <summary>
		/// Resolve DNS asynchronously and attempt to connect to any IP address returned by DNS server
		/// </summary>
		private ValueTask<Channel> CreateConnection(SocketAddress unresolvedAddress)
		{
			string hostname = unresolvedAddress.ToString();
			int port = unresolvedAddress.Port;

			// Resolve DNS --> Attempt to connect to all IP addresses until once succeeds
			return resolveName(hostname).thenCompose(inetAddresses => connectToResolvedAddresses(inetAddresses.GetEnumerator(), port));
		}

		/// <summary>
		/// Try to connect to a sequence of IP addresses until a successfull connection can be made, or fail if no address is
		/// working
		/// </summary>
		private ValueTask<Channel> ConnectToResolvedAddresses(IEnumerator<IPAddress> unresolvedAddresses, int port)
		{
			CompletableFuture<Channel> future = new CompletableFuture<Channel>();

			connectToAddress(unresolvedAddresses.next(), port).thenAccept(channel =>
			{
			future.complete(channel);
			}).exceptionally(exception =>
			{
			if (unresolvedAddresses.hasNext())
			{
				connectToResolvedAddresses(unresolvedAddresses, port).thenAccept(channel =>
				{
					future.complete(channel);
				}).exceptionally(ex =>
				{
					future.completeExceptionally(ex);
					return null;
				});
			}
			else
			{
				future.completeExceptionally(exception);
			}
			return null;
		});

			return future;
		}

		internal virtual ValueTask<IList<IPAddress>> resolveName(string hostname)
		{
			CompletableFuture<IList<InetAddress>> future = new CompletableFuture<IList<InetAddress>>();
			dnsResolver.resolveAll(hostname).addListener((Future<IList<InetAddress>> resolveFuture) =>
			{
			if (resolveFuture.Success)
			{
				future.complete(resolveFuture.get());
			}
			else
			{
				future.completeExceptionally(resolveFuture.cause());
			}
			});
			return future;
		}

		/// <summary>
		/// Attempt to establish a TCP connection to an already resolved single IP address
		/// </summary>
		private CompletableFuture<Channel> connectToAddress(IPAddress ipAddress, int port)
		{
			CompletableFuture<Channel> future = new CompletableFuture<Channel>();

			bootstrap.connect(ipAddress, port).addListener((ChannelFuture channelFuture) =>
			{
			if (channelFuture.Success)
			{
				future.complete(channelFuture.channel());
			}
			else
			{
				future.completeExceptionally(channelFuture.cause());
			}
			});

			return future;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public virtual void Dispose()
		{
			try
			{
				eventLoopGroup.shutdownGracefully(0, 1, TimeUnit.SECONDS).await();
			}
			catch (InterruptedException e)
			{
				log.warn("EventLoopGroup shutdown was interrupted", e);
			}

			dnsResolver.close();
		}

		private void cleanupConnection(InetSocketAddress address, int connectionKey, CompletableFuture<ClientCnx> connectionFuture)
		{
			ConcurrentMap<int, CompletableFuture<ClientCnx>> map = pool[address];
			if (map != null)
			{
				map.remove(connectionKey, connectionFuture);
			}
		}

		public static int signSafeMod(long dividend, int divisor)
		{
			int mod = (int)(dividend % (long) divisor);
			if (mod < 0)
			{
				mod += divisor;
			}
			return mod;
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ConnectionPool));
	}

}