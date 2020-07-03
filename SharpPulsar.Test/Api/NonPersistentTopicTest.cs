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
namespace SharpPulsar.Test.Api
{
public class NonPersistentTopicTest : ProducerConsumerBase
	{
		private static readonly Logger log = LoggerFactory.getLogger(typeof(NonPersistentTopicTest));
		private readonly new string configClusterName = "r1";
		public virtual object[][] SubscriptionType
		{
			get
			{
				return new object[][]
				{
					new object[] {SubscriptionType.Shared},
					new object[] {SubscriptionType.Exclusive}
				};
			}
		}

		public virtual object[][] LoadManager
		{
			get
			{
	
                return new object[][]
				{
					new object[] {typeof(SimpleLoadManagerImpl).FullName},
					new object[] {typeof(ModularLoadManagerImpl).FullName}
				};
			}
		}


		public override void setup()
		{
			base.internalSetup();
			base.producerBaseSetup();
		}


		public override void cleanup()
		{
			base.internalCleanup();
		}


		public virtual void testNonPersistentTopic(SubscriptionType type)
		{
			log.info("-- Starting {} test --", methodName);

			const string topic = "non-persistent://my-property/my-ns/unacked-topic";
			ConsumerImpl<sbyte[]> consumer = (ConsumerImpl<sbyte[]>) pulsarClient.newConsumer().topic(topic).subscriptionName("subscriber-1").subscriptionType(type).subscribe();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topic).create();

			int totalProduceMsg = 500;
			for (int i = 0; i < totalProduceMsg; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
				Thread.Sleep(10);
			}


			Message<object> msg = null;
			ISet<string> messageSet = Sets.newHashSet();
			for (int i = 0; i < totalProduceMsg; i++)
			{
				msg = consumer.receive(1, TimeUnit.SECONDS);
				if (msg != null)
				{
					consumer.acknowledge(msg);
					string receivedMessage = new string(msg.Data);
					log.debug("Received message: [{}]", receivedMessage);
					string expectedMessage = "my-message-" + i;
					testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
				}
				else
				{
					break;
				}
			}
			assertEquals(messageSet.Count, totalProduceMsg);

			producer.close();
			consumer.close();
			log.info("-- Exiting {} test --", methodName);

		}


		public virtual void testPartitionedNonPersistentTopic(SubscriptionType type)
		{
			log.info("-- Starting {} test --", methodName);

			const string topic = "non-persistent://my-property/my-ns/partitioned-topic";
			admin.topics().createPartitionedTopic(topic, 5);
			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topic).subscriptionName("subscriber-1").subscriptionType(type).subscribe();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topic).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();

			int totalProduceMsg = 500;
			for (int i = 0; i < totalProduceMsg; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
				Thread.Sleep(10);
			}


			Message<object> msg = null;
			ISet<string> messageSet = Sets.newHashSet();
			for (int i = 0; i < totalProduceMsg; i++)
			{
				msg = consumer.receive(1, TimeUnit.SECONDS);
				if (msg != null)
				{
					consumer.acknowledge(msg);
					string receivedMessage = new string(msg.Data);
					log.debug("Received message: [{}]", receivedMessage);
					string expectedMessage = "my-message-" + i;
					testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
				}
				else
				{
					break;
				}
			}
			assertEquals(messageSet.Count, totalProduceMsg);

			producer.close();
			consumer.close();
			log.info("-- Exiting {} test --", methodName);

		}


		public virtual void testPartitionedNonPersistentTopicWithTcpLookup(SubscriptionType type)
		{
			log.info("-- Starting {} test --", methodName);

			const int numPartitions = 5;
			const string topic = "non-persistent://my-property/my-ns/partitioned-topic";
			admin.topics().createPartitionedTopic(topic, numPartitions);

			PulsarClient client = PulsarClient.builder().serviceUrl(org.apache.pulsar.BrokerServiceUrl).statsInterval(0, TimeUnit.SECONDS).build();
			Consumer<sbyte[]> consumer = client.newConsumer().topic(topic).subscriptionName("subscriber-1").subscriptionType(type).subscribe();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topic).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();

			// Ensure all partitions exist
			for (int i = 0; i < numPartitions; i++)
			{
				TopicName partition = TopicName.get(topic).getPartition(i);
				assertNotNull(org.apache.pulsar.BrokerService.getTopicReference(partition.ToString()));
			}

			int totalProduceMsg = 500;
			for (int i = 0; i < totalProduceMsg; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
				Thread.Sleep(10);
			}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Message<?> msg = null;
			Message<object> msg = null;
			ISet<string> messageSet = Sets.newHashSet();
			for (int i = 0; i < totalProduceMsg; i++)
			{
				msg = consumer.receive(1, TimeUnit.SECONDS);
				if (msg != null)
				{
					consumer.acknowledge(msg);
					string receivedMessage = new string(msg.Data);
					log.debug("Received message: [{}]", receivedMessage);
					string expectedMessage = "my-message-" + i;
					testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
				}
				else
				{
					break;
				}
			}
			assertEquals(messageSet.Count, totalProduceMsg);

			producer.close();
			consumer.close();
			log.info("-- Exiting {} test --", methodName);
			client.Dispose();
		}

		/// <summary>
		/// It verifies that broker doesn't dispatch messages if consumer runs out of permits filled out with messages
		/// </summary>
		/// 
		public virtual void testConsumerInternalQueueMaxOut(SubscriptionType type)
		{
			log.info("-- Starting {} test --", methodName);

			const string topic = "non-persistent://my-property/my-ns/unacked-topic";
			const int queueSize = 10;
			ConsumerImpl<sbyte[]> consumer = (ConsumerImpl<sbyte[]>) pulsarClient.newConsumer().topic(topic).receiverQueueSize(queueSize).subscriptionName("subscriber-1").subscriptionType(type).subscribe();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topic).create();

			int totalProduceMsg = 50;
			for (int i = 0; i < totalProduceMsg; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
				Thread.Sleep(10);
			}


			Message<object> msg = null;
			ISet<string> messageSet = Sets.newHashSet();
			for (int i = 0; i < totalProduceMsg; i++)
			{
				msg = consumer.receive(1, TimeUnit.SECONDS);
				if (msg != null)
				{
					consumer.acknowledge(msg);
					string receivedMessage = new string(msg.Data);
					log.debug("Received message: [{}]", receivedMessage);
					string expectedMessage = "my-message-" + i;
					testMessageOrderAndDuplicates(messageSet, receivedMessage, expectedMessage);
				}
				else
				{
					break;
				}
			}
			assertEquals(messageSet.Count, queueSize);

			producer.close();
			consumer.close();
			log.info("-- Exiting {} test --", methodName);

		}

		/// <summary>
		/// Verifies that broker should failed to publish message if producer publishes messages more than rate limit
		/// </summary>
		/// 
		public virtual void testProducerRateLimit()
		{
			int defaultNonPersistentMessageRate = conf.MaxConcurrentNonPersistentMessagePerConnection;
			try
			{
				const string topic = "non-persistent://my-property/my-ns/unacked-topic";
				// restart broker with lower publish rate limit
				conf.MaxConcurrentNonPersistentMessagePerConnection = 1;
				stopBroker();
				startBroker();
				// produce message concurrently
				ExecutorService executor = Executors.newFixedThreadPool(5);
				AtomicBoolean failed = new AtomicBoolean(false);
				Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topic).subscriptionName("subscriber-1").subscribe();
				Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topic).create();
				sbyte[] msgData = "testData".GetBytes();
				const int totalProduceMessages = 10;
				System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(totalProduceMessages);
				for (int i = 0; i < totalProduceMessages; i++)
				{
					executor.submit(() =>
					{
					try
					{
						producer.send(msgData);
					}
					catch (Exception e)
					{
						log.error("Failed to send message", e);
						failed.set(true);
					}
					latch.Signal();
					});
				}
				latch.await();


				Message<object> msg = null;
				ISet<string> messageSet = Sets.newHashSet();
				for (int i = 0; i < totalProduceMessages; i++)
				{
					msg = consumer.receive(500, TimeUnit.MILLISECONDS);
					if (msg != null)
					{
						messageSet.Add(new string(msg.Data));
					}
					else
					{
						break;
					}
				}

				// publish should not be failed
				assertFalse(failed.get());
				// but as message should be dropped at broker: broker should not receive the message
				assertNotEquals(messageSet.Count, totalProduceMessages);

				executor.shutdown();
				producer.close();
			}
			finally
			{
				conf.MaxConcurrentNonPersistentMessagePerConnection = defaultNonPersistentMessageRate;
			}
		}

		/// <summary>
		/// verifies message delivery with multiple consumers on shared and failover subscriptions
		/// </summary>
		/// <exception cref="Exception"> </exception>
		/// 
		public virtual void testMultipleSubscription()
		{
			log.info("-- Starting {} test --", methodName);

			const string topic = "non-persistent://my-property/my-ns/unacked-topic";
			ConsumerImpl<sbyte[]> consumer1Shared = (ConsumerImpl<sbyte[]>) pulsarClient.newConsumer().topic(topic).subscriptionName("subscriber-shared").subscriptionType(SubscriptionType.Shared).subscribe();

			ConsumerImpl<sbyte[]> consumer2Shared = (ConsumerImpl<sbyte[]>) pulsarClient.newConsumer().topic(topic).subscriptionName("subscriber-shared").subscriptionType(SubscriptionType.Shared).subscribe();

			ConsumerImpl<sbyte[]> consumer1FailOver = (ConsumerImpl<sbyte[]>) pulsarClient.newConsumer().topic(topic).subscriptionName("subscriber-fo").subscriptionType(SubscriptionType.Failover).subscribe();

			ConsumerImpl<sbyte[]> consumer2FailOver = (ConsumerImpl<sbyte[]>) pulsarClient.newConsumer().topic(topic).subscriptionName("subscriber-fo").subscriptionType(SubscriptionType.Failover).subscribe();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topic).create();

			int totalProduceMsg = 500;
			for (int i = 0; i < totalProduceMsg; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
				Thread.Sleep(10);
			}

			// consume from shared-subscriptions

			Message<object> msg = null;
			ISet<string> messageSet = Sets.newHashSet();
			for (int i = 0; i < totalProduceMsg; i++)
			{
				msg = consumer1Shared.receive(500, TimeUnit.MILLISECONDS);
				if (msg != null)
				{
					messageSet.Add(new string(msg.Data));
				}
				else
				{
					break;
				}
			}
			for (int i = 0; i < totalProduceMsg; i++)
			{
				msg = consumer2Shared.receive(500, TimeUnit.MILLISECONDS);
				if (msg != null)
				{
					messageSet.Add(new string(msg.Data));
				}
				else
				{
					break;
				}
			}
			assertEquals(messageSet.Count, totalProduceMsg);

			// consume from failover-subscriptions
			messageSet.Clear();
			for (int i = 0; i < totalProduceMsg; i++)
			{
				msg = consumer1FailOver.receive(500, TimeUnit.MILLISECONDS);
				if (msg != null)
				{
					messageSet.Add(new string(msg.Data));
				}
				else
				{
					break;
				}
			}
			for (int i = 0; i < totalProduceMsg; i++)
			{
				msg = consumer2FailOver.receive(500, TimeUnit.MILLISECONDS);
				if (msg != null)
				{
					messageSet.Add(new string(msg.Data));
				}
				else
				{
					break;
				}
			}
			assertEquals(messageSet.Count, totalProduceMsg);

			producer.close();
			consumer1Shared.close();
			consumer2Shared.close();
			consumer1FailOver.close();
			consumer2FailOver.close();
			log.info("-- Exiting {} test --", methodName);

		}

		/// <summary>
		/// verifies that broker is capturing topic stats correctly
		/// </summary>
		/// 
		public virtual void testTopicStats()
		{

			const string topicName = "non-persistent://my-property/my-ns/unacked-topic";
			const string subName = "non-persistent";
			const int timeWaitToSync = 100;

			NonPersistentTopicStats stats;
			SubscriptionStats subStats;

			Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionType(SubscriptionType.Shared).subscriptionName(subName).subscribe();
			Thread.Sleep(timeWaitToSync);

			NonPersistentTopic topicRef = (NonPersistentTopic) org.apache.pulsar.BrokerService.getTopicReference(topicName).get();
			assertNotNull(topicRef);

			rolloverPerIntervalStats(org.apache.pulsar);
			stats = topicRef.getStats(false);
			subStats = stats.Subscriptions.Values.GetEnumerator().next();

			// subscription stats
			assertEquals(stats.Subscriptions.Keys.Count, 1);
			assertEquals(subStats.consumers.Count, 1);

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).create();
			Thread.Sleep(timeWaitToSync);

			int totalProducedMessages = 100;
			for (int i = 0; i < totalProducedMessages; i++)
			{
				string message = "my-message-" + i;
				producer.send(message.GetBytes());
			}
			Thread.Sleep(timeWaitToSync);

			rolloverPerIntervalStats(org.apache.pulsar);
			stats = topicRef.getStats(false);
			subStats = stats.Subscriptions.Values.GetEnumerator().next();

			assertTrue(subStats.msgRateOut > 0);
			assertEquals(subStats.consumers.Count, 1);
			assertTrue(subStats.msgThroughputOut > 0);

			// consumer stats
			assertTrue(subStats.consumers[0].msgRateOut > 0.0);
			assertTrue(subStats.consumers[0].msgThroughputOut > 0.0);
			assertEquals(subStats.msgRateRedeliver, 0.0);
			producer.close();
			consumer.close();

		}

		/// <summary>
		/// verifies that non-persistent topic replicates using replicator
		/// </summary>
		/// 
		public virtual void testReplicator()
		{

			ReplicationClusterManager replication = new ReplicationClusterManager(this);
			replication.setupReplicationCluster();
			try
			{
				const string globalTopicName = "non-persistent://pulsar/global/ns/nonPersistentTopic";
				const int timeWaitToSync = 100;

				NonPersistentTopicStats stats;
				SubscriptionStats subStats;

				PulsarClient client1 = PulsarClient.builder().serviceUrl(replication.url1.ToString()).build();
				PulsarClient client2 = PulsarClient.builder().serviceUrl(replication.url2.ToString()).build();
				PulsarClient client3 = PulsarClient.builder().serviceUrl(replication.url3.ToString()).build();

				ConsumerImpl<sbyte[]> consumer1 = (ConsumerImpl<sbyte[]>) client1.newConsumer().topic(globalTopicName).subscriptionName("subscriber-1").subscribe();
				ConsumerImpl<sbyte[]> consumer2 = (ConsumerImpl<sbyte[]>) client1.newConsumer().topic(globalTopicName).subscriptionName("subscriber-2").subscribe();

				ConsumerImpl<sbyte[]> repl2Consumer = (ConsumerImpl<sbyte[]>) client2.newConsumer().topic(globalTopicName).subscriptionName("subscriber-1").subscribe();
				ConsumerImpl<sbyte[]> repl3Consumer = (ConsumerImpl<sbyte[]>) client3.newConsumer().topic(globalTopicName).subscriptionName("subscriber-1").subscribe();

				Producer<sbyte[]> producer = client1.newProducer().topic(globalTopicName).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();

				Thread.Sleep(timeWaitToSync);

				PulsarService replicationPulasr = replication.pulsar1;

				// Replicator for r1 -> r2,r3
				NonPersistentTopic topicRef = (NonPersistentTopic) replication.pulsar1.BrokerService.getTopicReference(globalTopicName).get();
				NonPersistentReplicator replicatorR2 = (NonPersistentReplicator) topicRef.getPersistentReplicator("r2");
				NonPersistentReplicator replicatorR3 = (NonPersistentReplicator) topicRef.getPersistentReplicator("r3");
				assertNotNull(topicRef);
				assertNotNull(replicatorR2);
				assertNotNull(replicatorR3);

				rolloverPerIntervalStats(replicationPulasr);
				stats = topicRef.getStats(false);
				subStats = stats.Subscriptions.Values.GetEnumerator().next();

				// subscription stats
				assertEquals(stats.Subscriptions.Keys.Count, 2);
				assertEquals(subStats.consumers.Count, 1);

				Thread.Sleep(timeWaitToSync);

				int totalProducedMessages = 100;
				for (int i = 0; i < totalProducedMessages; i++)
				{
					string message = "my-message-" + i;
					producer.send(message.GetBytes());
				}

				// (1) consume by consumer1

				Message<object> msg = null;
				ISet<string> messageSet = Sets.newHashSet();
				for (int i = 0; i < totalProducedMessages; i++)
				{
					msg = consumer1.receive(300, TimeUnit.MILLISECONDS);
					if (msg != null)
					{
						string receivedMessage = new string(msg.Data);
						testMessageOrderAndDuplicates(messageSet, receivedMessage, "my-message-" + i);
					}
					else
					{
						break;
					}
				}
				assertEquals(messageSet.Count, totalProducedMessages);

				// (2) consume by consumer2
				messageSet.Clear();
				for (int i = 0; i < totalProducedMessages; i++)
				{
					msg = consumer2.receive(300, TimeUnit.MILLISECONDS);
					if (msg != null)
					{
						string receivedMessage = new string(msg.Data);
						testMessageOrderAndDuplicates(messageSet, receivedMessage, "my-message-" + i);
					}
					else
					{
						break;
					}
				}
				assertEquals(messageSet.Count, totalProducedMessages);

				// (3) consume by repl2consumer
				messageSet.Clear();
				for (int i = 0; i < totalProducedMessages; i++)
				{
					msg = repl2Consumer.receive(300, TimeUnit.MILLISECONDS);
					if (msg != null)
					{
						string receivedMessage = new string(msg.Data);
						testMessageOrderAndDuplicates(messageSet, receivedMessage, "my-message-" + i);
					}
					else
					{
						break;
					}
				}
				assertEquals(messageSet.Count, totalProducedMessages);

				// (4) consume by repl3consumer
				messageSet.Clear();
				for (int i = 0; i < totalProducedMessages; i++)
				{
					msg = repl3Consumer.receive(300, TimeUnit.MILLISECONDS);
					if (msg != null)
					{
						string receivedMessage = new string(msg.Data);
						testMessageOrderAndDuplicates(messageSet, receivedMessage, "my-message-" + i);
					}
					else
					{
						break;
					}
				}
				assertEquals(messageSet.Count, totalProducedMessages);

				Thread.Sleep(timeWaitToSync);

				rolloverPerIntervalStats(replicationPulasr);
				stats = topicRef.getStats(false);
				subStats = stats.Subscriptions.Values.GetEnumerator().next();

				assertTrue(subStats.msgRateOut > 0);
				assertEquals(subStats.consumers.Count, 1);
				assertTrue(subStats.msgThroughputOut > 0);

				// consumer stats
				assertTrue(subStats.consumers[0].msgRateOut > 0.0);
				assertTrue(subStats.consumers[0].msgThroughputOut > 0.0);
				assertEquals(subStats.msgRateRedeliver, 0.0);

				producer.close();
				consumer1.close();
				repl2Consumer.close();
				repl3Consumer.close();
				client1.Dispose();
				client2.Dispose();
				client3.Dispose();

			}
			finally
			{
				replication.shutdownReplicationCluster();
			}

		}

		/// <summary>
		/// verifies load manager assigns topic only if broker started in non-persistent mode
		/// 
		/// <pre>
		/// 1. Start broker with disable non-persistent topic mode
		/// 2. Create namespace with non-persistency set
		/// 3. Create non-persistent topic
		/// 4. Load-manager should not be able to find broker
		/// 5. Create producer on that topic should fail
		/// </pre>
		/// </summary>
		public virtual void testLoadManagerAssignmentForNonPersistentTestAssignment(string loadManagerName)
		{

			const string @namespace = "my-property/my-ns";

			string topicName = "non-persistent://" + @namespace + "/loadManager";

			string defaultLoadManagerName = conf.LoadManagerClassName;

			bool defaultENableNonPersistentTopic = conf.EnableNonPersistentTopics;
			try
			{
				// start broker to not own non-persistent namespace and create non-persistent namespace
				stopBroker();
				conf.EnableNonPersistentTopics = false;
				conf.LoadManagerClassName = loadManagerName;
				startBroker();

				System.Reflection.FieldInfo field = typeof(PulsarService).getDeclaredField("loadManager");
				field.Accessible = true;
                AtomicReference<LoadManager> loadManagerRef = (AtomicReference<LoadManager>) field.get(org.apache.pulsar);
				LoadManager manager = LoadManager.create(org.apache.pulsar);
				manager.start();
				LoadManager oldLoadManager = loadManagerRef.getAndSet(manager);
				oldLoadManager.stop();

				NamespaceBundle fdqn = org.apache.pulsar.NamespaceService.getBundle(TopicName.get(topicName));
				LoadManager loadManager = org.apache.pulsar.LoadManager.get();
				ResourceUnit broker = null;
				try
				{
					broker = loadManager.getLeastLoaded(fdqn).get();
				}
				catch (Exception)
				{
					// Ok. (ModulearLoadManagerImpl throws RuntimeException incase don't find broker)
				}
				assertNull(broker);

				try
				{
					Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).createAsync().get(1, TimeUnit.SECONDS);
					producer.close();
					fail("topic loading should have failed");
				}
				catch (Exception)
				{
					// Ok
				}
				assertFalse(org.apache.pulsar.BrokerService.getTopicReference(topicName).Present);

			}
			finally
			{
				conf.EnableNonPersistentTopics = defaultENableNonPersistentTopic;
				conf.LoadManagerClassName = defaultLoadManagerName;
			}

		}

		/// <summary>
		/// verifies: broker should reject non-persistent topic loading if broker is not enable for non-persistent topic
		/// </summary>
		/// <param name="loadManagerName"> </param>
		/// <exception cref="Exception"> </exception>
		/// 
		public virtual void testNonPersistentTopicUnderPersistentNamespace()
		{

			const string @namespace = "my-property/my-ns";

			string topicName = "non-persistent://" + @namespace + "/persitentNamespace";


			bool defaultENableNonPersistentTopic = conf.EnableNonPersistentTopics;
			try
			{
				conf.EnableNonPersistentTopics = false;
				stopBroker();
				startBroker();
				try
				{
					Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).createAsync().get(1, TimeUnit.SECONDS);
					producer.close();
					fail("topic loading should have failed");
				}
				catch (Exception)
				{
					// Ok
				}

				assertFalse(org.apache.pulsar.BrokerService.getTopicReference(topicName).Present);
			}
			finally
			{
				conf.EnableNonPersistentTopics = defaultENableNonPersistentTopic;
			}
		}

		/// <summary>
		/// verifies that broker started with onlyNonPersistent mode doesn't own persistent-topic
		/// </summary>
		/// <param name="loadManagerName"> </param>
		/// <exception cref="Exception"> </exception>
		/// 
		public virtual void testNonPersistentBrokerModeRejectPersistentTopic(string loadManagerName)
		{

			const string @namespace = "my-property/my-ns";

			string topicName = "persistent://" + @namespace + "/loadManager";

			string defaultLoadManagerName = conf.LoadManagerClassName;

			bool defaultEnablePersistentTopic = conf.EnablePersistentTopics;

			bool defaultEnableNonPersistentTopic = conf.EnableNonPersistentTopics;
			try
			{
				// start broker to not own non-persistent namespace and create non-persistent namespace
				stopBroker();
				conf.EnableNonPersistentTopics = true;
				conf.EnablePersistentTopics = false;
				conf.LoadManagerClassName = loadManagerName;
				startBroker();

				System.Reflection.FieldInfo field = typeof(PulsarService).getDeclaredField("loadManager");
				field.Accessible = true;
                
                AtomicReference<LoadManager> loadManagerRef = (AtomicReference<LoadManager>) field.get(org.apache.pulsar);
				LoadManager manager = LoadManager.create(org.apache.pulsar);
				manager.start();
				LoadManager oldLoadManager = loadManagerRef.getAndSet(manager);
				oldLoadManager.stop();

				NamespaceBundle fdqn = org.apache.pulsar.NamespaceService.getBundle(TopicName.get(topicName));
				LoadManager loadManager = org.apache.pulsar.LoadManager.get();
				ResourceUnit broker = null;
				try
				{
					broker = loadManager.getLeastLoaded(fdqn).get();
				}
				catch (Exception)
				{
					// Ok. (ModulearLoadManagerImpl throws RuntimeException incase don't find broker)
				}
				assertNull(broker);

				try
				{
					Producer<sbyte[]> producer = pulsarClient.newProducer().topic(topicName).createAsync().get(1, TimeUnit.SECONDS);
					producer.close();
					fail("topic loading should have failed");
				}
				catch (Exception)
				{
					// Ok
				}

				assertFalse(org.apache.pulsar.BrokerService.getTopicReference(topicName).Present);

			}
			finally
			{
				conf.EnablePersistentTopics = defaultEnablePersistentTopic;
				conf.EnableNonPersistentTopics = defaultEnableNonPersistentTopic;
				conf.LoadManagerClassName = defaultLoadManagerName;
			}

		}

		/// <summary>
		/// Verifies msg-drop stats
		/// </summary>
		/// <exception cref="Exception"> </exception>
		/// 
		public virtual void testMsgDropStat()
		{

			int defaultNonPersistentMessageRate = conf.MaxConcurrentNonPersistentMessagePerConnection;
			try
			{
				const string topicName = "non-persistent://my-property/my-ns/stats-topic";
				// restart broker with lower publish rate limit
				conf.MaxConcurrentNonPersistentMessagePerConnection = 1;
				stopBroker();
				startBroker();
				Consumer<sbyte[]> consumer = pulsarClient.newConsumer().topic(topicName).subscriptionName("subscriber-1").receiverQueueSize(1).subscribe();

				Consumer<sbyte[]> consumer2 = pulsarClient.newConsumer().topic(topicName).subscriptionName("subscriber-2").receiverQueueSize(1).subscriptionType(SubscriptionType.Shared).subscribe();

				ProducerImpl<sbyte[]> producer = (ProducerImpl<sbyte[]>) pulsarClient.newProducer().topic(topicName).enableBatching(false).messageRoutingMode(MessageRoutingMode.SinglePartition).create();
				string firstTimeConnected = producer.ConnectedSince;
				ExecutorService executor = Executors.newFixedThreadPool(5);
				sbyte[] msgData = "testData".GetBytes();
				const int totalProduceMessages = 200;
				System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent(totalProduceMessages);
				for (int i = 0; i < totalProduceMessages; i++)
				{
					executor.submit(() =>
					{
					producer.sendAsync(msgData).handle((msg, e) =>
					{
						latch.Signal();
						return null;
					});
					});
				}
				latch.await();

				NonPersistentTopic topic = (NonPersistentTopic) org.apache.pulsar.BrokerService.getOrCreateTopic(topicName).get();
				org.apache.pulsar.BrokerService.updateRates();
				NonPersistentTopicStats stats = topic.getStats(false);
				NonPersistentPublisherStats npStats = stats.Publishers[0];
				NonPersistentSubscriptionStats sub1Stats = stats.Subscriptions["subscriber-1"];
				NonPersistentSubscriptionStats sub2Stats = stats.Subscriptions["subscriber-2"];
				assertTrue(npStats.msgDropRate > 0);
				assertTrue(sub1Stats.msgDropRate > 0);
				assertTrue(sub2Stats.msgDropRate > 0);

				producer.close();
				consumer.close();
				consumer2.close();
				executor.shutdown();
			}
			finally
			{
				conf.MaxConcurrentNonPersistentMessagePerConnection = defaultNonPersistentMessageRate;
			}

		}

		public class ReplicationClusterManager
		{
			private readonly NonPersistentTopicTest outerInstance;

			public ReplicationClusterManager(NonPersistentTopicTest outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal URL url1;
			internal PulsarService pulsar1;
			internal BrokerService ns1;

			internal PulsarAdmin admin1;
			internal LocalBookkeeperEnsemble bkEnsemble1;

			internal URL url2;
			internal ServiceConfiguration config2;
			internal PulsarService pulsar2;
			internal BrokerService ns2;
			internal PulsarAdmin admin2;
			internal LocalBookkeeperEnsemble bkEnsemble2;

			internal URL url3;
			internal ServiceConfiguration config3;
			internal PulsarService pulsar3;
			internal BrokerService ns3;
			internal PulsarAdmin admin3;
			internal LocalBookkeeperEnsemble bkEnsemble3;

			internal ZookeeperServerTest globalZkS;

			internal ExecutorService executor = new ThreadPoolExecutor(5, 20, 30, TimeUnit.SECONDS, new LinkedBlockingQueue<ThreadStart>());

			internal const int TIME_TO_CHECK_BACKLOG_QUOTA = 5;

			// Default frequency
			public virtual int BrokerServicePurgeInactiveFrequency
			{
				get
				{
					return 60;
				}
			}

			public virtual bool BrokerServicePurgeInactiveTopic
			{
				get
				{
					return false;
				}
			}


			public virtual void setupReplicationCluster()
			{
				log.info("--- Starting ReplicatorTestBase::setup ---");
				globalZkS = new ZookeeperServerTest(0);
				globalZkS.start();

				// Start region 1
				bkEnsemble1 = new LocalBookkeeperEnsemble(3, 0, () => 0);
				bkEnsemble1.start();

				// NOTE: we have to instantiate a new copy of System.getProperties() to make sure pulsar1 and pulsar2 have
				// completely
				// independent config objects instead of referring to the same properties object
				ServiceConfiguration config1 = new ServiceConfiguration();
				config1.ClusterName = outerInstance.configClusterName;
				config1.AdvertisedAddress = "localhost";
				config1.WebServicePort = 0;
				config1.ZookeeperServers = "127.0.0.1:" + bkEnsemble1.ZookeeperPort;
				config1.ConfigurationStoreServers = "127.0.0.1:" + globalZkS.ZookeeperPort + "/foo";
				config1.BrokerDeleteInactiveTopicsEnabled = BrokerServicePurgeInactiveTopic;
				config1.BrokerDeleteInactiveTopicsFrequencySeconds = inSec(BrokerServicePurgeInactiveFrequency, TimeUnit.SECONDS);
				config1.BrokerServicePort = 0;
				config1.BacklogQuotaCheckIntervalInSeconds = TIME_TO_CHECK_BACKLOG_QUOTA;
				config1.AllowAutoTopicCreationType = "non-partitioned";
				pulsar1 = new PulsarService(config1);
				pulsar1.start();
				ns1 = pulsar1.BrokerService;

				url1 = new URL(pulsar1.WebServiceAddress);
				admin1 = PulsarAdmin.builder().serviceHttpUrl(url1.ToString()).build();

				// Start region 2

				// Start zk & bks
				bkEnsemble2 = new LocalBookkeeperEnsemble(3, 0, () => 0);
				bkEnsemble2.start();

				config2 = new ServiceConfiguration();
				config2.ClusterName = "r2";
				config2.WebServicePort = 0;
				config2.AdvertisedAddress = "localhost";
				config2.ZookeeperServers = "127.0.0.1:" + bkEnsemble2.ZookeeperPort;
				config2.ConfigurationStoreServers = "127.0.0.1:" + globalZkS.ZookeeperPort + "/foo";
				config2.BrokerDeleteInactiveTopicsEnabled = BrokerServicePurgeInactiveTopic;
				config2.BrokerDeleteInactiveTopicsFrequencySeconds = inSec(BrokerServicePurgeInactiveFrequency, TimeUnit.SECONDS);
				config2.BrokerServicePort = 0;
				config2.BacklogQuotaCheckIntervalInSeconds = TIME_TO_CHECK_BACKLOG_QUOTA;
				config2.AllowAutoTopicCreationType = "non-partitioned";
				pulsar2 = new PulsarService(config2);
				pulsar2.start();
				ns2 = pulsar2.BrokerService;

				url2 = new URL(pulsar2.WebServiceAddress);
				admin2 = PulsarAdmin.builder().serviceHttpUrl(url2.ToString()).build();

				// Start region 3

				// Start zk & bks
				bkEnsemble3 = new LocalBookkeeperEnsemble(3, 0, () => 0);
				bkEnsemble3.start();

				config3 = new ServiceConfiguration();
				config3.ClusterName = "r3";
				config3.WebServicePort = 0;
				config3.AdvertisedAddress = "localhost";
				config3.ZookeeperServers = "127.0.0.1:" + bkEnsemble3.ZookeeperPort;
				config3.ConfigurationStoreServers = "127.0.0.1:" + globalZkS.ZookeeperPort + "/foo";
				config3.BrokerDeleteInactiveTopicsEnabled = BrokerServicePurgeInactiveTopic;
				config3.BrokerDeleteInactiveTopicsFrequencySeconds = inSec(BrokerServicePurgeInactiveFrequency, TimeUnit.SECONDS);
				config3.BrokerServicePort = 0;
				config3.AllowAutoTopicCreationType = "non-partitioned";
				pulsar3 = new PulsarService(config3);
				pulsar3.start();
				ns3 = pulsar3.BrokerService;

				url3 = new URL(pulsar3.WebServiceAddress);
				admin3 = PulsarAdmin.builder().serviceHttpUrl(url3.ToString()).build();

				// Provision the global namespace
				admin1.clusters().createCluster("r1", new ClusterData(url1.ToString(), null, pulsar1.SafeBrokerServiceUrl, pulsar1.BrokerServiceUrlTls));
				admin1.clusters().createCluster("r2", new ClusterData(url2.ToString(), null, pulsar2.SafeBrokerServiceUrl, pulsar1.BrokerServiceUrlTls));
				admin1.clusters().createCluster("r3", new ClusterData(url3.ToString(), null, pulsar3.SafeBrokerServiceUrl, pulsar1.BrokerServiceUrlTls));

				admin1.clusters().createCluster("global", new ClusterData("http://global:8080"));
				admin1.tenants().createTenant("pulsar", new TenantInfo(Sets.newHashSet("appid1", "appid2", "appid3"), Sets.newHashSet("r1", "r2", "r3")));
				admin1.namespaces().createNamespace("pulsar/global/ns");
				admin1.namespaces().setNamespaceReplicationClusters("pulsar/global/ns", Sets.newHashSet("r1", "r2", "r3"));

				assertEquals(admin2.clusters().getCluster("r1").ServiceUrl, url1.ToString());
				assertEquals(admin2.clusters().getCluster("r2").ServiceUrl, url2.ToString());
				assertEquals(admin2.clusters().getCluster("r3").ServiceUrl, url3.ToString());
				assertEquals(admin2.clusters().getCluster("r1").BrokerServiceUrl, pulsar1.SafeBrokerServiceUrl);
				assertEquals(admin2.clusters().getCluster("r2").BrokerServiceUrl, pulsar2.SafeBrokerServiceUrl);
				assertEquals(admin2.clusters().getCluster("r3").BrokerServiceUrl, pulsar3.SafeBrokerServiceUrl);
				Thread.Sleep(100);
				log.info("--- ReplicatorTestBase::setup completed ---");

			}

			public virtual int inSec(int time, TimeUnit unit)
			{
				return (int) TimeUnit.SECONDS.convert(time, unit);
			}


			public virtual void shutdownReplicationCluster()
			{
				log.info("--- Shutting down ---");
				executor.shutdown();

				admin1.Dispose();
				admin2.Dispose();
				admin3.Dispose();

				pulsar3.close();
				ns3.Dispose();

				pulsar2.close();
				ns2.Dispose();

				pulsar1.close();
				ns1.Dispose();

				bkEnsemble1.stop();
				bkEnsemble2.stop();
				bkEnsemble3.stop();
				globalZkS.stop();
			}

		}

		private void rolloverPerIntervalStats(PulsarService pulsar)
		{
			try
			{
				pulsar.Executor.submit(() => pulsar.BrokerService.updateRates()).get();
			}
			catch (Exception e)
			{
				log.error("Stats executor error", e);
			}
		}
	}

}