using SharpPulsar.Exception;
using SharpPulsar.Interface;
using SharpPulsar.Interface.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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

	public static class AuthenticationUtil
	{

		
		public static IDictionary<string, string> ConfigureFromJsonString(string authParamsString)
		{
			var input = new StringReader(authParamsString);
			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(CamelCaseNamingConvention.Instance)
				.Build();
			return deserializer.Deserialize<Dictionary<string, string>>(input);
		}


		public static IDictionary<string, string> ConfigureFromPulsar1AuthParamString(string authParamsString)
		{
			IDictionary<string, string> authParams = new Dictionary<string, string>();

			if (!string.IsNullOrWhiteSpace(authParamsString))
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
		/// <exception cref="PulsarClientException.UnsupportedAuthenticationException"> </exception>

		public static IAuthentication Create(string authPluginClassName, string authParamsString)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(authPluginClassName))
				{
					Type authClass = Type.GetType(authPluginClassName);
					IAuthentication auth = (IAuthentication) Activator.CreateInstance(authClass);
					if (auth is IEncodedAuthenticationParameterSupport)
					{
						// Parse parameters on plugin side.
						((IEncodedAuthenticationParameterSupport) auth).Configure(authParamsString);
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
					return new Auth.AuthenticationDisabled();
				}
			}
			catch (System.Exception t)
			{
				throw new PulsarClientException.UnsupportedAuthenticationException(t.Message);
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
		/// <exception cref="PulsarClientException.UnsupportedAuthenticationException"> </exception>

		public static IAuthentication Create(string authPluginClassName, IDictionary<string, string> authParams)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(authPluginClassName))
				{
					Type authClass = Type.GetType(authPluginClassName);
					IAuthentication auth = (IAuthentication) Activator.CreateInstance(authClass);
					auth.Configure(JsonSerializer.Serialize(authParams));
					return auth;
				}
				else
				{
					return new Auth.AuthenticationDisabled();
				}
			}
			catch (System.Exception t)
			{
				throw new PulsarClientException.UnsupportedAuthenticationException(t.Message);
			}
		}
	}

}