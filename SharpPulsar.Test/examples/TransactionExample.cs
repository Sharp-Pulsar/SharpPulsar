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
namespace Org.Apache.Pulsar.Client.Examples
{
	using Org.Apache.Pulsar.Client.Api;
	using MessageId = Org.Apache.Pulsar.Client.Api.MessageId;
	using PulsarClient = Org.Apache.Pulsar.Client.Api.PulsarClient;
	using Org.Apache.Pulsar.Client.Api;
	using SubscriptionType = Org.Apache.Pulsar.Client.Api.SubscriptionType;
	using Transaction = Org.Apache.Pulsar.Client.Api.Transaction.Transaction;
	using Org.Apache.Pulsar.Client.Impl;
	using Org.Apache.Pulsar.Client.Impl;
	using PulsarClientImpl = Org.Apache.Pulsar.Client.Impl.PulsarClientImpl;

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
		public static void Main(string[] Args)
		{
			string ServiceUrl = "pulsar://localhost:6650";

			PulsarClientImpl Client = (PulsarClientImpl) PulsarClient.builder().serviceUrl(ServiceUrl).build();

			string InputTopic = "input-topic";
			string OutputTopic1 = "output-topic-1";
			string OutputTopic2 = "output-topic-2";

			ConsumerImpl<string> Consumer = (ConsumerImpl<string>) Client.newConsumer(SchemaFields.STRING).topic(InputTopic).subscriptionType(SubscriptionType.Exclusive).subscriptionName("transactional-sub").subscribe();

			ProducerImpl<string> Producer1 = (ProducerImpl<string>) Client.newProducer(SchemaFields.STRING).topic(OutputTopic1).sendTimeout(0, TimeUnit.MILLISECONDS).create();

			ProducerImpl<string> Producer2 = (ProducerImpl<string>) Client.newProducer(SchemaFields.STRING).topic(OutputTopic2).sendTimeout(0, TimeUnit.MILLISECONDS).create();


			while (true)
			{
				Message<string> Message = Consumer.receive();

				// process the messages to generate other messages
				string OutputMessage1 = Message.Value + "-output-1";
				string OutputMessage2 = Message.Value + "-output-2";

				Transaction Txn = Client.newTransaction().withTransactionTimeout(1, TimeUnit.MINUTES).build().get();

				CompletableFuture<MessageId> SendFuture1 = Producer1.newMessage(Txn).value(OutputMessage1).sendAsync();

				CompletableFuture<MessageId> SendFuture2 = Producer2.newMessage(Txn).value(OutputMessage2).sendAsync();

				CompletableFuture<Void> AckFuture = Consumer.acknowledgeAsync(Message.MessageId, Txn);

				Txn.commit().get();

				// the message ids can be returned from the sendFuture1 and sendFuture2

				MessageId MsgId1 = SendFuture1.get();
				MessageId MsgId2 = SendFuture2.get();
			}
		}

	}

}