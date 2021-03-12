using System;
using System.Threading;

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
	using Org.Apache.Pulsar.Client.Api;
	using Org.Apache.Pulsar.Client.Api;
	using PulsarClientException = Org.Apache.Pulsar.Client.Api.PulsarClientException;
	using Org.Apache.Pulsar.Client.Api;
	using ClientConfigurationData = Org.Apache.Pulsar.Client.Impl.Conf.ClientConfigurationData;
	using Org.Apache.Pulsar.Client.Impl.Conf;
	using PartitionedTopicMetadata = Org.Apache.Pulsar.Common.Partition.PartitionedTopicMetadata;
	using EventLoopUtil = Org.Apache.Pulsar.Common.Util.Netty.EventLoopUtil;
	using Test = org.testng.annotations.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.impl.ClientTestFixtures.createDelayedCompletedFuture;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.impl.ClientTestFixtures.createPulsarClientMockWithMockedClientCnx;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.*;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.expectThrows;

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
			string topicName = "test-stats";
			ClientConfigurationData conf = new ClientConfigurationData();
			conf.ServiceUrl = "pulsar://localhost:6650";
			conf.StatsIntervalSeconds = 100;

			ThreadFactory threadFactory = new DefaultThreadFactory("client-test-stats", Thread.CurrentThread.Daemon);
			EventLoopGroup eventLoopGroup = EventLoopUtil.NewEventLoopGroup(conf.NumIoThreads, threadFactory);
			ExecutorService listenerExecutor = Executors.newSingleThreadScheduledExecutor(threadFactory);

			PulsarClientImpl clientImpl = new PulsarClientImpl(conf, eventLoopGroup);

			ConsumerConfigurationData consumerConfData = new ConsumerConfigurationData();
			consumerConfData.TopicNames = Sets.newHashSet(topicName);

			assertEquals(long.Parse("100"), clientImpl.Configuration.StatsIntervalSeconds);

			MultiTopicsConsumerImpl impl = new MultiTopicsConsumerImpl(clientImpl, consumerConfData, listenerExecutor, null, null, null, true);

			impl.Stats;
		}

		// Test uses a mocked PulsarClientImpl which will complete the getPartitionedTopicMetadata() internal async call
		// after a delay longer than the interval between the two subscribeAsync() calls in the test method body.
		//
		// Code under tests is using CompletableFutures. Theses may hang indefinitely if code is broken.
		// That's why a test timeout is defined.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeOut = 5000) public void testParallelSubscribeAsync() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestParallelSubscribeAsync()
		{
			string topicName = "parallel-subscribe-async-topic";
			MultiTopicsConsumerImpl<sbyte[]> impl = CreateMultiTopicsConsumer();

			CompletableFuture<Void> firstInvocation = impl.SubscribeAsync(topicName, true);
			Thread.Sleep(5); // less than completionDelayMillis
			CompletableFuture<Void> secondInvocation = impl.SubscribeAsync(topicName, true);

			firstInvocation.get(); // does not throw
			Exception t = expectThrows(typeof(ExecutionException), secondInvocation.get);
			Exception cause = t.InnerException;
			assertEquals(cause.GetType(), typeof(PulsarClientException));
			assertTrue(cause.Message.EndsWith("Topic is already being subscribed for in other thread."));
		}

		private MultiTopicsConsumerImpl<sbyte[]> CreateMultiTopicsConsumer()
		{
			ExecutorService listenerExecutor = mock(typeof(ExecutorService));
			ConsumerConfigurationData<sbyte[]> consumerConfData = new ConsumerConfigurationData<sbyte[]>();
			consumerConfData.SubscriptionName = "subscriptionName";
			int completionDelayMillis = 100;
			Schema<sbyte[]> schema = Schema.BYTES;
			PulsarClientImpl clientMock = createPulsarClientMockWithMockedClientCnx();
			when(clientMock.GetPartitionedTopicMetadata(any())).thenAnswer(invocation => createDelayedCompletedFuture(new PartitionedTopicMetadata(), completionDelayMillis));
			when(clientMock.PreProcessSchemaBeforeSubscribe<sbyte[]>(any(), any(), any())).thenReturn(CompletableFuture.completedFuture(schema));
			MultiTopicsConsumerImpl<sbyte[]> impl = new MultiTopicsConsumerImpl<sbyte[]>(clientMock, consumerConfData, listenerExecutor, new CompletableFuture<sbyte[]>(), schema, null, true);
			return impl;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testReceiveAsyncCanBeCancelled()
		public virtual void TestReceiveAsyncCanBeCancelled()
		{
			// given
			MultiTopicsConsumerImpl<sbyte[]> consumer = CreateMultiTopicsConsumer();
			CompletableFuture<Message<sbyte[]>> future = consumer.ReceiveAsync();
			assertEquals(consumer.PeekPendingReceive(), future);
			// when
			future.cancel(true);
			// then
			assertTrue(consumer.PendingReceives.Empty);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBatchReceiveAsyncCanBeCancelled()
		public virtual void TestBatchReceiveAsyncCanBeCancelled()
		{
			// given
			MultiTopicsConsumerImpl<sbyte[]> consumer = CreateMultiTopicsConsumer();
			CompletableFuture<Messages<sbyte[]>> future = consumer.BatchReceiveAsync();
			assertTrue(consumer.HasPendingBatchReceive());
			// when
			future.cancel(true);
			// then
			assertFalse(consumer.HasPendingBatchReceive());
		}

	}

}