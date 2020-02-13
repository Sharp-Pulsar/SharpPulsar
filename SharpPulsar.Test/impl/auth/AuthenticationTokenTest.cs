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
namespace Org.Apache.Pulsar.Client.Impl.Auth
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;

	using Charsets = com.google.common.@base.Charsets;


	using FileUtils = org.apache.commons.io.FileUtils;
	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using AuthenticationDataProvider = Org.Apache.Pulsar.Client.Api.AuthenticationDataProvider;
	using ClientConfigurationData = Org.Apache.Pulsar.Client.Impl.Conf.ClientConfigurationData;
	using Test = org.testng.annotations.Test;

	public class AuthenticationTokenTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAuthToken() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAuthToken()
		{
			AuthenticationToken AuthToken = new AuthenticationToken("token-xyz");
			assertEquals(AuthToken.AuthMethodName, "token");

			AuthenticationDataProvider AuthData = AuthToken.AuthData;
			assertTrue(AuthData.hasDataFromCommand());
			assertEquals(AuthData.CommandData, "token-xyz");

			assertFalse(AuthData.hasDataForTls());
			assertNull(AuthData.TlsCertificates);
			assertNull(AuthData.TlsPrivateKey);

			assertTrue(AuthData.hasDataForHttp());
			assertEquals(AuthData.HttpHeaders, Collections.singletonMap("Authorization", "Bearer token-xyz").entrySet());

			AuthToken.Dispose();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAuthTokenClientConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAuthTokenClientConfig()
		{
			ClientConfigurationData ClientConfig = new ClientConfigurationData();
			ClientConfig.ServiceUrl = "pulsar://service-url";
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			ClientConfig.AuthPluginClassName = typeof(AuthenticationToken).FullName;
			ClientConfig.AuthParams = "token-xyz";

			PulsarClientImpl PulsarClient = new PulsarClientImpl(ClientConfig);

			Authentication AuthToken = PulsarClient.Configuration.Authentication;
			assertEquals(AuthToken.AuthMethodName, "token");

			AuthenticationDataProvider AuthData = AuthToken.AuthData;
			assertTrue(AuthData.hasDataFromCommand());
			assertEquals(AuthData.CommandData, "token-xyz");

			assertFalse(AuthData.hasDataForTls());
			assertNull(AuthData.TlsCertificates);
			assertNull(AuthData.TlsPrivateKey);

			assertTrue(AuthData.hasDataForHttp());
			assertEquals(AuthData.HttpHeaders, Collections.singletonMap("Authorization", "Bearer token-xyz").entrySet());

			AuthToken.Dispose();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAuthTokenConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAuthTokenConfig()
		{
			AuthenticationToken AuthToken = new AuthenticationToken();
			AuthToken.configure("token:my-test-token-string");
			assertEquals(AuthToken.AuthMethodName, "token");

			AuthenticationDataProvider AuthData = AuthToken.AuthData;
			assertTrue(AuthData.hasDataFromCommand());
			assertEquals(AuthData.CommandData, "my-test-token-string");
			AuthToken.Dispose();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAuthTokenConfigFromFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAuthTokenConfigFromFile()
		{
			File TokenFile = File.createTempFile("pular-test-token", ".key");
			TokenFile.deleteOnExit();
			FileUtils.write(TokenFile, "my-test-token-string", Charsets.UTF_8);

			AuthenticationToken AuthToken = new AuthenticationToken();
			AuthToken.configure("file://" + TokenFile);
			assertEquals(AuthToken.AuthMethodName, "token");

			AuthenticationDataProvider AuthData = AuthToken.AuthData;
			assertTrue(AuthData.hasDataFromCommand());
			assertEquals(AuthData.CommandData, "my-test-token-string");

			// Ensure if the file content changes, the token will get refreshed as well
			FileUtils.write(TokenFile, "other-token", Charsets.UTF_8);

			AuthenticationDataProvider AuthData2 = AuthToken.AuthData;
			assertTrue(AuthData2.hasDataFromCommand());
			assertEquals(AuthData2.CommandData, "other-token");

			AuthToken.Dispose();
		}

		/// <summary>
		/// File can have spaces and newlines before or after the token. We should be able to read
		/// the token correctly anyway.
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAuthTokenConfigFromFileWithNewline() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAuthTokenConfigFromFileWithNewline()
		{
			File TokenFile = File.createTempFile("pular-test-token", ".key");
			TokenFile.deleteOnExit();
			FileUtils.write(TokenFile, "  my-test-token-string  \r\n", Charsets.UTF_8);

			AuthenticationToken AuthToken = new AuthenticationToken();
			AuthToken.configure("file://" + TokenFile);
			assertEquals(AuthToken.AuthMethodName, "token");

			AuthenticationDataProvider AuthData = AuthToken.AuthData;
			assertTrue(AuthData.hasDataFromCommand());
			assertEquals(AuthData.CommandData, "my-test-token-string");

			// Ensure if the file content changes, the token will get refreshed as well
			FileUtils.write(TokenFile, "other-token", Charsets.UTF_8);

			AuthenticationDataProvider AuthData2 = AuthToken.AuthData;
			assertTrue(AuthData2.hasDataFromCommand());
			assertEquals(AuthData2.CommandData, "other-token");

			AuthToken.Dispose();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAuthTokenConfigNoPrefix() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void TestAuthTokenConfigNoPrefix()
		{
			AuthenticationToken AuthToken = new AuthenticationToken();
			AuthToken.configure("my-test-token-string");
			assertEquals(AuthToken.AuthMethodName, "token");

			AuthenticationDataProvider AuthData = AuthToken.AuthData;
			assertTrue(AuthData.hasDataFromCommand());
			assertEquals(AuthData.CommandData, "my-test-token-string");
			AuthToken.Dispose();
		}
	}

}