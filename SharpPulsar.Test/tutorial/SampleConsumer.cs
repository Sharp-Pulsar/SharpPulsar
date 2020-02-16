﻿using System;

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
	using Consumer = Org.Apache.Pulsar.Client.Api.Consumer;
	using Org.Apache.Pulsar.Client.Api;
	using PulsarClient = Org.Apache.Pulsar.Client.Api.PulsarClient;
	using PulsarClientException = Org.Apache.Pulsar.Client.Api.PulsarClientException;

	public class SampleConsumer
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws org.apache.pulsar.client.api.PulsarClientException, InterruptedException
		public static void Main(string[] Args)
		{

			PulsarClient PulsarClient = PulsarClient.builder().serviceUrl("pulsar://localhost:6650").build();

			Consumer<sbyte[]> Consumer = PulsarClient.newConsumer().topic("persistent://my-tenant/my-ns/my-topic").subscriptionName("my-subscription-name").subscribe();

			Message<sbyte[]> Msg = null;

			for (int I = 0; I < 100; I++)
			{
				Msg = Consumer.receive();
				// do something
				Console.WriteLine("Received: " + new string(Msg.Data));
			}

			// Acknowledge the consumption of all messages at once
			Consumer.acknowledgeCumulative(Msg);
			PulsarClient.Dispose();
		}
	}

}