﻿using System;

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
namespace SharpPulsar.Test.Api
{
    public class BytesKeyTest : ProducerConsumerBase
	{
		public override void setup()
		{
			base.internalSetup();
			base.producerBaseSetup();
		}

		public override void cleanup()
		{
			base.internalCleanup();
		}

		private void byteKeysTest(bool batching)
		{
			Random r = new Random(0);
			Consumer<string> consumer = pulsarClient.newConsumer(Schema_Fields.STRING).topic("persistent://my-property/my-ns/my-topic1").subscriptionName("my-subscriber-name").subscribe();

			Producer<string> producer = pulsarClient.newProducer(Schema_Fields.STRING).enableBatching(batching).batchingMaxPublishDelay(long.MaxValue, TimeUnit.SECONDS).batchingMaxMessages(int.MaxValue).topic("persistent://my-property/my-ns/my-topic1").create();

			sbyte[] byteKey = new sbyte[1000];
			r.NextBytes(byteKey);
			producer.newMessage().keyBytes(byteKey).value("TestMessage").sendAsync();
			producer.flush();

			Message<string> m = consumer.receive();
			Assert.assertEquals(m.Value, "TestMessage");
			Assert.assertEquals(m.KeyBytes, byteKey);
			Assert.assertTrue(m.hasBase64EncodedKey());
		}

		public virtual void testBytesKeyBatch()
		{
			byteKeysTest(true);
		}

		public virtual void testBytesKeyNoBatch()
		{
			byteKeysTest(false);
		}
	}

}