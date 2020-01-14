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
	using PulsarClient = org.apache.pulsar.client.api.PulsarClient;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using ServiceUrlProvider = org.apache.pulsar.client.api.ServiceUrlProvider;
	using Test = org.testng.annotations.Test;

	public class ClientBuilderImplTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testClientBuilderWithServiceUrlAndServiceUrlProviderNotSet() throws org.apache.pulsar.client.api.PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testClientBuilderWithServiceUrlAndServiceUrlProviderNotSet()
		{
			PulsarClient.builder().build();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testClientBuilderWithNullServiceUrl() throws org.apache.pulsar.client.api.PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testClientBuilderWithNullServiceUrl()
		{
			PulsarClient.builder().serviceUrl(null).build();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testClientBuilderWithNullServiceUrlProvider() throws org.apache.pulsar.client.api.PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testClientBuilderWithNullServiceUrlProvider()
		{
			PulsarClient.builder().serviceUrlProvider(null).build();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testClientBuilderWithServiceUrlAndServiceUrlProvider() throws org.apache.pulsar.client.api.PulsarClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testClientBuilderWithServiceUrlAndServiceUrlProvider()
		{
			PulsarClient.builder().serviceUrlProvider(new ServiceUrlProviderAnonymousInnerClass(this))}@Test(expectedExceptions = typeof(System.ArgumentException)) public void testClientBuilderWithBlankServiceUrlInServiceUrlProvider() throws PulsarClientException{PulsarClient.builder().serviceUrlProvider(new ServiceUrlProvider(){@Override public void initialize(PulsarClient client){}@Override public string ServiceUrl{return "";
		   .serviceUrl("pulsar://localhost:6650").build();
		}

				private class ServiceUrlProviderAnonymousInnerClass : ServiceUrlProvider
				{
					private readonly ClientBuilderImplTest outerInstance;

					public ServiceUrlProviderAnonymousInnerClass(ClientBuilderImplTest outerInstance)
					{
						this.outerInstance = outerInstance;
					}

					public override void initialize(PulsarClient client)
					{

					}

					public override string ServiceUrl
					{
						get
						{
							return "pulsar://localhost:6650";
						}
					}
				}
	}
		   ).build();
}

	}

}