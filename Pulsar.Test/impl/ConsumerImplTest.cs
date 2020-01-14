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
	using Timer = io.netty.util.Timer;
	using Consumer = org.apache.pulsar.client.api.Consumer;
	using Message = org.apache.pulsar.client.api.Message;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using SubscriptionMode = org.apache.pulsar.client.impl.ConsumerImpl.SubscriptionMode;
	using ClientConfigurationData = org.apache.pulsar.client.impl.conf.ClientConfigurationData;
	using org.apache.pulsar.client.impl.conf;
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
		public virtual void setUp()
		{
			consumerConf = new ConsumerConfigurationData<>();
			ClientConfigurationData clientConf = new ClientConfigurationData();
			PulsarClientImpl client = mock(typeof(PulsarClientImpl));
			CompletableFuture<ClientCnx> clientCnxFuture = new CompletableFuture<ClientCnx>();
			CompletableFuture<Consumer<ConsumerImpl>> subscribeFuture = new CompletableFuture<Consumer<ConsumerImpl>>();
			string topic = "non-persistent://tenant/ns1/my-topic";

			// Mock connection for grabCnx()
			when(client.getConnection(anyString())).thenReturn(clientCnxFuture);
			clientConf.OperationTimeoutMs = 100;
			clientConf.StatsIntervalSeconds = 0;
			when(client.Configuration).thenReturn(clientConf);
			when(client.timer()).thenReturn(mock(typeof(Timer)));

			consumerConf.SubscriptionName = "test-sub";
			consumer = ConsumerImpl.newConsumerImpl(client, topic, consumerConf, executorService, -1, false, subscribeFuture, SubscriptionMode.Durable, null, null, null, true);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(invocationTimeOut = 1000) public void testNotifyPendingReceivedCallback_EmptyQueueNotThrowsException()
		public virtual void testNotifyPendingReceivedCallback_EmptyQueueNotThrowsException()
		{
			consumer.notifyPendingReceivedCallback(null, null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(invocationTimeOut = 500) public void testCorrectBackoffConfiguration()
		public virtual void testCorrectBackoffConfiguration()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Backoff backoff = consumer.getConnectionHandler().backoff;
			Backoff backoff = consumer.ConnectionHandler.backoff;
			ClientConfigurationData clientConfigurationData = new ClientConfigurationData();
			Assert.assertEquals(backoff.Max, TimeUnit.NANOSECONDS.toMillis(clientConfigurationData.MaxBackoffIntervalNanos));
			Assert.assertEquals(backoff.next(), TimeUnit.NANOSECONDS.toMillis(clientConfigurationData.InitialBackoffIntervalNanos));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(invocationTimeOut = 1000) public void testNotifyPendingReceivedCallback_CompleteWithException()
		public virtual void testNotifyPendingReceivedCallback_CompleteWithException()
		{
			CompletableFuture<Message<ConsumerImpl>> receiveFuture = new CompletableFuture<Message<ConsumerImpl>>();
			consumer.pendingReceives.add(receiveFuture);
			Exception exception = new PulsarClientException.InvalidMessageException("some random exception");
			consumer.notifyPendingReceivedCallback(null, exception);

			try
			{
				receiveFuture.join();
			}
			catch (CompletionException e)
			{
				// Completion exception must be the same we provided at calling time
				Assert.assertEquals(e.InnerException, exception);
			}

			Assert.assertTrue(receiveFuture.CompletedExceptionally);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(invocationTimeOut = 1000) public void testNotifyPendingReceivedCallback_CompleteWithExceptionWhenMessageIsNull()
		public virtual void testNotifyPendingReceivedCallback_CompleteWithExceptionWhenMessageIsNull()
		{
			CompletableFuture<Message<ConsumerImpl>> receiveFuture = new CompletableFuture<Message<ConsumerImpl>>();
			consumer.pendingReceives.add(receiveFuture);
			consumer.notifyPendingReceivedCallback(null, null);

			try
			{
				receiveFuture.join();
			}
			catch (CompletionException e)
			{
				Assert.assertEquals("received message can't be null", e.InnerException.Message);
			}

			Assert.assertTrue(receiveFuture.CompletedExceptionally);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(invocationTimeOut = 1000) public void testNotifyPendingReceivedCallback_InterceptorsWorksWithPrefetchDisabled()
		public virtual void testNotifyPendingReceivedCallback_InterceptorsWorksWithPrefetchDisabled()
		{
			CompletableFuture<Message<ConsumerImpl>> receiveFuture = new CompletableFuture<Message<ConsumerImpl>>();
			MessageImpl message = mock(typeof(MessageImpl));
			ConsumerImpl<ConsumerImpl> spy = spy(consumer);

			consumer.pendingReceives.add(receiveFuture);
			consumerConf.ReceiverQueueSize = 0;
			doReturn(message).when(spy).beforeConsume(any());
			spy.notifyPendingReceivedCallback(message, null);
			Message<ConsumerImpl> receivedMessage = receiveFuture.join();

			verify(spy, times(1)).beforeConsume(message);
			Assert.assertTrue(receiveFuture.Done);
			Assert.assertFalse(receiveFuture.CompletedExceptionally);
			Assert.assertEquals(receivedMessage, message);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(invocationTimeOut = 1000) public void testNotifyPendingReceivedCallback_WorkNormally()
		public virtual void testNotifyPendingReceivedCallback_WorkNormally()
		{
			CompletableFuture<Message<ConsumerImpl>> receiveFuture = new CompletableFuture<Message<ConsumerImpl>>();
			MessageImpl message = mock(typeof(MessageImpl));
			ConsumerImpl<ConsumerImpl> spy = spy(consumer);

			consumer.pendingReceives.add(receiveFuture);
			doReturn(message).when(spy).beforeConsume(any());
			doNothing().when(spy).messageProcessed(message);
			spy.notifyPendingReceivedCallback(message, null);
			Message<ConsumerImpl> receivedMessage = receiveFuture.join();

			verify(spy, times(1)).beforeConsume(message);
			verify(spy, times(1)).messageProcessed(message);
			Assert.assertTrue(receiveFuture.Done);
			Assert.assertFalse(receiveFuture.CompletedExceptionally);
			Assert.assertEquals(receivedMessage, message);
		}
	}

}