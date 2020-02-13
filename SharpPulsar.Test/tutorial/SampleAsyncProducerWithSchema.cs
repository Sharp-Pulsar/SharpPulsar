using System.Collections.Generic;

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
namespace Org.Apache.Pulsar.Client.Tutorial
{
	using Lists = com.google.common.collect.Lists;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using MessageId = Org.Apache.Pulsar.Client.Api.MessageId;
	using Producer = Org.Apache.Pulsar.Client.Api.Producer;
	using PulsarClient = Org.Apache.Pulsar.Client.Api.PulsarClient;
	using Org.Apache.Pulsar.Client.Api.Schema;
	using Org.Apache.Pulsar.Client.Impl.Schema;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class SampleAsyncProducerWithSchema
	public class SampleAsyncProducerWithSchema
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
		public static void Main(string[] Args)
		{
			PulsarClient PulsarClient = PulsarClient.builder().serviceUrl("http://localhost:8080").build();

			Producer<JsonPojo> Producer = PulsarClient.newProducer(JSONSchema.of(SchemaDefinition.builder<JsonPojo>().withPojo(typeof(JsonPojo)).build())).topic("persistent://my-property/use/my-ns/my-topic").sendTimeout(3, TimeUnit.SECONDS).create();

			IList<CompletableFuture<MessageId>> Futures = Lists.newArrayList();

			for (int I = 0; I < 10; I++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String content = "my-message-" + i;
				string Content = "my-message-" + I;
				CompletableFuture<MessageId> Future = Producer.sendAsync(new JsonPojo(Content));

				Future.handle((v, ex) =>
				{
				if (ex == null)
				{
					log.info("Message persisted: {}", Content);
				}
				else
				{
					log.error("Error persisting message: {}", Content, ex);
				}
				return null;
				});

				Futures.Add(Future);
			}

			log.info("Waiting for async ops to complete");
			foreach (CompletableFuture<MessageId> Future in Futures)
			{
				Future.join();
			}

			log.info("All operations completed");

			PulsarClient.Dispose();
		}

	}

}