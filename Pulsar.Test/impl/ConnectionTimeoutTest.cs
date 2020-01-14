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

	using ConnectTimeoutException = io.netty.channel.ConnectTimeoutException;
	using PulsarClient = org.apache.pulsar.client.api.PulsarClient;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;

	public class ConnectionTimeoutTest
	{

		// 192.0.2.0/24 is assigned for documentation, should be a deadend
		internal const string blackholeBroker = "pulsar://192.0.2.1:1234";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLowTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testLowTimeout()
		{
			long startNanos = System.nanoTime();

			using (PulsarClient clientLow = PulsarClient.builder().serviceUrl(blackholeBroker).connectionTimeout(1, TimeUnit.MILLISECONDS).operationTimeout(1000, TimeUnit.MILLISECONDS).build(), PulsarClient clientDefault = PulsarClient.builder().serviceUrl(blackholeBroker).operationTimeout(1000, TimeUnit.MILLISECONDS).build())
			{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.CompletableFuture<?> lowFuture = clientLow.newProducer().topic("foo").createAsync();
				CompletableFuture<object> lowFuture = clientLow.newProducer().topic("foo").createAsync();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.CompletableFuture<?> defaultFuture = clientDefault.newProducer().topic("foo").createAsync();
				CompletableFuture<object> defaultFuture = clientDefault.newProducer().topic("foo").createAsync();

				try
				{
					lowFuture.get();
					Assert.fail("Shouldn't be able to connect to anything");
				}
				catch (Exception e)
				{
					Assert.assertFalse(defaultFuture.Done);
					Assert.assertEquals(e.InnerException.InnerException.Cause.GetType(), typeof(ConnectTimeoutException));
				}
			}
		}
	}

}