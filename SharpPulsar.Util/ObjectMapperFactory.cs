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
namespace SharpPulsar.Util
{
	using Include = com.fasterxml.jackson.annotation.JsonInclude.Include;
	using DeserializationFeature = com.fasterxml.jackson.databind.DeserializationFeature;
	//Install-Package YamlDotNet
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using YAMLFactory = com.fasterxml.jackson.dataformat.yaml.YAMLFactory;

	using FastThreadLocal = io.netty.util.concurrent.FastThreadLocal;
	public class ObjectMapperFactory
	{
		public static ObjectMapper Create()
		{
			ObjectMapper mapper = new ObjectMapper();
			// forward compatibility for the properties may go away in the future
			mapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);
			mapper.configure(DeserializationFeature.READ_UNKNOWN_ENUM_VALUES_AS_NULL, true);
			mapper.SerializationInclusion = Include.NON_NULL;
			return mapper;
		}

		public static ObjectMapper CreateYaml()
		{
			ObjectMapper mapper = new ObjectMapper(new YAMLFactory());
			// forward compatibility for the properties may go away in the future
			mapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);
			mapper.configure(DeserializationFeature.READ_UNKNOWN_ENUM_VALUES_AS_NULL, true);
			mapper.SerializationInclusion = Include.NON_NULL;
			return mapper;
		}

		private static readonly FastThreadLocal<ObjectMapper> JSON_MAPPER = new FastThreadLocalAnonymousInnerClass();

		private class FastThreadLocalAnonymousInnerClass : FastThreadLocal<ObjectMapper>
		{
			protected internal override ObjectMapper InitialValue()
			{
				return Create();
			}
		}

		private static readonly FastThreadLocal<ObjectMapper> YAML_MAPPER = new FastThreadLocalAnonymousInnerClass2();

		private class FastThreadLocalAnonymousInnerClass2 : FastThreadLocal<ObjectMapper>
		{
			protected internal override ObjectMapper InitialValue()
			{
				return CreateYaml();
			}
		}

		public static ObjectMapper ThreadLocal
		{
			get
			{
				return JSON_MAPPER.get();
			}
		}

		public static ObjectMapper ThreadLocalYaml
		{
			get
			{
				return YAML_MAPPER.get();
			}
		}

	}

}