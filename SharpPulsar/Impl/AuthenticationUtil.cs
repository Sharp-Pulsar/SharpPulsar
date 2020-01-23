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
namespace SharpPulsar.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.StringUtils.isNotBlank;

	using TypeReference = com.fasterxml.jackson.core.type.TypeReference;
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;


	using Authentication = SharpPulsar.Api.Authentication;
	using EncodedAuthenticationParameterSupport = SharpPulsar.Api.EncodedAuthenticationParameterSupport;
	using UnsupportedAuthenticationException = SharpPulsar.Api.PulsarClientException.UnsupportedAuthenticationException;
	using AuthenticationDisabled = SharpPulsar.Impl.Auth.AuthenticationDisabled;
	using ObjectMapperFactory = Org.Apache.Pulsar.Common.Util.ObjectMapperFactory;

	public class AuthenticationUtil
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.Map<String, String> configureFromJsonString(String authParamsString) throws java.io.IOException
		public static IDictionary<string, string> ConfigureFromJsonString(string AuthParamsString)
		{
			ObjectMapper JsonMapper = ObjectMapperFactory.create();
			return JsonMapper.readValue(AuthParamsString, new TypeReferenceAnonymousInnerClass());
		}

		public class TypeReferenceAnonymousInnerClass : TypeReference<Dictionary<string, string>>
		{
		}

		public static IDictionary<string, string> ConfigureFromPulsar1AuthParamString(string AuthParamsString)
		{
			IDictionary<string, string> AuthParams = new Dictionary<string, string>();

			if (isNotBlank(AuthParamsString))
			{
				string[] Params = AuthParamsString.Split(",", true);
				foreach (string P in Params)
				{
					string[] Kv = P.Split(":", true);
					if (Kv.Length == 2)
					{
						AuthParams[Kv[0]] = Kv[1];
					}
				}
			}
			return AuthParams;
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
//ORIGINAL LINE: @SuppressWarnings("deprecation") public static final SharpPulsar.api.Authentication create(String authPluginClassName, String authParamsString) throws SharpPulsar.api.PulsarClientException.UnsupportedAuthenticationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public static Authentication Create(string AuthPluginClassName, string AuthParamsString)
		{
			try
			{
				if (isNotBlank(AuthPluginClassName))
				{
					Type AuthClass = Type.GetType(AuthPluginClassName);
					Authentication Auth = (Authentication) System.Activator.CreateInstance(AuthClass);
					if (Auth is EncodedAuthenticationParameterSupport)
					{
						// Parse parameters on plugin side.
						((EncodedAuthenticationParameterSupport) Auth).configure(AuthParamsString);
					}
					else
					{
						// Parse parameters by default parse logic.
						Auth.configure(ConfigureFromPulsar1AuthParamString(AuthParamsString));
					}
					return Auth;
				}
				else
				{
					return new AuthenticationDisabled();
				}
			}
			catch (Exception T)
			{
				throw new UnsupportedAuthenticationException(T);
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
//ORIGINAL LINE: @SuppressWarnings("deprecation") public static final SharpPulsar.api.Authentication create(String authPluginClassName, java.util.Map<String, String> authParams) throws SharpPulsar.api.PulsarClientException.UnsupportedAuthenticationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public static Authentication Create(string AuthPluginClassName, IDictionary<string, string> AuthParams)
		{
			try
			{
				if (isNotBlank(AuthPluginClassName))
				{
					Type AuthClass = Type.GetType(AuthPluginClassName);
					Authentication Auth = (Authentication) System.Activator.CreateInstance(AuthClass);
					Auth.configure(AuthParams);
					return Auth;
				}
				else
				{
					return new AuthenticationDisabled();
				}
			}
			catch (Exception T)
			{
				throw new UnsupportedAuthenticationException(T);
			}
		}
	}

}