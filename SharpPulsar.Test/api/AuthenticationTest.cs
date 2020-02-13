using System;
using Xunit;

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
namespace SharpPulsar.Test
{
	using MockEncodedAuthenticationParameterSupport = Org.Apache.Pulsar.Client.Impl.Auth.MockEncodedAuthenticationParameterSupport;
	using MockAuthentication = Org.Apache.Pulsar.Client.Impl.Auth.MockAuthentication;

	using Test = org.testng.annotations.Test;
	using Assert = org.testng.Assert;

	public class AuthenticationTest
	{

		public virtual void TestConfigureDefaultFormat()
		{
			try
			{
				MockAuthentication TestAuthentication = (MockAuthentication) AuthenticationFactory.Create("org.apache.pulsar.client.impl.auth.MockAuthentication", "key1:value1,key2:value2");
				Assert.assertEquals(TestAuthentication.AuthParamsMap["key1"], "value1");
				Assert.assertEquals(TestAuthentication.AuthParamsMap["key2"], "value2");
			}
			catch (PulsarClientException.UnsupportedAuthenticationException E)
			{
				Console.WriteLine(E.ToString());
				Console.Write(E.StackTrace);
				Assert.fail();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConfigureWrongFormat()
		public virtual void TestConfigureWrongFormat()
		{
			try
			{
				MockAuthentication TestAuthentication = (MockAuthentication) AuthenticationFactory.Create("org.apache.pulsar.client.impl.auth.MockAuthentication", "foobar");
				Assert.assertTrue(TestAuthentication.AuthParamsMap.Count == 0);
			}
			catch (PulsarClientException.UnsupportedAuthenticationException E)
			{
				Console.WriteLine(E.ToString());
				Console.Write(E.StackTrace);
				Assert.fail();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConfigureNull()
		public virtual void TestConfigureNull()
		{
			try
			{
				MockAuthentication TestAuthentication = (MockAuthentication) AuthenticationFactory.Create("org.apache.pulsar.client.impl.auth.MockAuthentication", (string) null);
				Assert.assertTrue(TestAuthentication.AuthParamsMap.Count == 0);
			}
			catch (PulsarClientException.UnsupportedAuthenticationException E)
			{
				Console.WriteLine(E.ToString());
				Console.Write(E.StackTrace);
				Assert.fail();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConfigureEmpty()
		public virtual void TestConfigureEmpty()
		{
			try
			{
				MockAuthentication TestAuthentication = (MockAuthentication) AuthenticationFactory.Create("org.apache.pulsar.client.impl.auth.MockAuthentication", "");
				Assert.assertTrue(TestAuthentication.AuthParamsMap.Count == 0);
			}
			catch (PulsarClientException.UnsupportedAuthenticationException E)
			{
				Console.WriteLine(E.ToString());
				Console.Write(E.StackTrace);
				Assert.fail();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConfigurePluginSide()
		public virtual void TestConfigurePluginSide()
		{
			try
			{
				MockEncodedAuthenticationParameterSupport TestAuthentication = (MockEncodedAuthenticationParameterSupport) AuthenticationFactory.Create("org.apache.pulsar.client.impl.auth.MockEncodedAuthenticationParameterSupport", "key1:value1;key2:value2");
				Assert.assertEquals(TestAuthentication.AuthParamsMap["key1"], "value1");
				Assert.assertEquals(TestAuthentication.AuthParamsMap["key2"], "value2");
			}
			catch (PulsarClientException.UnsupportedAuthenticationException E)
			{
				Console.WriteLine(E.ToString());
				Console.Write(E.StackTrace);
				Assert.fail();
			}
		}
	}

}