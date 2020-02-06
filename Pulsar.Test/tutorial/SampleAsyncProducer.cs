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
namespace org.apache.pulsar.client.tutorial
{

	using MessageId = api.MessageId;
	using Producer = api.Producer;
	using PulsarClient = api.PulsarClient;
	using PulsarClientException = api.PulsarClientException;

	using Lists = com.google.common.collect.Lists;

	using Slf4j = lombok.@extern.slf4j.Slf4j;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class SampleAsyncProducer
	public class SampleAsyncProducer
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws org.apache.pulsar.client.api.PulsarClientException, InterruptedException, java.io.IOException
		public static void Main(string[] args)
		{
			PulsarClient pulsarClient = PulsarClient.builder().serviceUrl("http://localhost:8080").build();

			Producer<sbyte[]> producer = pulsarClient.newProducer().topic("persistent://my-tenant/my-ns/my-topic").sendTimeout(3, TimeUnit.SECONDS).create();

			IList<CompletableFuture<MessageId>> futures = Lists.newArrayList();

			for (int i = 0; i < 10; i++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String content = "my-message-" + i;
				string content = "my-message-" + i;
				CompletableFuture<MessageId> future = producer.sendAsync(content.GetBytes());

				future.handle((v, ex) =>
				{
				if (ex == null)
				{
					log.info("Message persisted: {}", content);
				}
				else
				{
					log.error("Error persisting message: {}", content, ex);
				}
				return null;
				});

				futures.Add(future);
			}

			log.info("Waiting for async ops to complete");
			foreach (CompletableFuture<MessageId> future in futures)
			{
				future.join();
			}

			log.info("All operations completed");

			pulsarClient.close();
		}

	}

}