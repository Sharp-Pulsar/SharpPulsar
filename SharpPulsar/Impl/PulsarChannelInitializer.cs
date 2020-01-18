using System;

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

	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using SocketChannel = io.netty.channel.socket.SocketChannel;
	using LengthFieldBasedFrameDecoder = io.netty.handler.codec.LengthFieldBasedFrameDecoder;
	using SslContext = io.netty.handler.ssl.SslContext;

	using AuthenticationDataProvider = org.apache.pulsar.client.api.AuthenticationDataProvider;
	using ClientConfigurationData = SharpPulsar.Impl.conf.ClientConfigurationData;
	using org.apache.pulsar.client.util;
	using ByteBufPair = org.apache.pulsar.common.protocol.ByteBufPair;
	using Commands = org.apache.pulsar.common.protocol.Commands;
	using SecurityUtility = org.apache.pulsar.common.util.SecurityUtility;

	public class PulsarChannelInitializer : ChannelInitializer<SocketChannel>
	{

		public const string TLS_HANDLER = "tls";

		private readonly System.Func<ClientCnx> clientCnxSupplier;
		private readonly bool tlsEnabled;

		private readonly System.Func<SslContext> sslContextSupplier;

		private static readonly long TLS_CERTIFICATE_CACHE_MILLIS = TimeUnit.MINUTES.toMillis(1);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PulsarChannelInitializer(SharpPulsar.Impl.conf.ClientConfigurationData conf, java.util.function.Supplier<ClientCnx> clientCnxSupplier) throws Exception
		public PulsarChannelInitializer(ClientConfigurationData conf, System.Func<ClientCnx> clientCnxSupplier) : base()
		{
			this.clientCnxSupplier = clientCnxSupplier;
			this.tlsEnabled = conf.UseTls;

			if (conf.UseTls)
			{
				sslContextSupplier = new ObjectCache<SslContext>(() =>
				{
				try
				{
					AuthenticationDataProvider authData = conf.Authentication.AuthData;
					if (authData.hasDataForTls())
					{
						return SecurityUtility.createNettySslContextForClient(conf.TlsAllowInsecureConnection, conf.TlsTrustCertsFilePath, (X509Certificate[]) authData.TlsCertificates, authData.TlsPrivateKey);
					}
					else
					{
						return SecurityUtility.createNettySslContextForClient(conf.TlsAllowInsecureConnection, conf.TlsTrustCertsFilePath);
					}
				}
				catch (Exception e)
				{
					throw new Exception("Failed to create TLS context", e);
				}
				}, TLS_CERTIFICATE_CACHE_MILLIS, TimeUnit.MILLISECONDS);
			}
			else
			{
				sslContextSupplier = null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void initChannel(io.netty.channel.socket.SocketChannel ch) throws Exception
		public override void initChannel(SocketChannel ch)
		{
			if (tlsEnabled)
			{
				ch.pipeline().addLast(TLS_HANDLER, sslContextSupplier.get().newHandler(ch.alloc()));
				ch.pipeline().addLast("ByteBufPairEncoder", ByteBufPair.COPYING_ENCODER);
			}
			else
			{
				ch.pipeline().addLast("ByteBufPairEncoder", ByteBufPair.ENCODER);
			}

			ch.pipeline().addLast("frameDecoder", new LengthFieldBasedFrameDecoder(Commands.DEFAULT_MAX_MESSAGE_SIZE + Commands.MESSAGE_SIZE_FRAME_PADDING, 0, 4, 0, 4));
			ch.pipeline().addLast("handler", clientCnxSupplier.get());
		}
	}

}