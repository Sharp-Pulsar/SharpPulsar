

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

using System;
using System.Threading.Tasks;
using BAMCIS.Util.Concurrent;
using DotNetty.Common.Utilities;
using FakeItEasy;
using SharpPulsar.Api;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Utility;
using SharpPulsar.Utils;
using Xunit;

namespace SharpPulsar.Test.Impl
{

	public class ConsumerImplTest
	{


		private readonly ScheduledThreadPoolExecutor _executorService = new ScheduledThreadPoolExecutor(1);
		private ConsumerImpl<sbyte[]> _consumer;
		private ConsumerConfigurationData<sbyte[]> _consumerConf;

		public ConsumerImplTest()
		{
            var clientConf = A.Fake<ClientConfigurationData>(x=>x.ConfigureFake(c=> c.ServiceUrl= "pulsar://localhost:6650"));
			var client = A.Fake<PulsarClientImpl>(x=> x.WithArgumentsForConstructor(()=> new PulsarClientImpl(clientConf)));
			_consumerConf = new ConsumerConfigurationData<sbyte[]>();

			ValueTask<ClientCnx> clientCnxTask = new ValueTask<ClientCnx>();
			TaskCompletionSource<IConsumer<sbyte[]>> subscribeFuture = new TaskCompletionSource<IConsumer<sbyte[]>>();
			string topic = "non-persistent://tenant/ns1/my-topic";

			// Mock connection for grabCnx()
			A.CallTo(() => client.GetConnection(A<string>._)).Returns(clientCnxTask);
			clientConf.OperationTimeoutMs = 100;
			clientConf.StatsIntervalSeconds = 0;
            A.CallTo(() => client.Configuration).Returns(clientConf);
            A.CallToSet(()=> client.Timer).To(new HashedWheelTimer());

			_consumerConf.SubscriptionName = "test-sub";
			_consumer = ConsumerImpl<sbyte[]>.NewConsumerImpl(client, topic, _consumerConf, _executorService, -1, false, subscribeFuture, ConsumerImpl<sbyte[]>.SubscriptionMode.Durable, null, null, null, true);
		}

		[Fact]
		public void TestNotifyPendingReceivedCallbackEmptyQueueNotThrowsException()
		{
			_consumer.NotifyPendingReceivedCallback(null, null);
		}

		[Fact]
		public void TestCorrectBackoffConfiguration()
		{
			Backoff backoff = _consumer.Handler.Backoff;
			ClientConfigurationData clientConfigurationData = new ClientConfigurationData();
			Assert.Equal(BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS.ToMillis(clientConfigurationData.MaxBackoffIntervalNanos), backoff.Max);
			Assert.Equal(BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS.ToMillis(clientConfigurationData.InitialBackoffIntervalNanos), backoff.Next());
		}

		[Fact]
		public void TestNotifyPendingReceivedCallbackCompleteWithException()
		{
			var receiveTask = new TaskCompletionSource<IMessage<sbyte[]>>();
			_consumer.PendingReceives.Enqueue(receiveTask);
			System.Exception exception = new PulsarClientException.InvalidMessageException("some random exception");
			_consumer.NotifyPendingReceivedCallback(null, exception);

			try
			{
				receiveTask.Task.Wait();
			}
			catch (Exception e)
			{
				// Completion exception must be the same we provided at calling time
				Assert.Equal(exception, e.InnerException);
			}

			Assert.True(receiveTask.Task.IsFaulted);
		}

		[Fact]
		public void TestNotifyPendingReceivedCallbackCompleteWithExceptionWhenMessageIsNull()
		{
            var receiveTask = new TaskCompletionSource<IMessage<sbyte[]>>();
			_consumer.NotifyPendingReceivedCallback(null, null);

			try
			{
				receiveTask.Task.Wait();
			}
			catch (Exception e)
			{
				Assert.Equal("received message can't be null", e.InnerException?.Message);
			}

			Assert.True(receiveTask.Task.IsFaulted);
		}
		[Fact]
		public void TestNotifyPendingReceivedCallbackInterceptorsWorksWithPrefetchDisabled()
		{
            var receiveTask = new TaskCompletionSource<IMessage<sbyte[]>>();
			var message = A.Fake<MessageImpl<sbyte[]>>(); 

			_consumer.PendingReceives.Enqueue(receiveTask);

			_consumerConf.ReceiverQueueSize = 0;
            A.CallTo(() => _consumer.BeforeConsume(A<IMessage<sbyte[]>>._)).Returns(message);
			_consumer.NotifyPendingReceivedCallback(message, null);
			var receivedMessage = receiveTask.Task.Result;

			A.CallTo(()=> _consumer.BeforeConsume(message)).MustHaveHappened(1, Times.Exactly);
			Assert.True(receiveTask.Task.IsCompleted);
			Assert.False(receiveTask.Task.IsFaulted);
			Assert.Equal(message,receivedMessage);
		}

		[Fact]
		public  void TestNotifyPendingReceivedCallbackWorkNormally()
		{
            var receiveTask = new TaskCompletionSource<IMessage<sbyte[]>>();
            var message = A.Fake<MessageImpl<sbyte[]>>();

            _consumer.PendingReceives.Enqueue(receiveTask);
			A.CallTo(()=> _consumer.BeforeConsume(A<IMessage<sbyte[]>>._)).Returns(message);
            A.CallTo(() => _consumer.MessageProcessed(message)).DoesNothing();

            _consumer.NotifyPendingReceivedCallback(message, null);
            var receivedMessage = receiveTask.Task.Result;

            A.CallTo(() => _consumer.BeforeConsume(message)).MustHaveHappened(1, Times.Exactly);
			A.CallTo(() => _consumer.MessageProcessed(message)).MustHaveHappened(1, Times.Exactly);

			Assert.True(receiveTask.Task.IsCompleted);
			Assert.False(receiveTask.Task.IsFaulted);
			Assert.Equal(message, receivedMessage);
		}
	}

}