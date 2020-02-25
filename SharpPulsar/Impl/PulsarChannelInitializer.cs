using System;
using Avro;
using DotNetty.Codecs.Protobuf;
using Google.Protobuf;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utility;

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
    using Conf;
    using Protocol;
    using Shared;

    public class PulsarChannelInitializer : ChannelInitializer<IChannel>
	{
		public ClientConfigurationData Conf;

		private readonly Func<ClientCnx> _clientCnxSupplier;
		private readonly bool _tlsEnabled;

		public PulsarChannelInitializer(ClientConfigurationData conf, Func<ClientCnx> clientCnxSupplier) 
		{
			_clientCnxSupplier = clientCnxSupplier;
			_tlsEnabled = conf.UseTls;
			Conf = conf;
		}

		protected override void InitChannel(IChannel ch)
		{
			if (_tlsEnabled)
			{
				ch.Pipeline.AddLast("tls", TlsHandler.Client(Conf.ServiceUrl, Conf.Authentication.AuthData.TlsCertificates[0]));
			}
            ch.Pipeline.AddLast(new ProtobufVarint32FrameDecoder());
            ch.Pipeline.AddLast(new ProtobufDecoder(BaseCommand.Parser));

			ch.Pipeline.AddLast(new ProtobufVarint32LengthFieldPrepender());
            ch.Pipeline.AddLast(new ProtobufEncoder());
            //ch.Pipeline.AddLast("handler", _clientCnxSupplier.Invoke());
		}

	}

}