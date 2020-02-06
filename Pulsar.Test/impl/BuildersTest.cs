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
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;

	using MessageId = api.MessageId;
	using PulsarClient = api.PulsarClient;
	using org.apache.pulsar.client.impl.conf;
	using Test = org.testng.annotations.Test;

	public class BuildersTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clientBuilderTest()
		public virtual void clientBuilderTest()
		{
			ClientBuilderImpl clientBuilder = (ClientBuilderImpl) PulsarClient.builder().ioThreads(10).maxNumberOfRejectedRequestPerConnection(200).serviceUrl("pulsar://service:6650");

			assertFalse(clientBuilder.conf.UseTls);
			assertEquals(clientBuilder.conf.ServiceUrl, "pulsar://service:6650");

			ClientBuilderImpl b2 = (ClientBuilderImpl) clientBuilder.clone();
			assertNotSame(b2, clientBuilder);

			b2.serviceUrl("pulsar://other-broker:6650");

			assertEquals(clientBuilder.conf.ServiceUrl, "pulsar://service:6650");
			assertEquals(b2.conf.ServiceUrl, "pulsar://other-broker:6650");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableTlsTest()
		public virtual void enableTlsTest()
		{
			ClientBuilderImpl builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("pulsar://service:6650");
			assertFalse(builder.conf.UseTls);
			assertEquals(builder.conf.ServiceUrl, "pulsar://service:6650");

			builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("http://service:6650");
			assertFalse(builder.conf.UseTls);
			assertEquals(builder.conf.ServiceUrl, "http://service:6650");

			builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("pulsar+ssl://service:6650");
			assertTrue(builder.conf.UseTls);
			assertEquals(builder.conf.ServiceUrl, "pulsar+ssl://service:6650");

			builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("https://service:6650");
			assertTrue(builder.conf.UseTls);
			assertEquals(builder.conf.ServiceUrl, "https://service:6650");

			builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("pulsar://service:6650").enableTls(true);
			assertTrue(builder.conf.UseTls);
			assertEquals(builder.conf.ServiceUrl, "pulsar://service:6650");

			builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("pulsar+ssl://service:6650").enableTls(false);
			assertTrue(builder.conf.UseTls);
			assertEquals(builder.conf.ServiceUrl, "pulsar+ssl://service:6650");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readerBuilderLoadConfTest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void readerBuilderLoadConfTest()
		{
			PulsarClient client = PulsarClient.builder().serviceUrl("pulsar://localhost:6650").build();
			string topicName = "test_src";
			MessageId messageId = new MessageIdImpl(1, 2, 3);
			IDictionary<string, object> config = new Dictionary<string, object>();
			config["topicName"] = topicName;
			config["receiverQueueSize"] = 2000;
			ReaderBuilderImpl<sbyte[]> builder = (ReaderBuilderImpl<sbyte[]>) client.newReader().startMessageId(messageId).loadConf(config);

			Type clazz = builder.GetType();
			System.Reflection.FieldInfo conf = clazz.getDeclaredField("conf");
			conf.Accessible = true;
			object obj = conf.get(builder);
			assertTrue(obj is ReaderConfigurationData);
			assertEquals(((ReaderConfigurationData) obj).TopicName, topicName);
			assertEquals(((ReaderConfigurationData) obj).StartMessageId, messageId);
		}
	}

}