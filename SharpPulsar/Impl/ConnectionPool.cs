﻿using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using SharpPulsar.Common.Allocator;
using SharpPulsar.Exception;
using SharpPulsar.Impl.Conf;
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
		protected internal readonly ConcurrentDictionary<DnsEndPoint, ConcurrentDictionary<int, Task<ClientCnx>>> Pool;

		private readonly Bootstrap bootstrap;
		private readonly MultithreadEventLoopGroup eventLoopGroup;
		private readonly int maxConnectionsPerHosts;


		public ConnectionPool(ClientConfigurationData Conf, MultithreadEventLoopGroup eventLoopGroup) : this(Conf, eventLoopGroup, () => new ClientCnx(Conf, eventLoopGroup))
		{
		}

		public ConnectionPool(ClientConfigurationData Conf, MultithreadEventLoopGroup eventLoopGroup, Func<ClientCnx> ClientCnxSupplier)
		{
			this.eventLoopGroup = eventLoopGroup;
			this.maxConnectionsPerHosts = Conf.ConnectionsPerBroker;

			Pool = new ConcurrentDictionary<DnsEndPoint, ConcurrentDictionary<int, Task<ClientCnx>>>();
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
				log.error("Failed to create channel initializer");
				throw new PulsarClientException(e.Message);
			}

			//this.DnsResolver = (new DnsNameResolverBuilder(eEventLoopGroup. .next())).traceEnabled(true).channelType(EventLoopUtil.getDatagramChannelClass(EventLoopGroup)).build();
		}

		private static readonly Random random = new Random();

		public virtual ValueTask<ClientCnx> GetConnection(in EndPoint address)
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
		public virtual ValueTask<ClientCnx> GetConnection(EndPoint logicalAddress, EndPoint physicalAddress)
		{
			if (maxConnectionsPerHosts == 0)
			{
				// Disable pooling
				return CreateConnection(logicalAddress, physicalAddress, -1);
			}

			int RandomKey = SignSafeMod(random.Next(), maxConnectionsPerHosts);

			return Pool.computeIfAbsent(LogicalAddress, a => new ConcurrentDictionary<>()).computeIfAbsent(RandomKey, k => CreateConnection(LogicalAddress, PhysicalAddress, RandomKey));
		}

		private ValueTask<ClientCnx> CreateConnection(EndPoint logicalAddress, EndPoint physicalAddress, int ConnectionKey)
		{
			if (log.DebugEnabled)
			{
				log.debug("Connection for {} not found in cache", logicalAddress);
			}

			TaskCompletionSource<ClientCnx> cxnTask = new TaskCompletionSource<ClientCnx>();

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
				CleanupConnection(LogicalAddress, ConnectionKey, CnxFuture);
			});
			ClientCnx Cnx = (ClientCnx) channel.pipeline().get("handler");
			if (!channel.Active || Cnx == null)
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] Connection was already closed by the time we got notified", channel);
				}
				CnxFuture.completeExceptionally(new ChannelException("Connection already closed"));
				return;
			}
			if (!LogicalAddress.Equals(PhysicalAddress))
			{
				Cnx.TargetBroker = LogicalAddress;
			}
			Cnx.RemoteHostName = PhysicalAddress.HostName;
			Cnx.connectionFuture().thenRun(() =>
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] Connection handshake completed", Cnx.channel());
				}
				CnxFuture.complete(Cnx);
			}).exceptionally(exception =>
			{
				log.warn("[{}] Connection handshake failed: {}", Cnx.channel(), exception.Message);
				CnxFuture.completeExceptionally(exception);
				CleanupConnection(LogicalAddress, ConnectionKey, CnxFuture);
				Cnx.ctx().close();
				return null;
			});
			}).exceptionally(exception =>
			{
			eventLoopGroup.execute(() =>
			{
				log.warn("Failed to open connection to {} : {}", PhysicalAddress, exception.Message);
				CleanupConnection(LogicalAddress, ConnectionKey, CnxFuture);
				CnxFuture.completeExceptionally(new PulsarClientException(exception));
			});
			return null;
		});

			return CnxFuture;
		}

		/// <summary>
		/// Resolve DNS asynchronously and attempt to connect to any IP address returned by DNS server
		/// </summary>
		private CompletableFuture<Channel> CreateConnection(InetSocketAddress UnresolvedAddress)
		{
			string Hostname = UnresolvedAddress.HostString;
			int Port = UnresolvedAddress.Port;

			// Resolve DNS --> Attempt to connect to all IP addresses until once succeeds
			return ResolveName(Hostname).thenCompose(inetAddresses => ConnectToResolvedAddresses(inetAddresses.GetEnumerator(), Port));
		}

		/// <summary>
		/// Try to connect to a sequence of IP addresses until a successfull connection can be made, or fail if no address is
		/// working
		/// </summary>
		private CompletableFuture<Channel> ConnectToResolvedAddresses(IEnumerator<InetAddress> UnresolvedAddresses, int Port)
		{
			CompletableFuture<Channel> Future = new CompletableFuture<Channel>();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			ConnectToAddress(UnresolvedAddresses.next(), Port).thenAccept(channel =>
			{
			Future.complete(channel);
			}).exceptionally(exception =>
			{
			if (UnresolvedAddresses.hasNext())
			{
				ConnectToResolvedAddresses(UnresolvedAddresses, Port).thenAccept(channel =>
				{
					Future.complete(channel);
				}).exceptionally(ex =>
				{
					Future.completeExceptionally(ex);
					return null;
				});
			}
			else
			{
				Future.completeExceptionally(exception);
			}
			return null;
		});

			return Future;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting CompletableFuture<java.util.List<java.net.InetAddress>> resolveName(String hostname)
		public virtual CompletableFuture<IList<InetAddress>> ResolveName(string Hostname)
		{
			CompletableFuture<IList<InetAddress>> Future = new CompletableFuture<IList<InetAddress>>();
			DnsResolver.resolveAll(Hostname).addListener((Future<IList<InetAddress>> ResolveFuture) =>
			{
			if (ResolveFuture.Success)
			{
				Future.complete(ResolveFuture.get());
			}
			else
			{
				Future.completeExceptionally(ResolveFuture.cause());
			}
			});
			return Future;
		}

		/// <summary>
		/// Attempt to establish a TCP connection to an already resolved single IP address
		/// </summary>
		private CompletableFuture<Channel> ConnectToAddress(InetAddress IpAddress, int Port)
		{
			CompletableFuture<Channel> Future = new CompletableFuture<Channel>();

			bootstrap.connect(IpAddress, Port).addListener((ChannelFuture ChannelFuture) =>
			{
			if (ChannelFuture.Success)
			{
				Future.complete(ChannelFuture.channel());
			}
			else
			{
				Future.completeExceptionally(ChannelFuture.cause());
			}
			});

			return Future;
		}

		public void Close()
		{
			try
			{
				eventLoopGroup.shutdownGracefully(0, 1, BAMCIS.Util.Concurrent.TimeUnit.SECONDS).await();
			}
			catch (InterruptedException E)
			{
				log.warn("EventLoopGroup shutdown was interrupted", E);
			}

			DnsResolver.close();
		}

		private void CleanupConnection(InetSocketAddress Address, int ConnectionKey, CompletableFuture<ClientCnx> ConnectionFuture)
		{
			ConcurrentMap<int, CompletableFuture<ClientCnx>> Map = Pool[Address];
			if (Map != null)
			{
				Map.remove(ConnectionKey, ConnectionFuture);
			}
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

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ConnectionPool));
	}

}