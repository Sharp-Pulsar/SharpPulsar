

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

using System.Threading.Channels;
using DotNetty.Transport.Channels;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Utility.Netty;

namespace SharpPulsar.Test.Impl
{
	public class ClientCnxTest
	{
		public virtual void TestClientCnxTimeout()
		{
			var eventLoop = new MultithreadEventLoopGroup(1); 
			ClientConfigurationData Conf = new ClientConfigurationData();
			Conf.OperationTimeoutMs = 10;
			ClientCnx Cnx = new ClientCnx(Conf, eventLoop);

			ChannelHandlerContext Ctx = mock(typeof(ChannelHandlerContext));
			ChannelFuture ListenerFuture = mock(typeof(ChannelFuture));
			when(ListenerFuture.addListener(any())).thenReturn(ListenerFuture);
			when(Ctx.writeAndFlush(any())).thenReturn(ListenerFuture);

			System.Reflection.FieldInfo CtxField = typeof(PulsarHandler).getDeclaredField("ctx");
			CtxField.Accessible = true;
			CtxField.set(Cnx, Ctx);
			try
			{
				Cnx.newLookup(null, 123).get();
			}
			catch (System.Exception E)
			{
				assertTrue(E.InnerException is PulsarClientException.TimeoutException);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testReceiveErrorAtSendConnectFrameState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestReceiveErrorAtSendConnectFrameState()
		{
			ThreadFactory ThreadFactory = new DefaultThreadFactory("testReceiveErrorAtSendConnectFrameState");
			EventLoopGroup EventLoop = EventLoopUtil.newEventLoopGroup(1, ThreadFactory);
			ClientConfigurationData Conf = new ClientConfigurationData();
			Conf.OperationTimeoutMs = 10;
			ClientCnx Cnx = new ClientCnx(Conf, EventLoop);

			ChannelHandlerContext Ctx = mock(typeof(ChannelHandlerContext));
			Channel Channel = mock(typeof(Channel));
			when(Ctx.channel()).thenReturn(Channel);

			System.Reflection.FieldInfo CtxField = typeof(PulsarHandler).getDeclaredField("ctx");
			CtxField.Accessible = true;
			CtxField.set(Cnx, Ctx);

			// set connection as SentConnectFrame
			System.Reflection.FieldInfo CnxField = typeof(ClientCnx).getDeclaredField("state");
			CnxField.Accessible = true;
			CnxField.set(Cnx, ClientCnx.State.SentConnectFrame);

			// receive error
			PulsarApi.CommandError CommandError = PulsarApi.CommandError.newBuilder().setRequestId(-1).setError(PulsarApi.ServerError.AuthenticationError).setMessage("authentication was failed").build();
			try
			{
				Cnx.handleError(CommandError);
			}
			catch (System.Exception)
			{
				fail("should not throw any error");
			}
		}

	}

}