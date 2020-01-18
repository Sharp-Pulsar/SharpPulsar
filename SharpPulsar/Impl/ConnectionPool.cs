using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;

	using Bootstrap = io.netty.bootstrap.Bootstrap;
	using Channel = io.netty.channel.Channel;
	using ChannelException = io.netty.channel.ChannelException;
	using ChannelFuture = io.netty.channel.ChannelFuture;
	using ChannelOption = io.netty.channel.ChannelOption;
	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using DnsNameResolver = io.netty.resolver.dns.DnsNameResolver;
	using DnsNameResolverBuilder = io.netty.resolver.dns.DnsNameResolverBuilder;
	using Future = io.netty.util.concurrent.Future;


	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using ClientConfigurationData = SharpPulsar.Impl.conf.ClientConfigurationData;
	using PulsarByteBufAllocator = org.apache.pulsar.common.allocator.PulsarByteBufAllocator;
	using EventLoopUtil = org.apache.pulsar.common.util.netty.EventLoopUtil;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class ConnectionPool : System.IDisposable
	{
		protected internal readonly ConcurrentDictionary<InetSocketAddress, ConcurrentMap<int, CompletableFuture<ClientCnx>>> pool;

		private readonly Bootstrap bootstrap;
		private readonly EventLoopGroup eventLoopGroup;
		private readonly int maxConnectionsPerHosts;

		protected internal readonly DnsNameResolver dnsResolver;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ConnectionPool(SharpPulsar.Impl.conf.ClientConfigurationData conf, io.netty.channel.EventLoopGroup eventLoopGroup) throws org.apache.pulsar.client.api.PulsarClientException
		public ConnectionPool(ClientConfigurationData conf, EventLoopGroup eventLoopGroup) : this(conf, eventLoopGroup, () -> new ClientCnx(conf, eventLoopGroup))
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ConnectionPool(SharpPulsar.Impl.conf.ClientConfigurationData conf, io.netty.channel.EventLoopGroup eventLoopGroup, java.util.function.Supplier<ClientCnx> clientCnxSupplier) throws org.apache.pulsar.client.api.PulsarClientException
		public ConnectionPool(ClientConfigurationData conf, EventLoopGroup eventLoopGroup, System.Func<ClientCnx> clientCnxSupplier)
		{
			this.eventLoopGroup = eventLoopGroup;
			this.maxConnectionsPerHosts = conf.ConnectionsPerBroker;

			pool = new ConcurrentDictionary<InetSocketAddress, ConcurrentMap<int, CompletableFuture<ClientCnx>>>();
			bootstrap = new Bootstrap();
			bootstrap.group(eventLoopGroup);
			bootstrap.channel(EventLoopUtil.getClientSocketChannelClass(eventLoopGroup));

			bootstrap.option(ChannelOption.CONNECT_TIMEOUT_MILLIS, conf.ConnectionTimeoutMs);
			bootstrap.option(ChannelOption.TCP_NODELAY, conf.UseTcpNoDelay);
			bootstrap.option(ChannelOption.ALLOCATOR, PulsarByteBufAllocator.DEFAULT);

			try
			{
				bootstrap.handler(new PulsarChannelInitializer(conf, clientCnxSupplier));
			}
			catch (Exception e)
			{
				log.error("Failed to create channel initializer");
				throw new PulsarClientException(e);
			}

			this.dnsResolver = (new DnsNameResolverBuilder(eventLoopGroup.next())).traceEnabled(true).channelType(EventLoopUtil.getDatagramChannelClass(eventLoopGroup)).build();
		}

		private static readonly Random random = new Random();

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public java.util.concurrent.CompletableFuture<ClientCnx> getConnection(final java.net.InetSocketAddress address)
		public virtual CompletableFuture<ClientCnx> getConnection(InetSocketAddress address)
		{
			return getConnection(address, address);
		}

		internal virtual void closeAllConnections()
		{
			pool.Values.forEach(map =>
			{
			map.values().forEach(future =>
			{
				if (future.Done)
				{
					if (!future.CompletedExceptionally)
					{
						future.join().close();
					}
					else
					{
					}
				}
				else
				{
					future.thenAccept(ClientCnx.close);
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
		public virtual CompletableFuture<ClientCnx> getConnection(InetSocketAddress logicalAddress, InetSocketAddress physicalAddress)
		{
			if (maxConnectionsPerHosts == 0)
			{
				// Disable pooling
				return createConnection(logicalAddress, physicalAddress, -1);
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int randomKey = signSafeMod(random.nextInt(), maxConnectionsPerHosts);
			int randomKey = signSafeMod(random.Next(), maxConnectionsPerHosts);

			return pool.computeIfAbsent(logicalAddress, a => new ConcurrentDictionary<>()).computeIfAbsent(randomKey, k => createConnection(logicalAddress, physicalAddress, randomKey));
		}

		private CompletableFuture<ClientCnx> createConnection(InetSocketAddress logicalAddress, InetSocketAddress physicalAddress, int connectionKey)
		{
			if (log.DebugEnabled)
			{
				log.debug("Connection for {} not found in cache", logicalAddress);
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<ClientCnx> cnxFuture = new java.util.concurrent.CompletableFuture<ClientCnx>();
			CompletableFuture<ClientCnx> cnxFuture = new CompletableFuture<ClientCnx>();

			// Trigger async connect to broker
			createConnection(physicalAddress).thenAccept(channel =>
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
		private CompletableFuture<Channel> createConnection(InetSocketAddress unresolvedAddress)
		{
			string hostname = unresolvedAddress.HostString;
			int port = unresolvedAddress.Port;

			// Resolve DNS --> Attempt to connect to all IP addresses until once succeeds
			return resolveName(hostname).thenCompose(inetAddresses => connectToResolvedAddresses(inetAddresses.GetEnumerator(), port));
		}

		/// <summary>
		/// Try to connect to a sequence of IP addresses until a successfull connection can be made, or fail if no address is
		/// working
		/// </summary>
		private CompletableFuture<Channel> connectToResolvedAddresses(IEnumerator<InetAddress> unresolvedAddresses, int port)
		{
			CompletableFuture<Channel> future = new CompletableFuture<Channel>();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting CompletableFuture<java.util.List<java.net.InetAddress>> resolveName(String hostname)
		internal virtual CompletableFuture<IList<InetAddress>> resolveName(string hostname)
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
		private CompletableFuture<Channel> connectToAddress(InetAddress ipAddress, int port)
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