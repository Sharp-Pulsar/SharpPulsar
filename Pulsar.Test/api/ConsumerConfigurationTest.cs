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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;

	using org.apache.pulsar.client.impl.conf;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;
	using Test = org.testng.annotations.Test;

	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using ObjectWriter = com.fasterxml.jackson.databind.ObjectWriter;
	using SerializationFeature = com.fasterxml.jackson.databind.SerializationFeature;

	/// <summary>
	/// Unit test of <seealso cref="ConsumerConfiguration"/>.
	/// </summary>
	public class ConsumerConfigurationTest
	{

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ConsumerConfigurationTest));

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({ "unchecked", "rawtypes" }) @Test public void testJsonIgnore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void testJsonIgnore()
		{

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<?> conf = new org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<>();
			ConsumerConfigurationData<object> conf = new ConsumerConfigurationData<object>();
			conf.ConsumerEventListener = new ConsumerEventListenerAnonymousInnerClass(this);

			conf.MessageListener = (MessageListener)(consumer, msg) =>
			{
			};

			conf.CryptoKeyReader = mock(typeof(CryptoKeyReader));

			ObjectMapper m = new ObjectMapper();
			m.configure(SerializationFeature.FAIL_ON_EMPTY_BEANS, false);
			ObjectWriter w = m.writerWithDefaultPrettyPrinter();

			string confAsString = w.writeValueAsString(conf);
			log.info("conf : {}", confAsString);

			assertFalse(confAsString.Contains("messageListener"));
			assertFalse(confAsString.Contains("consumerEventListener"));
			assertFalse(confAsString.Contains("cryptoKeyReader"));
		}

		private class ConsumerEventListenerAnonymousInnerClass : ConsumerEventListener
		{
			private readonly ConsumerConfigurationTest outerInstance;

			public ConsumerEventListenerAnonymousInnerClass(ConsumerConfigurationTest outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void becameActive<T1>(Consumer<T1> consumer, int partitionId)
			{
			}

			public override void becameInactive<T1>(Consumer<T1> consumer, int partitionId)
			{
			}
		}

	}

}