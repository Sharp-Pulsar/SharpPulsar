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
namespace SharpPulsar.Test.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.fail;

	using InvalidServiceURL = Org.Apache.Pulsar.Client.Api.PulsarClientException.InvalidServiceURL;

    /// <summary>
	/// Unit test <seealso cref="PulsarServiceNameResolver"/>.
	/// </summary>
	public class PulsarServiceNameResolverTest
	{

		private PulsarServiceNameResolver resolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void setup()
		public virtual void Setup()
		{
			this.resolver = new PulsarServiceNameResolver();
			assertNull(resolver.ServiceUrl);
			assertNull(resolver.ServiceUri);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalStateException.class) public void testResolveBeforeUpdateServiceUrl()
		public virtual void TestResolveBeforeUpdateServiceUrl()
		{
			resolver.ResolveHost();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalStateException.class) public void testResolveUrlBeforeUpdateServiceUrl() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestResolveUrlBeforeUpdateServiceUrl()
		{
			resolver.ResolveHostUri();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testUpdateInvalidServiceUrl()
		public virtual void TestUpdateInvalidServiceUrl()
		{
			string ServiceUrl = "pulsar:///";
			try
			{
				resolver.UpdateServiceUrl(ServiceUrl);
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
		public virtual void TestSimpleHostUrl()
		{
			string ServiceUrl = "pulsar://host1:6650";
			resolver.UpdateServiceUrl(ServiceUrl);
			assertEquals(ServiceUrl, resolver.ServiceUrl);
			assertEquals(ServiceURI.create(ServiceUrl), resolver.ServiceUri);

			InetSocketAddress ExpectedAddress = InetSocketAddress.createUnresolved("host1", 6650);
			assertEquals(ExpectedAddress, resolver.ResolveHost());
			assertEquals(URI.create(ServiceUrl), resolver.ResolveHostUri());

			string NewServiceUrl = "pulsar://host2:6650";
			resolver.UpdateServiceUrl(NewServiceUrl);
			assertEquals(NewServiceUrl, resolver.ServiceUrl);
			assertEquals(ServiceURI.create(NewServiceUrl), resolver.ServiceUri);

			InetSocketAddress NewExpectedAddress = InetSocketAddress.createUnresolved("host2", 6650);
			assertEquals(NewExpectedAddress, resolver.ResolveHost());
			assertEquals(URI.create(NewServiceUrl), resolver.ResolveHostUri());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultipleHostsUrl() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestMultipleHostsUrl()
		{
			string ServiceUrl = "pulsar://host1:6650,host2:6650";
			resolver.UpdateServiceUrl(ServiceUrl);
			assertEquals(ServiceUrl, resolver.ServiceUrl);
			assertEquals(ServiceURI.create(ServiceUrl), resolver.ServiceUri);

			ISet<InetSocketAddress> ExpectedAddresses = new HashSet<InetSocketAddress>();
			ISet<URI> ExpectedHostUrls = new HashSet<URI>();
			ExpectedAddresses.Add(InetSocketAddress.createUnresolved("host1", 6650));
			ExpectedAddresses.Add(InetSocketAddress.createUnresolved("host2", 6650));
			ExpectedHostUrls.Add(URI.create("pulsar://host1:6650"));
			ExpectedHostUrls.Add(URI.create("pulsar://host2:6650"));

			for (int I = 0; I < 10; I++)
			{
				assertTrue(ExpectedAddresses.Contains(resolver.ResolveHost()));
				assertTrue(ExpectedHostUrls.Contains(resolver.ResolveHostUri()));
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultipleHostsTlsUrl() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestMultipleHostsTlsUrl()
		{
			string ServiceUrl = "pulsar+ssl://host1:6651,host2:6651";
			resolver.UpdateServiceUrl(ServiceUrl);
			assertEquals(ServiceUrl, resolver.ServiceUrl);
			assertEquals(ServiceURI.create(ServiceUrl), resolver.ServiceUri);

			ISet<InetSocketAddress> ExpectedAddresses = new HashSet<InetSocketAddress>();
			ISet<URI> ExpectedHostUrls = new HashSet<URI>();
			ExpectedAddresses.Add(InetSocketAddress.createUnresolved("host1", 6651));
			ExpectedAddresses.Add(InetSocketAddress.createUnresolved("host2", 6651));
			ExpectedHostUrls.Add(URI.create("pulsar+ssl://host1:6651"));
			ExpectedHostUrls.Add(URI.create("pulsar+ssl://host2:6651"));

			for (int I = 0; I < 10; I++)
			{
				assertTrue(ExpectedAddresses.Contains(resolver.ResolveHost()));
				assertTrue(ExpectedHostUrls.Contains(resolver.ResolveHostUri()));
			}
		}
	}

}