﻿using DotNetty.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharpPulsar.Api;
using SharpPulsar.Utils;

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
namespace SharpPulsar.Impl.Conf
{
	/// <summary>
	/// Utils for loading configuration data.
	/// </summary>
	public sealed class ConfigurationDataUtils
	{

		public static ObjectMapper Create()
		{
			return new ObjectMapper();
		}

        public class FastThreadLocalAnonymousInnerClass : FastThreadLocal<ObjectMapper>
		{
			public ObjectMapper InitialValue()
			{
				return Create();
			}
		}

		public static ObjectMapper ThreadLocal { get; } = new FastThreadLocalAnonymousInnerClass().InitialValue();

        private ConfigurationDataUtils()
		{
		}

		public static T LoadData(IDictionary<string, object> config, T existingData)
		{
			var mapper = ThreadLocal;
			try
			{
                var existingConfigJson = mapper.WriteValueAsString(existingData);
				var existingConfig = (Dictionary<string, object>)mapper.ReadValue(existingConfigJson, typeof(Dictionary<string, object>));
				IDictionary<string, object> newConfig = new Dictionary<string, object>();
				existingConfig.ToList().ForEach(x=> newConfig[x.Key] = x.Value);
				config.ToList().ForEach(x => newConfig[x.Key] = x.Value);
				var configJson = mapper.WriteValueAsString(newConfig);
                var fullName = typeof(T).ToString();
                if (fullName != null)
                {
                    if (fullName.Contains("ProducerConfigurationData"))
                    {
                        return (T)mapper.ReadValue(configJson, typeof(T));
                    }
					else if (fullName.Contains("ClientConfigurationData"))
                    {
                        return (T)mapper.ReadValue(configJson, typeof(T), ClientConfigurationDataOptions(mapper.WriteValueAsString(newConfig["Authentication"]), mapper));
					}
                    else
                    {
						return (T)mapper.ReadValue(configJson, typeof(T));
					}
                }
                throw new NullReferenceException("ConfigurationData is null");
			}
			catch (IOException e)
			{
				throw new Exception("Failed to load config into existing configuration data", e);
			}

		}

        private static JsonSerializerOptions ClientConfigurationDataOptions(string auth, ObjectMapper mapper)
        {
            var authObj = (Dictionary<string, object>)mapper.ReadValue(auth, typeof(Dictionary<string, object>));
            var authMethod = authObj["AuthMethodName"].ToString();

			var iuath = authMethod switch
            {
                "none" => typeof(Auth.AuthenticationDisabled),
                "tls" => typeof(Auth.AuthenticationTls),
                "token" => typeof(Auth.AuthenticationToken),
                "basic" => typeof(Auth.AuthenticationBasic),
                _ => null
            };
            if(iuath == null)
				throw new NullReferenceException("Authentication is null");
			var serializerOptions = new JsonSerializerOptions
            {
                Converters = {
                    new InterfaceConverterFactory(iuath, typeof(IAuthentication))
                },
                ReadCommentHandling = JsonCommentHandling.Skip,
				PropertyNameCaseInsensitive = true
			};
            return serializerOptions;
        }
        private static JsonSerializerOptions ProducerConfigurationDataOptions(IDictionary<string, object> data, ObjectMapper mapper)
        {
            //var customMessageRouter = data["CustomMessageRouter"].GetType();
            var batcherBuilder = data["BatcherBuilder"].GetType();
            //var cryptoKeyReader = data["CryptoKeyReader"].GetType();
            var serializerOptions = new JsonSerializerOptions
            {
                Converters = {
                    //new InterfaceConverterFactory(customMessageRouter, typeof(IMessageRouter)),
                    new InterfaceConverterFactory(batcherBuilder, typeof(IBatcherBuilder)),
                    //new InterfaceConverterFactory(cryptoKeyReader, typeof(ICryptoKeyReader))
                },
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true
            };
            return serializerOptions;
        }
    }
	
}