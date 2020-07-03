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

using ProducerConsumerBase = SharpPulsar.Test.Api.ProducerConsumerBase;

namespace SharpPulsar.Test.Impl
{

    public class CompactedOutBatchMessageTest : ProducerConsumerBase
	{

		public override void setup()
		{
			base.internalSetup();
			ProducerBaseSetup();
		}


		public override void cleanup()
		{
			base.internalCleanup();
		}


		public virtual void testCompactedOutMessages()
		{
			const string topic1 = "persistent://my-property/my-ns/my-topic";

			MessageMetadata metadata = MessageMetadata.newBuilder().setProducerName("foobar").setSequenceId(1).setPublishTime(1).setNumMessagesInBatch(3).build();

			// build a buffer with 4 messages, first and last compacted out
			ByteBuf batchBuffer = Unpooled.buffer(1000);
			Commands.serializeSingleMessageInBatchWithPayload(SingleMessageMetadata.newBuilder().setCompactedOut(true).setPartitionKey("key1"), Unpooled.EMPTY_BUFFER, batchBuffer);
			Commands.serializeSingleMessageInBatchWithPayload(SingleMessageMetadata.newBuilder().setCompactedOut(true).setPartitionKey("key2"), Unpooled.EMPTY_BUFFER, batchBuffer);
			Commands.serializeSingleMessageInBatchWithPayload(SingleMessageMetadata.newBuilder().setCompactedOut(false).setPartitionKey("key3"), Unpooled.EMPTY_BUFFER, batchBuffer);
			Commands.serializeSingleMessageInBatchWithPayload(SingleMessageMetadata.newBuilder().setCompactedOut(true).setPartitionKey("key4"), Unpooled.EMPTY_BUFFER, batchBuffer);

			using (ConsumerImpl<sbyte[]> consumer = (ConsumerImpl<sbyte[]>) pulsarClient.newConsumer().topic(topic1).subscriptionName("my-subscriber-name").subscribe())
			{
				// shove it in the sideways
				consumer.receiveIndividualMessagesFromBatch(metadata, 0, null, batchBuffer, MessageIdData.newBuilder().setLedgerId(1234).setEntryId(567).build(), consumer.cnx());

                Message<object> m = consumer.receive();
				assertEquals(((BatchMessageIdImpl)m.MessageId).LedgerId, 1234);
				assertEquals(((BatchMessageIdImpl)m.MessageId).EntryId, 567);
				assertEquals(((BatchMessageIdImpl)m.MessageId).BatchIndex, 2);
				assertEquals(m.Key, "key3");

				assertEquals(consumer.numMessagesInQueue(), 0);
			}
		}
	}

}