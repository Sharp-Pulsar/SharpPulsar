using System;
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
//	import static org.apache.commons.lang3.StringUtils.isNotBlank;

	using TypeReference = com.fasterxml.jackson.core.type.TypeReference;
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;


	using Authentication = org.apache.pulsar.client.api.Authentication;
	using EncodedAuthenticationParameterSupport = org.apache.pulsar.client.api.EncodedAuthenticationParameterSupport;
	using UnsupportedAuthenticationException = org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException;
	using AuthenticationDisabled = org.apache.pulsar.client.impl.auth.AuthenticationDisabled;
	using ObjectMapperFactory = org.apache.pulsar.common.util.ObjectMapperFactory;

	public class AuthenticationUtil
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.Map<String, String> configureFromJsonString(String authParamsString) throws java.io.IOException
		public static IDictionary<string, string> configureFromJsonString(string authParamsString)
		{
			ObjectMapper jsonMapper = ObjectMapperFactory.create();
			return jsonMapper.readValue(authParamsString, new TypeReferenceAnonymousInnerClass());
		}

		private class TypeReferenceAnonymousInnerClass : TypeReference<Dictionary<string, string>>
		{
		}

		public static IDictionary<string, string> configureFromPulsar1AuthParamString(string authParamsString)
		{
			IDictionary<string, string> authParams = new Dictionary<string, string>();

			if (isNotBlank(authParamsString))
			{
				string[] @params = authParamsString.Split(",", true);
				foreach (string p in @params)
				{
					string[] kv = p.Split(":", true);
					if (kv.Length == 2)
					{
						authParams[kv[0]] = kv[1];
					}
				}
			}
			return authParams;
		}

		/// <summary>
		/// Create an instance of the Authentication-Plugin
		/// </summary>
		/// <param name="authPluginClassName">
		///            name of the Authentication-Plugin you want to use </param>
		/// <param name="authParamsString">
		///            string which represents parameters for the Authentication-Plugin, e.g., "key1:val1,key2:val2" </param>
		/// <returns> instance of the Authentication-Plugin </returns>
		/// <exception cref="UnsupportedAuthenticationException"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") public static final org.apache.pulsar.client.api.Authentication create(String authPluginClassName, String authParamsString) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public static Authentication create(string authPluginClassName, string authParamsString)
		{
			try
			{
				if (isNotBlank(authPluginClassName))
				{
					Type authClass = Type.GetType(authPluginClassName);
					Authentication auth = (Authentication) System.Activator.CreateInstance(authClass);
					if (auth is EncodedAuthenticationParameterSupport)
					{
						// Parse parameters on plugin side.
						((EncodedAuthenticationParameterSupport) auth).configure(authParamsString);
					}
					else
					{
						// Parse parameters by default parse logic.
						auth.configure(configureFromPulsar1AuthParamString(authParamsString));
					}
					return auth;
				}
				else
				{
					return new AuthenticationDisabled();
				}
			}
			catch (Exception t)
			{
				throw new UnsupportedAuthenticationException(t);
			}
		}

		/// <summary>
		/// Create an instance of the Authentication-Plugin
		/// </summary>
		/// <param name="authPluginClassName">
		///            name of the Authentication-Plugin you want to use </param>
		/// <param name="authParams">
		///            map which represents parameters for the Authentication-Plugin </param>
		/// <returns> instance of the Authentication-Plugin </returns>
		/// <exception cref="UnsupportedAuthenticationException"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") public static final org.apache.pulsar.client.api.Authentication create(String authPluginClassName, java.util.Map<String, String> authParams) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public static Authentication create(string authPluginClassName, IDictionary<string, string> authParams)
		{
			try
			{
				if (isNotBlank(authPluginClassName))
				{
					Type authClass = Type.GetType(authPluginClassName);
					Authentication auth = (Authentication) System.Activator.CreateInstance(authClass);
					auth.configure(authParams);
					return auth;
				}
				else
				{
					return new AuthenticationDisabled();
				}
			}
			catch (Exception t)
			{
				throw new UnsupportedAuthenticationException(t);
			}
		}
	}

}