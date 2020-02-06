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
namespace org.apache.pulsar.client.impl
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
	using PulsarClientException = api.PulsarClientException;
	using ClientConfigurationData = conf.ClientConfigurationData;
	using PulsarApi = common.api.proto.PulsarApi;
	using PulsarHandler = common.protocol.PulsarHandler;
	using EventLoopUtil = common.util.netty.EventLoopUtil;
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
		public virtual void testClientCnxTimeout()
		{
			EventLoopGroup eventLoop = EventLoopUtil.newEventLoopGroup(1, new DefaultThreadFactory("testClientCnxTimeout"));
			ClientConfigurationData conf = new ClientConfigurationData();
			conf.OperationTimeoutMs = 10;
			ClientCnx cnx = new ClientCnx(conf, eventLoop);

			ChannelHandlerContext ctx = mock(typeof(ChannelHandlerContext));
			ChannelFuture listenerFuture = mock(typeof(ChannelFuture));
			when(listenerFuture.addListener(any())).thenReturn(listenerFuture);
			when(ctx.writeAndFlush(any())).thenReturn(listenerFuture);

			System.Reflection.FieldInfo ctxField = typeof(PulsarHandler).getDeclaredField("ctx");
			ctxField.Accessible = true;
			ctxField.set(cnx, ctx);
			try
			{
				cnx.newLookup(null, 123).get();
			}
			catch (Exception e)
			{
				assertTrue(e.InnerException is PulsarClientException.TimeoutException);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testReceiveErrorAtSendConnectFrameState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testReceiveErrorAtSendConnectFrameState()
		{
			ThreadFactory threadFactory = new DefaultThreadFactory("testReceiveErrorAtSendConnectFrameState");
			EventLoopGroup eventLoop = EventLoopUtil.newEventLoopGroup(1, threadFactory);
			ClientConfigurationData conf = new ClientConfigurationData();
			conf.OperationTimeoutMs = 10;
			ClientCnx cnx = new ClientCnx(conf, eventLoop);

			ChannelHandlerContext ctx = mock(typeof(ChannelHandlerContext));
			Channel channel = mock(typeof(Channel));
			when(ctx.channel()).thenReturn(channel);

			System.Reflection.FieldInfo ctxField = typeof(PulsarHandler).getDeclaredField("ctx");
			ctxField.Accessible = true;
			ctxField.set(cnx, ctx);

			// set connection as SentConnectFrame
			System.Reflection.FieldInfo cnxField = typeof(ClientCnx).getDeclaredField("state");
			cnxField.Accessible = true;
			cnxField.set(cnx, ClientCnx.State.SentConnectFrame);

			// receive error
			PulsarApi.CommandError commandError = PulsarApi.CommandError.newBuilder().setRequestId(-1).setError(PulsarApi.ServerError.AuthenticationError).setMessage("authentication was failed").build();
			try
			{
				cnx.handleError(commandError);
			}
			catch (Exception)
			{
				fail("should not throw any error");
			}
		}

	}

}