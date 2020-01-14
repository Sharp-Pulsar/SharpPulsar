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
namespace org.apache.pulsar.client.impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.fail;

	using InvalidServiceURL = org.apache.pulsar.client.api.PulsarClientException.InvalidServiceURL;
	using ServiceURI = org.apache.pulsar.common.net.ServiceURI;
	using BeforeMethod = org.testng.annotations.BeforeMethod;
	using Test = org.testng.annotations.Test;

	/// <summary>
	/// Unit test <seealso cref="PulsarServiceNameResolver"/>.
	/// </summary>
	public class PulsarServiceNameResolverTest
	{

		private PulsarServiceNameResolver resolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void setup()
		public virtual void setup()
		{
			this.resolver = new PulsarServiceNameResolver();
			assertNull(resolver.ServiceUrl);
			assertNull(resolver.ServiceUri);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalStateException.class) public void testResolveBeforeUpdateServiceUrl()
		public virtual void testResolveBeforeUpdateServiceUrl()
		{
			resolver.resolveHost();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalStateException.class) public void testResolveUrlBeforeUpdateServiceUrl() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testResolveUrlBeforeUpdateServiceUrl()
		{
			resolver.resolveHostUri();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testUpdateInvalidServiceUrl()
		public virtual void testUpdateInvalidServiceUrl()
		{
			string serviceUrl = "pulsar:///";
			try
			{
				resolver.updateServiceUrl(serviceUrl);
				fail("Should fail to update service url if service url is invalid");
			}
			catch (InvalidServiceURL)
			{
				// expected
			}
			assertNull(resolver.ServiceUrl);
			assertNull(resolver.ServiceUri);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimpleHostUrl() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testSimpleHostUrl()
		{
			string serviceUrl = "pulsar://host1:6650";
			resolver.updateServiceUrl(serviceUrl);
			assertEquals(serviceUrl, resolver.ServiceUrl);
			assertEquals(ServiceURI.create(serviceUrl), resolver.ServiceUri);

			InetSocketAddress expectedAddress = InetSocketAddress.createUnresolved("host1", 6650);
			assertEquals(expectedAddress, resolver.resolveHost());
			assertEquals(URI.create(serviceUrl), resolver.resolveHostUri());

			string newServiceUrl = "pulsar://host2:6650";
			resolver.updateServiceUrl(newServiceUrl);
			assertEquals(newServiceUrl, resolver.ServiceUrl);
			assertEquals(ServiceURI.create(newServiceUrl), resolver.ServiceUri);

			InetSocketAddress newExpectedAddress = InetSocketAddress.createUnresolved("host2", 6650);
			assertEquals(newExpectedAddress, resolver.resolveHost());
			assertEquals(URI.create(newServiceUrl), resolver.resolveHostUri());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultipleHostsUrl() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testMultipleHostsUrl()
		{
			string serviceUrl = "pulsar://host1:6650,host2:6650";
			resolver.updateServiceUrl(serviceUrl);
			assertEquals(serviceUrl, resolver.ServiceUrl);
			assertEquals(ServiceURI.create(serviceUrl), resolver.ServiceUri);

			ISet<InetSocketAddress> expectedAddresses = new HashSet<InetSocketAddress>();
			ISet<URI> expectedHostUrls = new HashSet<URI>();
			expectedAddresses.Add(InetSocketAddress.createUnresolved("host1", 6650));
			expectedAddresses.Add(InetSocketAddress.createUnresolved("host2", 6650));
			expectedHostUrls.Add(URI.create("pulsar://host1:6650"));
			expectedHostUrls.Add(URI.create("pulsar://host2:6650"));

			for (int i = 0; i < 10; i++)
			{
				assertTrue(expectedAddresses.Contains(resolver.resolveHost()));
				assertTrue(expectedHostUrls.Contains(resolver.resolveHostUri()));
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultipleHostsTlsUrl() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testMultipleHostsTlsUrl()
		{
			string serviceUrl = "pulsar+ssl://host1:6651,host2:6651";
			resolver.updateServiceUrl(serviceUrl);
			assertEquals(serviceUrl, resolver.ServiceUrl);
			assertEquals(ServiceURI.create(serviceUrl), resolver.ServiceUri);

			ISet<InetSocketAddress> expectedAddresses = new HashSet<InetSocketAddress>();
			ISet<URI> expectedHostUrls = new HashSet<URI>();
			expectedAddresses.Add(InetSocketAddress.createUnresolved("host1", 6651));
			expectedAddresses.Add(InetSocketAddress.createUnresolved("host2", 6651));
			expectedHostUrls.Add(URI.create("pulsar+ssl://host1:6651"));
			expectedHostUrls.Add(URI.create("pulsar+ssl://host2:6651"));

			for (int i = 0; i < 10; i++)
			{
				assertTrue(expectedAddresses.Contains(resolver.resolveHost()));
				assertTrue(expectedHostUrls.Contains(resolver.resolveHostUri()));
			}
		}
	}

}