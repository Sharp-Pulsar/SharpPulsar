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

	using IAuthenticationDataProvider = SharpPulsar.Api.IAuthenticationDataProvider;
	using ClientConfigurationData = SharpPulsar.Impl.Conf.ClientConfigurationData;
	using SharpPulsar.Util;
	using ByteBufPair = Org.Apache.Pulsar.Common.Protocol.ByteBufPair;
	using Commands = Org.Apache.Pulsar.Common.Protocol.Commands;
	using SecurityUtility = Org.Apache.Pulsar.Common.Util.SecurityUtility;

	public class PulsarChannelInitializer : ChannelInitializer<SocketChannel>
	{

		public const string TlsHandler = "tls";

		private readonly System.Func<ClientCnx> clientCnxSupplier;
		private readonly bool tlsEnabled;

		private readonly System.Func<SslContext> sslContextSupplier;

		private static readonly long TLS_CERTIFICATE_CACHE_MILLIS = BAMCIS.Util.Concurrent.TimeUnit.MINUTES.toMillis(1);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PulsarChannelInitializer(SharpPulsar.impl.conf.ClientConfigurationData conf, java.util.function.Supplier<ClientCnx> clientCnxSupplier) throws Exception
		public PulsarChannelInitializer(ClientConfigurationData Conf, System.Func<ClientCnx> ClientCnxSupplier) : base()
		{
			this.clientCnxSupplier = ClientCnxSupplier;
			this.tlsEnabled = Conf.UseTls;

			if (Conf.UseTls)
			{
				sslContextSupplier = new ObjectCache<SslContext>(() =>
				{
				try
				{
					IAuthenticationDataProvider AuthData = Conf.Authentication.AuthData;
					if (AuthData.hasDataForTls())
					{
						return SecurityUtility.createNettySslContextForClient(Conf.TlsAllowInsecureConnection, Conf.TlsTrustCertsFilePath, (X509Certificate[]) AuthData.TlsCertificates, AuthData.TlsPrivateKey);
					}
					else
					{
						return SecurityUtility.createNettySslContextForClient(Conf.TlsAllowInsecureConnection, Conf.TlsTrustCertsFilePath);
					}
				}
				catch (Exception E)
				{
					throw new Exception("Failed to create TLS context", E);
				}
				}, TLS_CERTIFICATE_CACHE_MILLIS, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
			}
			else
			{
				sslContextSupplier = null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void initChannel(io.netty.channel.socket.SocketChannel ch) throws Exception
		public override void InitChannel(SocketChannel Ch)
		{
			if (tlsEnabled)
			{
				Ch.pipeline().addLast(TlsHandler, sslContextSupplier.get().newHandler(Ch.alloc()));
				Ch.pipeline().addLast("ByteBufPairEncoder", ByteBufPair.COPYING_ENCODER);
			}
			else
			{
				Ch.pipeline().addLast("ByteBufPairEncoder", ByteBufPair.ENCODER);
			}

			Ch.pipeline().addLast("frameDecoder", new LengthFieldBasedFrameDecoder(Commands.DEFAULT_MAX_MESSAGE_SIZE + Commands.MESSAGE_SIZE_FRAME_PADDING, 0, 4, 0, 4));
			Ch.pipeline().addLast("handler", clientCnxSupplier.get());
		}
	}

}