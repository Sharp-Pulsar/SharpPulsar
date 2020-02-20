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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SharpPulsar.Api;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Conf;
using Xunit;

namespace SharpPulsar.Test.Impl.auth
{

    public class AuthenticationTokenTest
	{
		[Fact]
		public void TestAuthToken()
		{
			var authToken = new AuthenticationToken("token-xyz");
			Assert.Equal("token", authToken.AuthMethodName);

			var authData = authToken.AuthData;
			Assert.True(authData.HasDataFromCommand());
            Assert.Equal("token-xyz", authData.CommandData);

			Assert.False(authData.HasDataForTls());
			Assert.Null(authData.TlsCertificates);
			Assert.Null(authData.TlsPrivateKey);

			Assert.True(authData.HasDataForHttp());
            Assert.Equal(new HashSet<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("Authorization", "Bearer token-xyz") }, authData.HttpHeaders);

			authToken.DisposeAsync();
		}
		[Fact]
		public  void TestAuthTokenClientConfig()
		{
            var clientConfig = new ClientConfigurationData
            {
                ServiceUrl = "pulsar://localhost:6650",
                AuthPluginClassName = typeof(AuthenticationToken).FullName,
                AuthParams = "token-xyz"
            };

            var pulsarClient = new PulsarClientImpl(clientConfig);

			var authToken = pulsarClient.Configuration.Authentication;
			Assert.Equal("token", authToken.AuthMethodName);

			var authData = authToken.AuthData;
			Assert.True(authData.HasDataFromCommand());
			Assert.Equal("token-xyz", authData.CommandData);

			Assert.False(authData.HasDataForTls());
			Assert.Null(authData.TlsCertificates);
			Assert.Null(authData.TlsPrivateKey);

			Assert.True(authData.HasDataForHttp());
            Assert.Equal(new HashSet<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("Authorization", "Bearer token-xyz") }, authData.HttpHeaders);

			authToken.Dispose();
		}
		[Fact]
		public void TestAuthTokenConfig()
		{
			var authToken = new AuthenticationToken();
			authToken.Configure("token:my-test-token-string");
			Assert.Equal("token", authToken.AuthMethodName);

			var authData = authToken.AuthData;
			Assert.True(authData.HasDataFromCommand());
			Assert.Equal("my-test-token-string", authData.CommandData);
			authToken.Dispose();
		}
		[Fact]
		public void TestAuthTokenConfigFromFile()
		{
            var tokenFile = Path.GetTempPath() + Guid.NewGuid() + "pular-test-token.key";
            using (var fs = new FileStream(tokenFile, FileMode.OpenOrCreate))
            {
                var info = new UTF8Encoding(true).GetBytes("my-test-token-string");
                fs.Write(info, 0, info.Length);

                var authToken = new AuthenticationToken();
                authToken.Configure("file://" + tokenFile);
                Assert.Equal("token", authToken.AuthMethodName);

                var authData = authToken.AuthData;
                Assert.True(authData.HasDataFromCommand());
                Assert.Equal("my-test-token-string", authData.CommandData);

				// Ensure if the file content changes, the token will get refreshed as well
				info = new UTF8Encoding(true).GetBytes("other-token");
                fs.Write(info, 0, info.Length);

                var authData2 = authToken.AuthData;
                Assert.True(authData2.HasDataFromCommand());
                Assert.Equal("other-token", authData2.CommandData);

                authToken.Dispose();
			}

            File.Delete(tokenFile);
			
		}

		/// <summary>
		/// File can have spaces and newlines before or after the token. We should be able to read
		/// the token correctly anyway.
		/// </summary>
		[Fact]
		public  void TestAuthTokenConfigFromFileWithNewline()
		{
            var tokenFile = Path.GetTempPath() + Guid.NewGuid() + "pular-test-token.key";
            using (var fs = new FileStream(tokenFile, FileMode.OpenOrCreate))
            {
                var info = new UTF8Encoding(true).GetBytes("  my-test-token-string  \r\n");
                fs.Write(info, 0, info.Length);

                var authToken = new AuthenticationToken();
                authToken.Configure("file://" + tokenFile);
                Assert.Equal("token", authToken.AuthMethodName);

                var authData = authToken.AuthData;
                Assert.True(authData.HasDataFromCommand());
                Assert.Equal("my-test-token-string", authData.CommandData);

				// Ensure if the file content changes, the token will get refreshed as well
				info = new UTF8Encoding(true).GetBytes("other-token");
                fs.Write(info, 0, info.Length);

				var authData2 = authToken.AuthData;
                Assert.True(authData2.HasDataFromCommand());
                Assert.Equal("other-token", authData2.CommandData);

                authToken.Dispose();
			}
            File.Delete(tokenFile);
		}

		[Fact]
		public void TestAuthTokenConfigNoPrefix()
		{
			var authToken = new AuthenticationToken();
			authToken.Configure("my-test-token-string");
			Assert.Equal("token", authToken.AuthMethodName);

			var authData = authToken.AuthData;
			Assert.True(authData.HasDataFromCommand());
			Assert.Equal("my-test-token-string", authData.CommandData);
			authToken.Dispose();
		}
	}

}