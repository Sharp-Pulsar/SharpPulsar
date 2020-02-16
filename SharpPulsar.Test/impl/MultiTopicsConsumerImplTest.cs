﻿using System.Threading;

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
	using Sets = com.google.common.collect.Sets;
	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using DefaultThreadFactory = io.netty.util.concurrent.DefaultThreadFactory;
	using ClientConfigurationData = Org.Apache.Pulsar.Client.Impl.Conf.ClientConfigurationData;
	using Org.Apache.Pulsar.Client.Impl.Conf;
	using EventLoopUtil = Org.Apache.Pulsar.Common.Util.Netty.EventLoopUtil;
	using Test = org.testng.annotations.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;

	/// <summary>
	/// Unit Tests of <seealso cref="MultiTopicsConsumerImpl"/>.
	/// </summary>
	public class MultiTopicsConsumerImplTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetStats() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestGetStats()
		{
			string TopicName = "test-stats";
			ClientConfigurationData Conf = new ClientConfigurationData();
			Conf.ServiceUrl = "pulsar://localhost:6650";
			Conf.StatsIntervalSeconds = 100;

			ThreadFactory ThreadFactory = new DefaultThreadFactory("client-test-stats", Thread.CurrentThread.Daemon);
			EventLoopGroup EventLoopGroup = EventLoopUtil.newEventLoopGroup(Conf.NumIoThreads, ThreadFactory);
			ExecutorService ListenerExecutor = Executors.newSingleThreadScheduledExecutor(ThreadFactory);

			PulsarClientImpl ClientImpl = new PulsarClientImpl(Conf, EventLoopGroup);

			ConsumerConfigurationData ConsumerConfData = new ConsumerConfigurationData();
			ConsumerConfData.TopicNames = Sets.newHashSet(TopicName);

			assertEquals(long.Parse("100"), ClientImpl.Configuration.StatsIntervalSeconds);

			MultiTopicsConsumerImpl Impl = new MultiTopicsConsumerImpl(ClientImpl, ConsumerConfData, ListenerExecutor, null, null, null, true);

			Impl.Stats;
		}

	}

}