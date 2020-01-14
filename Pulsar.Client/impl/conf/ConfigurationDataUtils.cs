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
namespace org.apache.pulsar.client.impl.conf
{
	using Include = com.fasterxml.jackson.annotation.JsonInclude.Include;
	using DeserializationFeature = com.fasterxml.jackson.databind.DeserializationFeature;
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using Maps = com.google.common.collect.Maps;
	using FastThreadLocal = io.netty.util.concurrent.FastThreadLocal;

	/// <summary>
	/// Utils for loading configuration data.
	/// </summary>
	public sealed class ConfigurationDataUtils
	{

		public static ObjectMapper create()
		{
			ObjectMapper mapper = new ObjectMapper();
			// forward compatibility for the properties may go away in the future
			mapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, true);
			mapper.configure(DeserializationFeature.READ_UNKNOWN_ENUM_VALUES_AS_NULL, false);
			mapper.SerializationInclusion = Include.NON_NULL;
			return mapper;
		}

		private static readonly FastThreadLocal<ObjectMapper> mapper = new FastThreadLocalAnonymousInnerClass();

		private class FastThreadLocalAnonymousInnerClass : FastThreadLocal<ObjectMapper>
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override protected com.fasterxml.jackson.databind.ObjectMapper initialValue() throws Exception
			protected internal override ObjectMapper initialValue()
			{
				return create();
			}
		}

		public static ObjectMapper ThreadLocal
		{
			get
			{
				return mapper.get();
			}
		}

		private ConfigurationDataUtils()
		{
		}

		public static T loadData<T>(IDictionary<string, object> config, T existingData, Type dataCls)
		{
			ObjectMapper mapper = ThreadLocal;
			try
			{
				string existingConfigJson = mapper.writeValueAsString(existingData);
				IDictionary<string, object> existingConfig = mapper.readValue(existingConfigJson, typeof(System.Collections.IDictionary));
				IDictionary<string, object> newConfig = Maps.newHashMap();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
				newConfig.putAll(existingConfig);
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
				newConfig.putAll(config);
				string configJson = mapper.writeValueAsString(newConfig);
				return mapper.readValue(configJson, dataCls);
			}
			catch (IOException e)
			{
				throw new Exception("Failed to load config into existing configuration data", e);
			}

		}

	}


}