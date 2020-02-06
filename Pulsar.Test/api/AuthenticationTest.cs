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
namespace org.apache.pulsar.client.api
{
	using MockEncodedAuthenticationParameterSupport = impl.auth.MockEncodedAuthenticationParameterSupport;
	using MockAuthentication = impl.auth.MockAuthentication;

	using Test = org.testng.annotations.Test;
	using Assert = org.testng.Assert;

	public class AuthenticationTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConfigureDefaultFormat()
		public virtual void testConfigureDefaultFormat()
		{
			try
			{
				MockAuthentication testAuthentication = (MockAuthentication) AuthenticationFactory.create("org.apache.pulsar.client.impl.auth.MockAuthentication", "key1:value1,key2:value2");
				Assert.assertEquals(testAuthentication.authParamsMap["key1"], "value1");
				Assert.assertEquals(testAuthentication.authParamsMap["key2"], "value2");
			}
			catch (PulsarClientException.UnsupportedAuthenticationException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Assert.fail();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConfigureWrongFormat()
		public virtual void testConfigureWrongFormat()
		{
			try
			{
				MockAuthentication testAuthentication = (MockAuthentication) AuthenticationFactory.create("org.apache.pulsar.client.impl.auth.MockAuthentication", "foobar");
				Assert.assertTrue(testAuthentication.authParamsMap.Count == 0);
			}
			catch (PulsarClientException.UnsupportedAuthenticationException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Assert.fail();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConfigureNull()
		public virtual void testConfigureNull()
		{
			try
			{
				MockAuthentication testAuthentication = (MockAuthentication) AuthenticationFactory.create("org.apache.pulsar.client.impl.auth.MockAuthentication", (string) null);
				Assert.assertTrue(testAuthentication.authParamsMap.Count == 0);
			}
			catch (PulsarClientException.UnsupportedAuthenticationException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Assert.fail();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConfigureEmpty()
		public virtual void testConfigureEmpty()
		{
			try
			{
				MockAuthentication testAuthentication = (MockAuthentication) AuthenticationFactory.create("org.apache.pulsar.client.impl.auth.MockAuthentication", "");
				Assert.assertTrue(testAuthentication.authParamsMap.Count == 0);
			}
			catch (PulsarClientException.UnsupportedAuthenticationException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Assert.fail();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConfigurePluginSide()
		public virtual void testConfigurePluginSide()
		{
			try
			{
				MockEncodedAuthenticationParameterSupport testAuthentication = (MockEncodedAuthenticationParameterSupport) AuthenticationFactory.create("org.apache.pulsar.client.impl.auth.MockEncodedAuthenticationParameterSupport", "key1:value1;key2:value2");
				Assert.assertEquals(testAuthentication.authParamsMap["key1"], "value1");
				Assert.assertEquals(testAuthentication.authParamsMap["key2"], "value2");
			}
			catch (PulsarClientException.UnsupportedAuthenticationException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Assert.fail();
			}
		}
	}

}