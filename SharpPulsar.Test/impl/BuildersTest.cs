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
namespace Org.Apache.Pulsar.Client.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;

	using MessageId = Org.Apache.Pulsar.Client.Api.MessageId;
	using PulsarClient = Org.Apache.Pulsar.Client.Api.PulsarClient;
	using Org.Apache.Pulsar.Client.Impl.Conf;
	using Test = org.testng.annotations.Test;

	public class BuildersTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clientBuilderTest()
		public virtual void ClientBuilderTest()
		{
			ClientBuilderImpl ClientBuilder = (ClientBuilderImpl) PulsarClient.builder().ioThreads(10).maxNumberOfRejectedRequestPerConnection(200).serviceUrl("pulsar://service:6650");

			assertFalse(ClientBuilder.Conf.UseTls);
			assertEquals(ClientBuilder.Conf.ServiceUrl, "pulsar://service:6650");

			ClientBuilderImpl B2 = (ClientBuilderImpl) ClientBuilder.clone();
			assertNotSame(B2, ClientBuilder);

			B2.serviceUrl("pulsar://other-broker:6650");

			assertEquals(ClientBuilder.Conf.ServiceUrl, "pulsar://service:6650");
			assertEquals(B2.Conf.ServiceUrl, "pulsar://other-broker:6650");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enableTlsTest()
		public virtual void EnableTlsTest()
		{
			ClientBuilderImpl Builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("pulsar://service:6650");
			assertFalse(Builder.Conf.UseTls);
			assertEquals(Builder.Conf.ServiceUrl, "pulsar://service:6650");

			Builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("http://service:6650");
			assertFalse(Builder.Conf.UseTls);
			assertEquals(Builder.Conf.ServiceUrl, "http://service:6650");

			Builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("pulsar+ssl://service:6650");
			assertTrue(Builder.Conf.UseTls);
			assertEquals(Builder.Conf.ServiceUrl, "pulsar+ssl://service:6650");

			Builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("https://service:6650");
			assertTrue(Builder.Conf.UseTls);
			assertEquals(Builder.Conf.ServiceUrl, "https://service:6650");

			Builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("pulsar://service:6650").enableTls(true);
			assertTrue(Builder.Conf.UseTls);
			assertEquals(Builder.Conf.ServiceUrl, "pulsar://service:6650");

			Builder = (ClientBuilderImpl)PulsarClient.builder().serviceUrl("pulsar+ssl://service:6650").enableTls(false);
			assertTrue(Builder.Conf.UseTls);
			assertEquals(Builder.Conf.ServiceUrl, "pulsar+ssl://service:6650");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readerBuilderLoadConfTest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		public virtual void ReaderBuilderLoadConfTest()
		{
			PulsarClient Client = PulsarClient.builder().serviceUrl("pulsar://localhost:6650").build();
			string TopicName = "test_src";
			MessageId MessageId = new MessageIdImpl(1, 2, 3);
			IDictionary<string, object> Config = new Dictionary<string, object>();
			Config["topicName"] = TopicName;
			Config["receiverQueueSize"] = 2000;
			ReaderBuilderImpl<sbyte[]> Builder = (ReaderBuilderImpl<sbyte[]>) Client.newReader().startMessageId(MessageId).loadConf(Config);

			Type Clazz = Builder.GetType();
			System.Reflection.FieldInfo Conf = Clazz.getDeclaredField("conf");
			Conf.Accessible = true;
			object Obj = Conf.get(Builder);
			assertTrue(Obj is ReaderConfigurationData);
			assertEquals(((ReaderConfigurationData) Obj).TopicName, TopicName);
			assertEquals(((ReaderConfigurationData) Obj).StartMessageId, MessageId);
		}
	}

}