using System;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Test.Impl.auth;
using Xunit;
using Xunit.Abstractions;

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
namespace SharpPulsar.Test.Api
{
	using MockEncodedAuthenticationParameterSupport = MockEncodedAuthenticationParameterSupport;
	using MockAuthentication = MockAuthentication;

    public class AuthenticationTest
	{
        private readonly ITestOutputHelper _testOutputHelper;

        public AuthenticationTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
		public void TestConfigureDefaultFormat()
		{
			try
			{
				MockAuthentication testAuthentication = (MockAuthentication) AuthenticationFactory.Create("org.apache.pulsar.client.impl.auth.MockAuthentication", "key1:value1,key2:value2");
				Assert.Equal("value1", testAuthentication.AuthParamsMap["key1"]);
				Assert.Equal("value2", testAuthentication.AuthParamsMap["key2"]);
			}
			catch (PulsarClientException.UnsupportedAuthenticationException e)
			{
				_testOutputHelper.WriteLine(e.ToString());
                _testOutputHelper.WriteLine(e.StackTrace);
                Assert.True(false, e.Message);
            }
		}

		[Fact]
		public void TestConfigureWrongFormat()
		{
			try
			{
				MockAuthentication testAuthentication = (MockAuthentication) AuthenticationFactory.Create("org.apache.pulsar.client.impl.auth.MockAuthentication", "foobar");
				Assert.True(testAuthentication.AuthParamsMap.Count == 0);
			}
			catch (PulsarClientException.UnsupportedAuthenticationException e)
			{
                _testOutputHelper.WriteLine(e.ToString());
                _testOutputHelper.WriteLine(e.StackTrace);
                Assert.True(false, "TestConfigureWrongFormat failed");
			}
		}

		[Fact]
		public  void TestConfigureNull()
		{
			try
			{
				MockAuthentication testAuthentication = (MockAuthentication) AuthenticationFactory.Create("org.apache.pulsar.client.impl.auth.MockAuthentication", (string) null);
				Assert.True(testAuthentication.AuthParamsMap.Count == 0);
			}
			catch (PulsarClientException.UnsupportedAuthenticationException e)
			{
				_testOutputHelper.WriteLine(e.ToString());
				_testOutputHelper.WriteLine(e.StackTrace);
                Assert.True(false, e.Message);
			}
		}
		[Fact]
		public void TestConfigureEmpty()
		{
			try
			{
				MockAuthentication testAuthentication = (MockAuthentication) AuthenticationFactory.Create("org.apache.pulsar.client.impl.auth.MockAuthentication", "");
				Assert.True(testAuthentication.AuthParamsMap.Count == 0);
			}
			catch (PulsarClientException.UnsupportedAuthenticationException e)
			{
				_testOutputHelper.WriteLine(e.ToString());
				_testOutputHelper.WriteLine(e.StackTrace);
                Assert.True(false, e.Message);
			}
		}
		[Fact]
		public void TestConfigurePluginSide()
		{
			try
			{
				MockEncodedAuthenticationParameterSupport testAuthentication = (MockEncodedAuthenticationParameterSupport) AuthenticationFactory.Create("org.apache.pulsar.client.impl.auth.MockEncodedAuthenticationParameterSupport", "key1:value1;key2:value2");
				Assert.Equal("value1", testAuthentication.AuthParamsMap["key1"]);
				Assert.Equal("value2", testAuthentication.AuthParamsMap["key2"]);
			}
			catch (PulsarClientException.UnsupportedAuthenticationException e)
			{
				_testOutputHelper.WriteLine(e.ToString());
				_testOutputHelper.WriteLine(e.StackTrace);
                Assert.True(false, e.Message);
			}
		}
	}

}