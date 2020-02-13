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
	using Timer = io.netty.util.Timer;
	using Consumer = Org.Apache.Pulsar.Client.Api.Consumer;
	using Org.Apache.Pulsar.Client.Api;
	using PulsarClientException = Org.Apache.Pulsar.Client.Api.PulsarClientException;
	using SubscriptionMode = Org.Apache.Pulsar.Client.Impl.ConsumerImpl.SubscriptionMode;
	using ClientConfigurationData = Org.Apache.Pulsar.Client.Impl.Conf.ClientConfigurationData;
	using Org.Apache.Pulsar.Client.Impl.Conf;
	using Assert = org.testng.Assert;
	using BeforeMethod = org.testng.annotations.BeforeMethod;
	using Test = org.testng.annotations.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.*;

	public class ConsumerImplTest
	{


		private readonly ExecutorService executorService = Executors.newSingleThreadExecutor();
		private ConsumerImpl<ConsumerImpl> consumer;
		private ConsumerConfigurationData consumerConf;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void setUp()
		public virtual void SetUp()
		{
			consumerConf = new ConsumerConfigurationData<>();
			ClientConfigurationData ClientConf = new ClientConfigurationData();
			PulsarClientImpl Client = mock(typeof(PulsarClientImpl));
			CompletableFuture<ClientCnx> ClientCnxFuture = new CompletableFuture<ClientCnx>();
			CompletableFuture<Consumer<ConsumerImpl>> SubscribeFuture = new CompletableFuture<Consumer<ConsumerImpl>>();
			string Topic = "non-persistent://tenant/ns1/my-topic";

			// Mock connection for grabCnx()
			when(Client.getConnection(anyString())).thenReturn(ClientCnxFuture);
			ClientConf.OperationTimeoutMs = 100;
			ClientConf.StatsIntervalSeconds = 0;
			when(Client.Configuration).thenReturn(ClientConf);
			when(Client.timer()).thenReturn(mock(typeof(Timer)));

			consumerConf.SubscriptionName = "test-sub";
			consumer = ConsumerImpl.NewConsumerImpl(Client, Topic, consumerConf, executorService, -1, false, SubscribeFuture, SubscriptionMode.Durable, null, null, null, true);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(invocationTimeOut = 1000) public void testNotifyPendingReceivedCallback_EmptyQueueNotThrowsException()
		public virtual void TestNotifyPendingReceivedCallbackEmptyQueueNotThrowsException()
		{
			consumer.NotifyPendingReceivedCallback(null, null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(invocationTimeOut = 500) public void testCorrectBackoffConfiguration()
		public virtual void TestCorrectBackoffConfiguration()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Backoff backoff = consumer.getConnectionHandler().backoff;
			Backoff Backoff = consumer.ConnectionHandler.backoff;
			ClientConfigurationData ClientConfigurationData = new ClientConfigurationData();
			Assert.assertEquals(Backoff.Max, TimeUnit.NANOSECONDS.toMillis(ClientConfigurationData.MaxBackoffIntervalNanos));
			Assert.assertEquals(Backoff.next(), TimeUnit.NANOSECONDS.toMillis(ClientConfigurationData.InitialBackoffIntervalNanos));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(invocationTimeOut = 1000) public void testNotifyPendingReceivedCallback_CompleteWithException()
		public virtual void TestNotifyPendingReceivedCallbackCompleteWithException()
		{
			CompletableFuture<Message<ConsumerImpl>> ReceiveFuture = new CompletableFuture<Message<ConsumerImpl>>();
			consumer.PendingReceives.add(ReceiveFuture);
			Exception Exception = new PulsarClientException.InvalidMessageException("some random exception");
			consumer.NotifyPendingReceivedCallback(null, Exception);

			try
			{
				ReceiveFuture.join();
			}
			catch (CompletionException E)
			{
				// Completion exception must be the same we provided at calling time
				Assert.assertEquals(E.InnerException, Exception);
			}

			Assert.assertTrue(ReceiveFuture.CompletedExceptionally);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(invocationTimeOut = 1000) public void testNotifyPendingReceivedCallback_CompleteWithExceptionWhenMessageIsNull()
		public virtual void TestNotifyPendingReceivedCallbackCompleteWithExceptionWhenMessageIsNull()
		{
			CompletableFuture<Message<ConsumerImpl>> ReceiveFuture = new CompletableFuture<Message<ConsumerImpl>>();
			consumer.PendingReceives.add(ReceiveFuture);
			consumer.NotifyPendingReceivedCallback(null, null);

			try
			{
				ReceiveFuture.join();
			}
			catch (CompletionException E)
			{
				Assert.assertEquals("received message can't be null", E.InnerException.Message);
			}

			Assert.assertTrue(ReceiveFuture.CompletedExceptionally);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(invocationTimeOut = 1000) public void testNotifyPendingReceivedCallback_InterceptorsWorksWithPrefetchDisabled()
		public virtual void TestNotifyPendingReceivedCallbackInterceptorsWorksWithPrefetchDisabled()
		{
			CompletableFuture<Message<ConsumerImpl>> ReceiveFuture = new CompletableFuture<Message<ConsumerImpl>>();
			MessageImpl Message = mock(typeof(MessageImpl));
			ConsumerImpl<ConsumerImpl> Spy = spy(consumer);

			consumer.PendingReceives.add(ReceiveFuture);
			consumerConf.ReceiverQueueSize = 0;
			doReturn(Message).when(Spy).beforeConsume(any());
			Spy.notifyPendingReceivedCallback(Message, null);
			Message<ConsumerImpl> ReceivedMessage = ReceiveFuture.join();

			verify(Spy, times(1)).beforeConsume(Message);
			Assert.assertTrue(ReceiveFuture.Done);
			Assert.assertFalse(ReceiveFuture.CompletedExceptionally);
			Assert.assertEquals(ReceivedMessage, Message);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(invocationTimeOut = 1000) public void testNotifyPendingReceivedCallback_WorkNormally()
		public virtual void TestNotifyPendingReceivedCallbackWorkNormally()
		{
			CompletableFuture<Message<ConsumerImpl>> ReceiveFuture = new CompletableFuture<Message<ConsumerImpl>>();
			MessageImpl Message = mock(typeof(MessageImpl));
			ConsumerImpl<ConsumerImpl> Spy = spy(consumer);

			consumer.PendingReceives.add(ReceiveFuture);
			doReturn(Message).when(Spy).beforeConsume(any());
			doNothing().when(Spy).messageProcessed(Message);
			Spy.notifyPendingReceivedCallback(Message, null);
			Message<ConsumerImpl> ReceivedMessage = ReceiveFuture.join();

			verify(Spy, times(1)).beforeConsume(Message);
			verify(Spy, times(1)).messageProcessed(Message);
			Assert.assertTrue(ReceiveFuture.Done);
			Assert.assertFalse(ReceiveFuture.CompletedExceptionally);
			Assert.assertEquals(ReceivedMessage, Message);
		}
	}

}