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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	using Authentication = org.apache.pulsar.client.api.Authentication;
	using PulsarClient = org.apache.pulsar.client.api.PulsarClient;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using Test = org.testng.annotations.Test;

	public class ClientInitializationTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testInitializeAuthWithTls() throws org.apache.pulsar.client.api.PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testInitializeAuthWithTls()
		{
			Authentication auth = mock(typeof(Authentication));

			PulsarClient.builder().serviceUrl("pulsar+ssl://my-host:6650").authentication(auth).build();

			// Auth should only be started, though we shouldn't have tried to get credentials yet (until we first attempt to
			// connect).
			verify(auth).start();
			verifyNoMoreInteractions(auth);
		}
	}

}