using SharpPulsar.Api;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Conf;
using System;
using System.Collections.Generic;
using System.Text.Json;
using static SharpPulsar.Exception.PulsarClientException;

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

	public class AuthenticationUtil
	{
		public static IDictionary<string, string> ConfigureFromJsonString(string authParamsString)
		{
			ObjectMapper jsonMapper = new ObjectMapper();
			return (IDictionary<string, string>)jsonMapper.ReadValue(authParamsString, typeof(TypeReferenceAnonymousInnerClass));
		}

		public class TypeReferenceAnonymousInnerClass : Dictionary<string, string>
		{
		}

		public static IDictionary<string, string> ConfigureFromPulsar1AuthParamString(string authParamsString)
		{
			IDictionary<string, string> authParams = new Dictionary<string, string>();

			if (!string.IsNullOrWhiteSpace(authParamsString))
			{
				string[] @params = authParamsString.Split(',');
				foreach (var p in @params)
				{
					string[] kv = p.Split(':');
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
		public static IAuthentication Create(string authPluginClassName, string authParamsString)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(authPluginClassName))
				{
					Type authClass = Type.GetType(authPluginClassName);
					var auth = (IAuthentication) Activator.CreateInstance(authClass);
					if (auth is EncodedAuthenticationParameterSupport)
					{
						// Parse parameters on plugin side.
						((EncodedAuthenticationParameterSupport) auth).Configure(authParamsString);
					}
					else
					{
						// Parse parameters by default parse logic.
						//auth.Configure(ConfigureFromPulsar1AuthParamString(authParamsString));[Deprecated]
						auth.Configure(authParamsString);
					}
					return auth;
				}
				else
				{
					return new AuthenticationDisabled();
				}
			}
			catch (System.Exception t)
			{
				throw new UnsupportedAuthenticationException(t.Message);
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
		/// 
		public static IAuthentication Create(string authPluginClassName, IDictionary<string, string> authParams)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(authPluginClassName))
				{
					Type AuthClass = Type.GetType(authPluginClassName);
					var auth = (IAuthentication) Activator.CreateInstance(AuthClass);
					auth.Configure(JsonSerializer.Serialize(authParams));
					return auth;
				}
				else
				{
					return new AuthenticationDisabled();
				}
			}
			catch (System.Exception t)
			{
				throw new UnsupportedAuthenticationException(t.Message);
			}
		}
	}

}