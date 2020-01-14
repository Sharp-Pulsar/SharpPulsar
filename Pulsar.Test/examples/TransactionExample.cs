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
namespace org.apache.pulsar.client.examples
{
	using Message = org.apache.pulsar.client.api.Message;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using PulsarClient = org.apache.pulsar.client.api.PulsarClient;
	using Schema = org.apache.pulsar.client.api.Schema;
	using SubscriptionType = org.apache.pulsar.client.api.SubscriptionType;
	using Transaction = org.apache.pulsar.client.api.transaction.Transaction;
	using org.apache.pulsar.client.impl;
	using org.apache.pulsar.client.impl;
	using PulsarClientImpl = org.apache.pulsar.client.impl.PulsarClientImpl;

	/// <summary>
	/// Example to use Pulsar transactions.
	/// 
	/// <para>TODO: add an example about how to use the Pulsar transaction API.
	/// </para>
	/// </summary>
	public class TransactionExample
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws Exception
		public static void Main(string[] args)
		{
			string serviceUrl = "pulsar://localhost:6650";

			PulsarClientImpl client = (PulsarClientImpl) PulsarClient.builder().serviceUrl(serviceUrl).build();

			string inputTopic = "input-topic";
			string outputTopic1 = "output-topic-1";
			string outputTopic2 = "output-topic-2";

			ConsumerImpl<string> consumer = (ConsumerImpl<string>) client.newConsumer(Schema.STRING).topic(inputTopic).subscriptionType(SubscriptionType.Exclusive).subscriptionName("transactional-sub").subscribe();

			ProducerImpl<string> producer1 = (ProducerImpl<string>) client.newProducer(Schema.STRING).topic(outputTopic1).sendTimeout(0, TimeUnit.MILLISECONDS).create();

			ProducerImpl<string> producer2 = (ProducerImpl<string>) client.newProducer(Schema.STRING).topic(outputTopic2).sendTimeout(0, TimeUnit.MILLISECONDS).create();


			while (true)
			{
				Message<string> message = consumer.receive();

				// process the messages to generate other messages
				string outputMessage1 = message.Value + "-output-1";
				string outputMessage2 = message.Value + "-output-2";

				Transaction txn = client.newTransaction().withTransactionTimeout(1, TimeUnit.MINUTES).build().get();

				CompletableFuture<MessageId> sendFuture1 = producer1.newMessage(txn).value(outputMessage1).sendAsync();

				CompletableFuture<MessageId> sendFuture2 = producer2.newMessage(txn).value(outputMessage2).sendAsync();

				CompletableFuture<Void> ackFuture = consumer.acknowledgeAsync(message.MessageId, txn);

				txn.commit().get();

				// the message ids can be returned from the sendFuture1 and sendFuture2

				MessageId msgId1 = sendFuture1.get();
				MessageId msgId2 = sendFuture2.get();
			}
		}

	}

}