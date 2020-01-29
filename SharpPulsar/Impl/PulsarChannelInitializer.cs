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
    using DotNetty.Codecs;
    using DotNetty.Handlers.Tls;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;
    using SharpPulsar.Api;
    using SharpPulsar.Impl.Conf;
    using SharpPulsar.Protocol;
    using SharpPulsar.Shared;
    using SharpPulsar.Util;
    using System.Security.Cryptography.X509Certificates;

    public class PulsarChannelInitializer : ChannelInitializer<IChannel>
	{
		public ClientConfigurationData _conf;

		private readonly Func<ClientCnx> clientCnxSupplier;
		private readonly bool tlsEnabled;

		private readonly Func<TlsHandler> sslContextSupplier;

		private static readonly long TLS_CERTIFICATE_CACHE_MILLIS = BAMCIS.Util.Concurrent.TimeUnit.MINUTES.ToMillis(1);

		public PulsarChannelInitializer(ClientConfigurationData conf, Func<ClientCnx> ClientCnxSupplier) : base()
		{
			clientCnxSupplier = ClientCnxSupplier;
			tlsEnabled = conf.UseTls;
			_conf = conf;
			/*if (Conf.UseTls)
			{
				sslContextSupplier = new ObjectCache<TlsHandler>(() =>
				{
						try
						{
						
							IAuthenticationDataProvider AuthData = Conf.Authentication.AuthData;
							if (AuthData.HasDataForTls())
							{
								return SecurityUtility.CreateNettySslContextForClient(Conf.TlsAllowInsecureConnection, Conf.TlsTrustCertsFilePath, (X509Certificate2[]) AuthData.TlsCertificates, AuthData.TlsPrivateKey);
							}
							else
							{
								return SecurityUtility.CreateNettySslContextForClient(Conf.TlsAllowInsecureConnection, Conf.TlsTrustCertsFilePath);
							}
						}
						catch (System.Exception E)
						{
							throw new System.Exception("Failed to create TLS context", E);
						}
				}, TLS_CERTIFICATE_CACHE_MILLIS, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
			}
			else
			{
				sslContextSupplier = null;
			}*/
		}

		protected override void InitChannel(IChannel ch)
		{
			if (tlsEnabled)
			{
				ch.Pipeline.AddLast(TlsHandler.Client(_conf.ServiceUrl, _conf.Authentication.AuthData.TlsCertificates[0]));
				ch.Pipeline.AddLast("ByteBufPairEncoder", ByteBufPair.COPYINGENCODER);
			}
			else
			{
				ch.Pipeline.AddLast("ByteBufPairEncoder", ByteBufPair.ENCODER);
			}

			ch.Pipeline.AddLast("frameDecoder", new LengthFieldBasedFrameDecoder(Commands.DefaultMaxMessageSize + Commands.MessageSizeFramePadding, 0, 4, 0, 4));
			ch.Pipeline.AddLast("handler", clientCnxSupplier.Invoke());
		}

	}

}