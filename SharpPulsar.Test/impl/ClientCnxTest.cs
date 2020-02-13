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
namespace Org.Apache.Pulsar.Client.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.fail;


	using Channel = io.netty.channel.Channel;
	using PulsarClientException = Org.Apache.Pulsar.Client.Api.PulsarClientException;
	using ClientConfigurationData = Org.Apache.Pulsar.Client.Impl.Conf.ClientConfigurationData;
	using PulsarApi = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi;
	using PulsarHandler = Org.Apache.Pulsar.Common.Protocol.PulsarHandler;
	using EventLoopUtil = Org.Apache.Pulsar.Common.Util.Netty.EventLoopUtil;
	using Test = org.testng.annotations.Test;

	using ChannelFuture = io.netty.channel.ChannelFuture;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using DefaultThreadFactory = io.netty.util.concurrent.DefaultThreadFactory;

	public class ClientCnxTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClientCnxTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestClientCnxTimeout()
		{
			EventLoopGroup EventLoop = EventLoopUtil.newEventLoopGroup(1, new DefaultThreadFactory("testClientCnxTimeout"));
			ClientConfigurationData Conf = new ClientConfigurationData();
			Conf.OperationTimeoutMs = 10;
			ClientCnx Cnx = new ClientCnx(Conf, EventLoop);

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
			catch (Exception E)
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
			catch (Exception)
			{
				fail("should not throw any error");
			}
		}

	}

}