using System;
using System.Collections.Generic;
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
namespace SharpPulsar.Test
{
	/// <summary>
	/// End to end transaction test.
	/// </summary>
	public class TransactionEndToEndTest
	{

		private const int TopicPartition = 3;

		private const string TENANT = "tnx";
		private static readonly string _nAMESPACE1 = TENANT + "/ns1";
		private static readonly string _topicOutput = _nAMESPACE1 + "/output";
		private static readonly string _topicMessageAckTest = _nAMESPACE1 + "/message-ack-test";
		protected internal virtual void Setup()
		{

			string[] brokerServiceUrlArr = PulsarServiceList.get(0).BrokerServiceUrl.Split(":");
			string webServicePort = brokerServiceUrlArr[brokerServiceUrlArr.Length - 1];
			Admin.Clusters().CreateCluster(ClusterName, new ClusterData("http://localhost:" + webServicePort));
			Admin.Tenants().CreateTenant(TENANT, new TenantInfo(Sets.newHashSet("appid1"), Sets.newHashSet(ClusterName)));
			Admin.Namespaces().CreateNamespace(_nAMESPACE1);
			Admin.Topics().CreatePartitionedTopic(_topicOutput, TopicPartition);
			Admin.Topics().CreatePartitionedTopic(_topicMessageAckTest, TopicPartition);

			Admin.Tenants().CreateTenant(NamespaceName.SystemNamespace.Tenant, new TenantInfo(Sets.newHashSet("appid1"), Sets.newHashSet(ClusterName)));
			Admin.Namespaces().CreateNamespace(NamespaceName.SystemNamespace.ToString());
			Admin.Topics().CreatePartitionedTopic(TopicName.TransactionCoordinatorAssign.ToString(), 16);

			PulsarClient = PulsarClient.builder().serviceUrl(PulsarServiceList.get(0).BrokerServiceUrl).statsInterval(0, TimeUnit.SECONDS).enableTransaction(true).build();

			Thread.Sleep(1000 * 3);
		}

		protected internal virtual void Cleanup()
		{
			base.InternalCleanup();
		}

		public virtual void NoBatchProduceCommitTest()
		{
			ProduceCommitTest(false);
		}

		public virtual void BatchProduceCommitTest()
		{
			ProduceCommitTest(true);
		}

		private void ProduceCommitTest(bool enableBatch)
		{
			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic(_topicOutput).SubscriptionName("test").EnableBatchIndexAcknowledgment(true).Subscribe();

			ProducerBuilder<sbyte[]> producerBuilder = PulsarClient.NewProducer().Topic(_topicOutput).EnableBatching(enableBatch).SendTimeout(0, TimeUnit.SECONDS);

			Producer<sbyte[]> producer = producerBuilder.Create();

			Transaction txn1 = Txn;
			Transaction txn2 = Txn;

			int txn1MessageCnt = 0;
			int txn2MessageCnt = 0;
			int messageCnt = 1000;
			for(int i = 0; i < messageCnt; i++)
			{
				if(i % 5 == 0)
				{
					producer.newMessage(txn1).value(("Hello Txn - " + i).GetBytes(UTF_8)).send();
					txn1MessageCnt++;
				}
				else
				{
					producer.newMessage(txn2).value(("Hello Txn - " + i).GetBytes(UTF_8)).sendAsync();
					txn2MessageCnt++;
				}
			}

			// Can't receive transaction messages before commit.
			Message<sbyte[]> message = consumer.receive(5, TimeUnit.SECONDS);
			Assert.assertNull(message);

			txn1.Commit().get();

			// txn1 messages could be received after txn1 committed
			int receiveCnt = 0;
			for(int i = 0; i < txn1MessageCnt; i++)
			{
				message = consumer.receive();
				Assert.assertNotNull(message);
				receiveCnt++;
			}
			Assert.assertEquals(txn1MessageCnt, receiveCnt);

			message = consumer.receive(5, TimeUnit.SECONDS);
			Assert.assertNull(message);

			txn2.Commit().get();

			// txn2 messages could be received after txn2 committed
			receiveCnt = 0;
			for(int i = 0; i < txn2MessageCnt; i++)
			{
				message = consumer.receive();
				Assert.assertNotNull(message);
				receiveCnt++;
			}
			Assert.assertEquals(txn2MessageCnt, receiveCnt);

			message = consumer.receive(5, TimeUnit.SECONDS);
			Assert.assertNull(message);

			log.info("message commit test enableBatch {}", enableBatch);
		}

		public virtual void ProduceAbortTest()
		{
			Transaction txn = Txn;


			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(_topicOutput).SendTimeout(0, TimeUnit.SECONDS).EnableBatching(false).Create();

			int messageCnt = 10;
			for(int i = 0; i < messageCnt; i++)
			{
				producer.newMessage(txn).value(("Hello Txn - " + i).GetBytes(UTF_8)).sendAsync();
			}

			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic(_topicOutput).SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest).SubscriptionName("test").EnableBatchIndexAcknowledgment(true).Subscribe();

			// Can't receive transaction messages before abort.
			Message<sbyte[]> message = consumer.receive(5, TimeUnit.SECONDS);
			Assert.assertNull(message);

			txn.Abort().get();

			// Cant't receive transaction messages after abort.
			message = consumer.receive(5, TimeUnit.SECONDS);
			Assert.assertNull(message);
		}

		public virtual void TxnIndividualAckTestNoBatchAndSharedSub()
		{
			TxnAckTest(false, 1, SubscriptionType.Shared);
		}

		public virtual void TxnIndividualAckTestBatchAndSharedSub()
		{
			TxnAckTest(true, 200, SubscriptionType.Shared);
		}

		public virtual void TxnIndividualAckTestNoBatchAndFailoverSub()
		{
			TxnAckTest(false, 1, SubscriptionType.Failover);
		}

		public virtual void TxnIndividualAckTestBatchAndFailoverSub()
		{
			TxnAckTest(true, 200, SubscriptionType.Failover);
		}

		private void TxnAckTest(bool batchEnable, int maxBatchSize, SubscriptionType subscriptionType)
		{
			string normalTopic = _nAMESPACE1 + "/normal-topic";

			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic(normalTopic).SubscriptionName("test").EnableBatchIndexAcknowledgment(true).SubscriptionType(subscriptionType).Subscribe();

			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(normalTopic).EnableBatching(batchEnable).BatchingMaxMessages(maxBatchSize).Create();

			for(int retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				Transaction txn = Txn;

				int messageCnt = 1000;
				// produce normal messages
				for(int i = 0; i < messageCnt; i++)
				{
					producer.newMessage().value("hello".GetBytes()).sendAsync();
				}

				// consume and ack messages with txn
				for(int i = 0; i < messageCnt; i++)
				{
					Message<sbyte[]> message = consumer.receive();
					Assert.assertNotNull(message);
					log.info("receive msgId: {}, count : {}", message.MessageId, i);
					consumer.acknowledgeAsync(message.MessageId, txn).get();
				}

				// the messages are pending ack state and can't be received
				Message<sbyte[]> message = consumer.receive(2, TimeUnit.SECONDS);
				Assert.assertNull(message);

				// 1) txn abort
				txn.Abort().get();

				// after transaction abort, the messages could be received
				Transaction commitTxn = Txn;
				for(int i = 0; i < messageCnt; i++)
				{
					message = consumer.receive(2, TimeUnit.SECONDS);
					Assert.assertNotNull(message);
					consumer.acknowledgeAsync(message.MessageId, commitTxn).get();
					log.info("receive msgId: {}, count: {}", message.MessageId, i);
				}

				// 2) ack committed by a new txn
				commitTxn.Commit().get();

				// after transaction commit, the messages can't be received
				message = consumer.receive(2, TimeUnit.SECONDS);
				Assert.assertNull(message);

				try
				{
					commitTxn.Commit().get();
					fail("recommit one transaction should be failed.");
				}
				catch(Exception reCommitError)
				{
					// recommit one transaction should be failed
					log.info("expected exception for recommit one transaction.");
					Assert.assertNotNull(reCommitError);
					Assert.assertTrue(reCommitError.InnerException is TransactionCoordinatorClientException.InvalidTxnStatusException);
				}
			}
		}

		public virtual void TxnMessageAckTest()
		{
			string topic = _topicMessageAckTest;
			const string subName = "test";
			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic(topic).SubscriptionName(subName).EnableBatchIndexAcknowledgment(true).AcknowledgmentGroupTime(0, TimeUnit.MILLISECONDS).Subscribe();

			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(topic).SendTimeout(0, TimeUnit.SECONDS).EnableBatching(false).Create();

			Transaction txn = Txn;

			int messageCnt = 10;
			for(int i = 0; i < messageCnt; i++)
			{
				producer.newMessage(txn).value(("Hello Txn - " + i).GetBytes(UTF_8)).sendAsync();
			}
			log.info("produce transaction messages finished");

			// Can't receive transaction messages before commit.
			Message<sbyte[]> message = consumer.receive(5, TimeUnit.SECONDS);
			Assert.assertNull(message);
			log.info("transaction messages can't be received before transaction committed");

			txn.Commit().get();

			int ackedMessageCount = 0;
			int receiveCnt = 0;
			for(int i = 0; i < messageCnt; i++)
			{
				message = consumer.receive();
				Assert.assertNotNull(message);
				receiveCnt++;
				if(i % 2 == 0)
				{
					consumer.acknowledge(message);
					ackedMessageCount++;
				}
			}
			Assert.assertEquals(messageCnt, receiveCnt);

			message = consumer.receive(5, TimeUnit.SECONDS);
			Assert.assertNull(message);

			MarkDeletePositionCheck(topic, subName, false);

			consumer.redeliverUnacknowledgedMessages();

			receiveCnt = 0;
			for(int i = 0; i < messageCnt - ackedMessageCount; i++)
			{
				message = consumer.receive(2, TimeUnit.SECONDS);
				Assert.assertNotNull(message);
				consumer.acknowledge(message);
				receiveCnt++;
			}
			Assert.assertEquals(messageCnt - ackedMessageCount, receiveCnt);

			message = consumer.receive(2, TimeUnit.SECONDS);
			Assert.assertNull(message);
			for(int partition = 0; partition < TopicPartition; partition++)
			{
				topic = TopicName.Get(topic).getPartition(partition).ToString();
				bool exist = false;
				for(int i = 0; i < PulsarServiceList.size(); i++)
				{

					System.Reflection.FieldInfo field = typeof(BrokerService).getDeclaredField("topics");
					field.Accessible = true;
					ConcurrentOpenHashMap<string, CompletableFuture<Optional<Topic>>> topics = (ConcurrentOpenHashMap<string, CompletableFuture<Optional<Topic>>>) field.get(PulsarServiceList.get(i).BrokerService);
					CompletableFuture<Optional<Topic>> topicFuture = topics.Get(topic);

					if(topicFuture != null)
					{
						Optional<Topic> topicOptional = topicFuture.get();
						if(topicOptional.Present)
						{
							PersistentSubscription persistentSubscription = (PersistentSubscription) topicOptional.get().getSubscription(subName);
							Position markDeletePosition = persistentSubscription.Cursor.MarkDeletedPosition;
							Position lastConfirmedEntry = persistentSubscription.Cursor.ManagedLedger.LastConfirmedEntry;
							exist = true;
							if(!markDeletePosition.Equals(lastConfirmedEntry))
							{
								//this because of the transaction commit marker have't delete
								//delete commit marker after ack position
								//when delete commit marker operation is processing, next delete operation will not do again
								//when delete commit marker operation finish, it can run next delete commit marker operation
								//so this test may not delete all the position in this manageLedger.
								Position markerPosition = ((ManagedLedgerImpl) persistentSubscription.Cursor.ManagedLedger).getNextValidPosition((PositionImpl) markDeletePosition);
								//marker is the lastConfirmedEntry, after commit the marker will only be write in
								if(!markerPosition.Equals(lastConfirmedEntry))
								{
									log.error("Mark delete position is not commit marker position!");
									fail();
								}
							}
						}
					}
				}
				assertTrue(exist);
			}

			log.info("receive transaction messages count: {}", receiveCnt);
		}

		public virtual void TxnAckTestBatchAndCumulativeSub()
		{
			TxnCumulativeAckTest(true, 200, SubscriptionType.Failover);
		}

		public virtual void TxnAckTestNoBatchAndCumulativeSub()
		{
			TxnCumulativeAckTest(false, 1, SubscriptionType.Failover);
		}

		public virtual void TxnCumulativeAckTest(bool batchEnable, int maxBatchSize, SubscriptionType subscriptionType)
		{
			string normalTopic = _nAMESPACE1 + "/normal-topic";

			Consumer<sbyte[]> consumer = PulsarClient.NewConsumer().Topic(normalTopic).SubscriptionName("test").EnableBatchIndexAcknowledgment(true).SubscriptionType(subscriptionType).AckTimeout(1, TimeUnit.MINUTES).Subscribe();

			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(normalTopic).EnableBatching(batchEnable).BatchingMaxMessages(maxBatchSize).BatchingMaxPublishDelay(1, TimeUnit.SECONDS).Create();

			for(int retryCnt = 0; retryCnt < 2; retryCnt++)
			{
				Transaction abortTxn = Txn;
				int messageCnt = 1000;
				// produce normal messages
				for(int i = 0; i < messageCnt; i++)
				{
					producer.newMessage().value("hello".GetBytes()).sendAsync();
				}
				Message<sbyte[]> message = null;
				Thread.Sleep(1000L);
				for(int i = 0; i < messageCnt; i++)
				{
					message = consumer.receive(1, TimeUnit.SECONDS);
					Assert.assertNotNull(message);
					if(i % 3 == 0)
					{
						consumer.acknowledgeCumulativeAsync(message.MessageId, abortTxn).get();
					}
					log.info("receive msgId abort: {}, retryCount : {}, count : {}", message.MessageId, retryCnt, i);
				}
				try
				{
					consumer.acknowledgeCumulativeAsync(message.MessageId, abortTxn).get();
					fail("not ack conflict ");
				}
				catch(Exception e)
				{
					Assert.assertTrue(e.InnerException is PulsarClientException.TransactionConflictException);
				}

				try
				{
					consumer.acknowledgeCumulativeAsync(DefaultImplementation.NewMessageId(((MessageIdImpl) message.MessageId).LedgerId, ((MessageIdImpl) message.MessageId).EntryId - 1, -1), abortTxn).get();
					fail("not ack conflict ");
				}
				catch(Exception e)
				{
					Assert.assertTrue(e.InnerException is PulsarClientException.TransactionConflictException);
				}

				// the messages are pending ack state and can't be received
				message = consumer.receive(2, TimeUnit.SECONDS);
				Assert.assertNull(message);

				abortTxn.Abort().get();
				Transaction commitTxn = Txn;
				for(int i = 0; i < messageCnt; i++)
				{
					message = consumer.receive(1, TimeUnit.SECONDS);
					Assert.assertNotNull(message);
					if(i % 3 == 0)
					{
						consumer.acknowledgeCumulativeAsync(message.MessageId, commitTxn).get();
					}
					log.info("receive msgId abort: {}, retryCount : {}, count : {}", message.MessageId, retryCnt, i);
				}

				commitTxn.Commit().get();
				try
				{
					commitTxn.Commit().get();
					fail("recommit one transaction should be failed.");
				}
				catch(Exception reCommitError)
				{
					// recommit one transaction should be failed
					log.info("expected exception for recommit one transaction.");
					Assert.assertNotNull(reCommitError);
					Assert.assertTrue(reCommitError.InnerException is TransactionCoordinatorClientException.InvalidTxnStatusException);
				}

				message = consumer.receive(1, TimeUnit.SECONDS);
				Assert.assertNull(message);
			}
		}

		private Transaction Txn
		{
			get
			{
				return PulsarClient.NewTransaction().WithTransactionTimeout(2, TimeUnit.SECONDS).Build().get();
			}
		}

		public virtual void TxnMetadataHandlerRecoverTest()
		{
			string topic = _nAMESPACE1 + "/tc-metadata-handler-recover";
			Producer<sbyte[]> producer = PulsarClient.NewProducer().Topic(topic).SendTimeout(0, TimeUnit.SECONDS).Create();

			IDictionary<TxnID, IList<MessageId>> txnIDListMap = new Dictionary<TxnID, IList<MessageId>>();

			int txnCnt = 20;
			int messageCnt = 10;
			for(int i = 0; i < txnCnt; i++)
			{
				TransactionImpl txn = (TransactionImpl) PulsarClient.NewTransaction().WithTransactionTimeout(5, TimeUnit.MINUTES).Build().get();
				IList<MessageId> messageIds = new List<MessageId>();
				for(int j = 0; j < messageCnt; j++)
				{
					MessageId messageId = producer.NewMessage(txn).Value("Hello".GetBytes()).SendAsync().get();
					messageIds.Add(messageId);
				}
				txnIDListMap[new TxnID(txn.TxnIdMostBits, txn.TxnIdLeastBits)] = messageIds;
			}

			PulsarClient.Dispose();
			PulsarClientImpl recoverPulsarClient = (PulsarClientImpl) PulsarClient.builder().serviceUrl(PulsarServiceList.get(0).BrokerServiceUrl).statsInterval(0, TimeUnit.SECONDS).enableTransaction(true).build();

			TransactionCoordinatorClient tcClient = recoverPulsarClient.TcClient;
			foreach(KeyValuePair<TxnID, IList<MessageId>> entry in txnIDListMap.SetOfKeyValuePairs())
			{
				tcClient.Commit(entry.Key, entry.Value);
			}

			Consumer<sbyte[]> consumer = recoverPulsarClient.NewConsumer().Topic(topic).SubscriptionName("test").SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest).Subscribe();

			for(int i = 0; i < txnCnt * messageCnt; i++)
			{
				Message<sbyte[]> message = consumer.Receive();
				Assert.assertNotNull(message);
			}
		}

	}

}