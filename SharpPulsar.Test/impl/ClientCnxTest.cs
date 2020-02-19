

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

using System.Reflection;
using System.Threading.Channels;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Moq;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utility.Netty;
using Xunit;

namespace SharpPulsar.Test.Impl
{
	public class ClientCnxTest
	{
		[Fact]
		public void TestClientCnxTimeout()
		{
			var eventLoop = new MultithreadEventLoopGroup(1);
            var conf = new ClientConfigurationData {OperationTimeoutMs = 10};
            var cnx = new ClientCnx(conf, eventLoop);

            var mock = new Mock<IChannelHandlerContext>();
            var ctx = mock.Object;
			var mock2 = new Mock<Task>();
            var listenerFuture = mock2.Object;
			mock2.Setup(x => x.ContinueWith(t=>It.IsAny<Task>()).Result).Returns(listenerFuture);
			mock.Setup(x => x.WriteAndFlushAsync(It.IsAny<object>())).Returns(listenerFuture);

			var ctxField = typeof(PulsarHandler).GetField("Context", BindingFlags.NonPublic | BindingFlags.Instance);
			//ctxField.Accessible = true;
			ctxField.SetValue(cnx, ctx);
			try
			{
				cnx.NewLookup(null, 123);
			}
			catch (System.Exception e)
			{
				Assert.True(e.InnerException is PulsarClientException.TimeoutException);
			}
		}
		[Fact]
		public void TestReceiveErrorAtSendConnectFrameState()
		{
			var eventLoop = new MultithreadEventLoopGroup(1);
            var conf = new ClientConfigurationData {OperationTimeoutMs = 10};
            var cnx = new ClientCnx(conf, eventLoop);

            var mock = new Mock<IChannelHandlerContext>();
            var ctx = mock.Object;
			var channel = new Mock<IChannel>().Object;
			mock.Setup(x =>x.Channel).Returns(channel);

			var ctxField = typeof(PulsarHandler).GetField("Context", BindingFlags.NonPublic | BindingFlags.Instance);
			//ctxField.Accessible = true;
			ctxField?.SetValue(cnx, ctx);

			// set connection as SentConnectFrame
			var cnxField = typeof(ClientCnx).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);
			
			cnxField?.SetValue(cnx, ClientCnx.State.SentConnectFrame);

			// receive error
			var commandError = CommandError.NewBuilder().SetRequestId(-1).SetError(ServerError.AuthenticationError).SetMessage("authentication was failed").Build();
			try
			{
				cnx.HandleError(commandError);
			}
			catch (System.Exception)
			{
				Assert.False(false,"should not throw any error");
			}
		}

	}

}