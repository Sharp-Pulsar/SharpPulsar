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
namespace org.apache.pulsar.client.tutorial
{
	using JsonProcessingException = com.fasterxml.jackson.core.JsonProcessingException;
	using Consumer = org.apache.pulsar.client.api.Consumer;
	using Message = org.apache.pulsar.client.api.Message;
	using PulsarClient = org.apache.pulsar.client.api.PulsarClient;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using org.apache.pulsar.client.impl.schema;

	public class SampleConsumerWithSchema
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws org.apache.pulsar.client.api.PulsarClientException, com.fasterxml.jackson.core.JsonProcessingException
		public static void Main(string[] args)
		{

			PulsarClient pulsarClient = PulsarClient.builder().serviceUrl("http://localhost:8080").build();

			Consumer<JsonPojo> consumer = pulsarClient.newConsumer(JSONSchema.of(SchemaDefinition.builder<JsonPojo>().withPojo(typeof(JsonPojo)).build())).topic("persistent://my-property/use/my-ns/my-topic").subscriptionName("my-subscription-name").subscribe();

			Message<JsonPojo> msg = null;

			for (int i = 0; i < 100; i++)
			{
				msg = consumer.receive();
				// do something
				Console.WriteLine("Received: " + msg.Value.content);
			}

			// Acknowledge the consumption of all messages at once
			consumer.acknowledgeCumulative(msg);
			pulsarClient.close();
		}
	}

}