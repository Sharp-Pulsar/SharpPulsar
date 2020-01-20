using DotNetty.Buffers;
using Google.Protobuf;
using Optional;
using SharpPulsar.Common.Protocol.Schema;
using SharpPulsar.Common.Schema;
using SharpPulsar.Entity;
using SharpPulsar.Util.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
namespace SharpPulsar.Common.Protocol
{
	using static SharpPulsar.Common.Proto.Api.PulsarApi;

	public class Commands
	{

		// default message size for transfer
		public const int DEFAULT_MAX_MESSAGE_SIZE = 5 * 1024 * 1024;
		public const int MESSAGE_SIZE_FRAME_PADDING = 10 * 1024;
		public const int INVALID_MAX_MESSAGE_SIZE = -1;
		public const short magicCrc32c = 0x0e01;
		private const int checksumSize = 4;	
		
		public static bool HasChecksum(IByteBuffer buffer)
		{
			return buffer.GetShort(buffer.ReaderIndex) == magicCrc32c;
		}

		/// <summary>
		/// Read the checksum and advance the reader index in the buffer.
		/// 
		/// <para>Note: This method assume the checksum presence was already verified before.
		/// </para>
		/// </summary>
		public static int ReadChecksum(IByteBuffer buffer)
		{
			buffer.SkipBytes(2); //skip magic bytes
			return buffer.ReadInt();
		}

		public static void SkipChecksumIfPresent(IByteBuffer buffer)
		{
			if (HasChecksum(buffer))
			{
				ReadChecksum(buffer);
			}
		}	

		
		public static IByteBuffer NewCloseConsumer(long consumerId, long requestId)
		{
			CommandCloseConsumer closeConsumerBuilder = new CommandCloseConsumer
			{
				ConsumerId = (ulong)consumerId,
				RequestId = (ulong)requestId
			};

			CommandCloseConsumer closeConsumer = closeConsumerBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.CloseConsumer, CloseConsumer = (closeConsumer) });
			closeConsumerBuilder.recycle();
			closeConsumer.recycle();
			return res;
		}

		public static IByteBuffer NewReachedEndOfTopic(long consumerId)
		{
			CommandReachedEndOfTopic reachedEndOfTopicBuilder = new CommandReachedEndOfTopic
			{
				ConsumerId = (ulong)consumerId
			};

			PulsarApi.CommandReachedEndOfTopic reachedEndOfTopic = reachedEndOfTopicBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.ReachedEndOfTopic, reachedEndOfTopic = (reachedEndOfTopic) });
			reachedEndOfTopicBuilder.recycle();
			reachedEndOfTopic.recycle();
			return res;
		}

		public static IByteBuffer NewCloseProducer(long producerId, long requestId)
		{
			CommandCloseProducer closeProducerBuilder = new CommandCloseProducer
			{
				ProducerId = (ulong)producerId,
				RequestId = (ulong)requestId
			};
			CommandCloseProducer closeProducer = closeProducerBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.CloseProducer, CloseProducer = (closeProducerBuilder) });
			closeProducerBuilder.recycle();
			closeProducer.recycle();
			return res;
		}
		public static IByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, IDictionary<string, string> metadata)
		{
			return NewProducer(topic, producerId, requestId, producerName, false, metadata);
		}

		public static IByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata)
		{
			return newProducer(topic, producerId, requestId, producerName, encrypted, metadata, null, 0, false);
		}

		private static PulsarApi.Schema.Type GetSchemaType(SchemaType type)
		{
			if (type.Value < 0)
			{
				return PulsarApi.Schema.Type.None;
			}
			else
			{
				return PulsarApi.Schema.Type.valueOf(type.Value);
			}
		}

		public static SchemaType GetSchemaType(Proto.Schema.Type type)
		{
			if (type.Number < 0)
			{
				// this is unexpected
				return SchemaType.NONE;
			}
			else
			{
				return SchemaType.valueOf(type.Number);
			}
		}

		private static PulsarApi.Schema GetSchema(SchemaInfo schemaInfo)
		{
			PulsarApi.Schema builder = new PulsarApi.Schema
			{
				Name = (schemaInfo.Name),
				SchemaData = ByteString.CopyFrom((byte[])(object)schemaInfo.Schema).ToByteArray(),
				type = GetSchemaType(schemaInfo.Type),
				//[addAllProperties = schemaInfo.Properties.Select(entry => new KeyValue { Key = entry.Key, Value = (entry.Value) })

			};
			PulsarApi.Schema schema = builder;
			builder.recycle();
			return schema;
		}

		public static IByteBuffer NewProducer(string topic, long producerId, long requestId, string producerName, bool encrypted, IDictionary<string, string> metadata, SchemaInfo schemaInfo, long epoch, bool userProvidedProducerName)
		{
			CommandProducer producerBuilder = new CommandProducer
			{
				Topic = (topic),
				ProducerId = (ulong)producerId,
				RequestId = (ulong)requestId,
				Epoch = (ulong)epoch
			};
			if (!string.ReferenceEquals(producerName, null))
			{
				producerBuilder.ProducerName = (producerName);
			}
			producerBuilder.UserProvidedProducerName = userProvidedProducerName;
			producerBuilder.Encrypted = encrypted;

			producerBuilder.Metadatas = (CommandUtils.ToKeyValueList(metadata));

			if (null != schemaInfo)
			{
				producerBuilder.Schema = GetSchema(schemaInfo);
			}

			CommandProducer producer = producerBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.Producer, Producer = (producer) });
			producerBuilder.recycle();
			producer.recycle();
			return res;
		}

		public static IByteBuffer NewPartitionMetadataResponse(ServerError error, string errorMsg, long requestId)
		{
			CommandPartitionedTopicMetadataResponse partitionMetadataResponseBuilder = new CommandPartitionedTopicMetadataResponse
			{
				RequestId = (ulong)requestId,
				Error = error,
				Response = CommandPartitionedTopicMetadataResponse.LookupType.Failed
			};
			if (!string.ReferenceEquals(errorMsg, null))
			{
				partitionMetadataResponseBuilder.Message = (errorMsg);
			}

			CommandPartitionedTopicMetadataResponse partitionMetadataResponse = partitionMetadataResponseBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.PartitionedMetadataResponse, partitionMetadataResponse = (partitionMetadataResponse) });
			partitionMetadataResponseBuilder.recycle();
			partitionMetadataResponse.recycle();
			return res;
		}

		public static IByteBuffer NewPartitionMetadataRequest(string topic, long requestId)
		{
			CommandPartitionedTopicMetadata partitionMetadataBuilder = new CommandPartitionedTopicMetadata();
			partitionMetadataBuilder.Topic = (topic);
			partitionMetadataBuilder.RequestId = (ulong)requestId;

			CommandPartitionedTopicMetadata partitionMetadata = partitionMetadataBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.PartitionedMetadata, partitionMetadata = (partitionMetadata) });
			partitionMetadataBuilder.recycle();
			partitionMetadata.recycle();
			return res;
		}

		public static IByteBuffer NewPartitionMetadataResponse(int partitions, long requestId)
		{
			CommandPartitionedTopicMetadataResponse partitionMetadataResponseBuilder = new CommandPartitionedTopicMetadataResponse();
			partitionMetadataResponseBuilder.Partitions = (uint)partitions;
			partitionMetadataResponseBuilder.Response = CommandPartitionedTopicMetadataResponse.LookupType.Success;
			partitionMetadataResponseBuilder.RequestId = (ulong)requestId;

			CommandPartitionedTopicMetadataResponse partitionMetadataResponse = partitionMetadataResponseBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.PartitionedMetadataResponse, partitionMetadataResponse = (partitionMetadataResponse) });
			partitionMetadataResponseBuilder.recycle();
			partitionMetadataResponse.recycle();
			return res;
		}

		public static IByteBuffer NewLookup(string topic, bool authoritative, long requestId)
		{
			CommandLookupTopic lookupTopicBuilder = new CommandLookupTopic
			{
				Topic = (topic),
				RequestId = (ulong)requestId,
				Authoritative = authoritative
			};

			CommandLookupTopic lookupBroker = lookupTopicBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.Lookup, lookupTopic = (lookupBroker) });
			lookupTopicBuilder.recycle();
			lookupBroker.recycle();
			return res;
		}

		public static IByteBuffer NewLookupResponse(string brokerServiceUrl, string brokerServiceUrlTls, bool authoritative, PulsarApi.CommandLookupTopicResponse.LookupType response, long requestId, bool proxyThroughServiceUrl)
		{
			CommandLookupTopicResponse commandLookupTopicResponseBuilder = new CommandLookupTopicResponse
			{
				brokerServiceUrl = brokerServiceUrl
			};
			if (!string.ReferenceEquals(brokerServiceUrlTls, null))
			{
				commandLookupTopicResponseBuilder.brokerServiceUrlTls = brokerServiceUrlTls;
			}
			commandLookupTopicResponseBuilder.Response = response;
			commandLookupTopicResponseBuilder.RequestId = (ulong)requestId;
			commandLookupTopicResponseBuilder.Authoritative = authoritative;
			commandLookupTopicResponseBuilder.ProxyThroughServiceUrl = proxyThroughServiceUrl;

			CommandLookupTopicResponse commandLookupTopicResponse = commandLookupTopicResponseBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.LookupResponse, lookupTopicResponse = (commandLookupTopicResponse) });
			commandLookupTopicResponseBuilder.recycle();
			commandLookupTopicResponse.recycle();
			return res;
		}

		public static IByteBuffer NewLookupErrorResponse(ServerError error, string errorMsg, long requestId)
		{
			CommandLookupTopicResponse connectionBuilder = new CommandLookupTopicResponse();
			connectionBuilder.RequestId = (ulong)requestId;
			connectionBuilder.Error = error;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				connectionBuilder.Message = (errorMsg);
			}
			connectionBuilder.Response = CommandLookupTopicResponse.LookupType.Failed;

			CommandLookupTopicResponse connectionBroker = connectionBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.LookupResponse, lookupTopicResponse = (connectionBroker) });
			connectionBuilder.recycle();
			connectionBroker.recycle();
			return res;
		}

		public static IByteBuffer NewMultiMessageAck(long consumerId, IList<KeyValuePair<long, long>> entries)
		{
			CommandAck ackBuilder = new CommandAck();
			ackBuilder.ConsumerId = (ulong)consumerId;
			ackBuilder.ack_type = CommandAck.AckType.Individual;

			int entriesCount = entries.Count;
			for (int i = 0; i < entriesCount; i++)
			{
				long ledgerId = entries[i].Key;
				long entryId = entries[i].Value;

				MessageIdData messageIdDataBuilder = new MessageIdData
				{
					ledgerId = (ulong)ledgerId,
					entryId = (ulong)entryId
				};
				MessageIdData messageIdData = messageIdDataBuilder;
				ackBuilder.addMessageId(messageIdData);

				messageIdDataBuilder.recycle();
			}

			CommandAck ack = ackBuilder;

			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.Ack, Ack = (ack) });

			for (int i = 0; i < entriesCount; i++)
			{
				ack.MessageIds[i].recycle();
			}
			ack.recycle();
			ackBuilder.recycle();
			return res;
		}

		public static IByteBuffer NewAck(long consumerId, long ledgerId, long entryId, PulsarApi.CommandAck.AckType ackType, PulsarApi.CommandAck.ValidationError validationError, IDictionary<string, long> properties)
		{
			return NewAck(consumerId, ledgerId, entryId, ackType, validationError, properties, 0, 0);
		}

		public static IByteBuffer NewAck(long consumerId, long ledgerId, long entryId, PulsarApi.CommandAck.AckType ackType, PulsarApi.CommandAck.ValidationError validationError, IDictionary<string, long> properties, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandAck ackBuilder = new CommandAck
			{
				ConsumerId = (ulong)consumerId,
				ack_type = ackType
			};
			MessageIdData messageIdDataBuilder = new MessageIdData
			{
				ledgerId = (ulong)ledgerId,
				entryId = (ulong)entryId
			};
			MessageIdData messageIdData = messageIdDataBuilder;
			ackBuilder.addMessageId(messageIdData);
			if (validationError != null)
			{
				ackBuilder.validation_error = validationError;
			}
			if (txnIdMostBits > 0)
			{
				ackBuilder.TxnidMostBits = (ulong)txnIdMostBits;
			}
			if (txnIdLeastBits > 0)
			{
				ackBuilder.TxnidLeastBits = (ulong)txnIdLeastBits;
			}
			foreach (KeyValuePair<string, long> e in properties.SetOfKeyValuePairs())
			{
				ackBuilder.addProperties(new KeyLongValue { Key = e.Key, Value = (ulong)e.Value });
			}
			CommandAck ack = ackBuilder;

			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.Ack, Ack = (ack) });
			ack.recycle();
			ackBuilder.recycle();
			messageIdDataBuilder.recycle();
			messageIdData.recycle();
			return res;
		}

		public static IByteBuffer NewAckResponse(long consumerId, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandAckResponse commandAckResponseBuilder = new CommandAckResponse
			{
				ConsumerId = (ulong)consumerId,
				TxnidLeastBits = (ulong)txnIdLeastBits,
				TxnidMostBits = (ulong)txnIdMostBits
			};
			CommandAckResponse commandAckResponse = commandAckResponseBuilder;

			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.AckResponse, ackResponse = (commandAckResponse) });
			commandAckResponseBuilder.recycle();
			commandAckResponse.recycle();

			return res;
		}

		public static IByteBuffer NewAckErrorResponse(ServerError error, string errorMsg, long consumerId)
		{
			CommandAckResponse ackErrorBuilder = new CommandAckResponse();
			ackErrorBuilder.ConsumerId = (ulong)consumerId;
			ackErrorBuilder.Error = error;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				ackErrorBuilder.Message = (errorMsg);
			}

			CommandAckResponse response = ackErrorBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.AckResponse, ackResponse = (response) });

			ackErrorBuilder.recycle();
			response.recycle();

			return res;
		}

		public static IByteBuffer NewFlow(long consumerId, int messagePermits)
		{
			CommandFlow flowBuilder = new CommandFlow
			{
				ConsumerId = (ulong)consumerId,
				messagePermits = (uint)messagePermits
			};
			CommandFlow flow = flowBuilder;

			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.Flow, Flow = (flowBuilder) });
			flow.recycle();
			flowBuilder.recycle();
			return res;
		}

		public static IByteBuffer NewRedeliverUnacknowledgedMessages(long consumerId)
		{
			CommandRedeliverUnacknowledgedMessages redeliverBuilder = new CommandRedeliverUnacknowledgedMessages();
			redeliverBuilder.ConsumerId = (ulong)consumerId;

			CommandRedeliverUnacknowledgedMessages redeliver = redeliverBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.RedeliverUnacknowledgedMessages, redeliverUnacknowledgedMessages = redeliverBuilder });
			redeliver.recycle();
			redeliverBuilder.recycle();
			return res;
		}

		public static IByteBuffer NewRedeliverUnacknowledgedMessages(long consumerId, IList<MessageIdData> messageIds)
		{
			CommandRedeliverUnacknowledgedMessages redeliverBuilder = new CommandRedeliverUnacknowledgedMessages
			{
				ConsumerId = (ulong)consumerId
			};
			redeliverBuilder.addAllMessageIds(messageIds);
			CommandRedeliverUnacknowledgedMessages redeliver = redeliverBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.RedeliverUnacknowledgedMessages, redeliverUnacknowledgedMessages = redeliverBuilder });
			redeliver.recycle();
			redeliverBuilder.recycle();
			return res;
		}

		public static IByteBuffer NewConsumerStatsResponse(ServerError serverError, string errMsg, long requestId)
		{
			CommandConsumerStatsResponse commandConsumerStatsResponseBuilder = new CommandConsumerStatsResponse();
			commandConsumerStatsResponseBuilder.RequestId = (ulong)requestId;
			commandConsumerStatsResponseBuilder.ErrorMessage = (errMsg);
			commandConsumerStatsResponseBuilder.ErrorCode = serverError;

			CommandConsumerStatsResponse commandConsumerStatsResponse = commandConsumerStatsResponseBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.ConsumerStatsResponse, consumerStatsResponse = commandConsumerStatsResponseBuilder });
			commandConsumerStatsResponse.recycle();
			commandConsumerStatsResponseBuilder.recycle();
			return res;
		}

		public static IByteBuffer NewConsumerStatsResponse(CommandConsumerStatsResponse builder)
		{
			CommandConsumerStatsResponse commandConsumerStatsResponse = builder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.ConsumerStatsResponse, consumerStatsResponse = builder });
			commandConsumerStatsResponse.recycle();
			builder.recycle();
			return res;
		}

		public static IByteBuffer NewGetTopicsOfNamespaceRequest(string @namespace, long requestId, PulsarApi.CommandGetTopicsOfNamespace.Mode mode)
		{
			CommandGetTopicsOfNamespace topicsBuilder = new CommandGetTopicsOfNamespace
			{
				Namespace = @namespace,
				RequestId = (ulong)requestId,
				mode = mode
			};

			CommandGetTopicsOfNamespace topicsCommand = topicsBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.GetTopicsOfNamespace, getTopicsOfNamespace = topicsCommand });
			topicsBuilder.recycle();
			topicsCommand.recycle();
			return res;
		}

		public static IByteBuffer NewGetTopicsOfNamespaceResponse(IList<string> topics, long requestId)
		{
			CommandGetTopicsOfNamespaceResponse topicsResponseBuilder = new CommandGetTopicsOfNamespaceResponse();

			topicsResponseBuilder.RequestId = (ulong)requestId;
			topicsResponseBuilder.addAllTopics(topics);

			CommandGetTopicsOfNamespaceResponse topicsOfNamespaceResponse = topicsResponseBuilder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.GetTopicsOfNamespaceResponse, getTopicsOfNamespaceResponse = topicsOfNamespaceResponse });

			topicsResponseBuilder.recycle();
			topicsOfNamespaceResponse.recycle();
			return res;
		}

		private static readonly IByteBuffer cmdPing;

		static Commands()
		{
			IByteBuffer serializedCmdPing = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.Ping, Ping = new CommandPing() });
			cmdPing = Unpooled.CopiedBuffer(serializedCmdPing);
			serializedCmdPing.Release();
			IByteBuffer serializedCmdPong = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.Pong, Pong = new CommandPong() });
			cmdPong = Unpooled.CopiedBuffer(serializedCmdPong);
			serializedCmdPong.Release();
		}

		internal static IByteBuffer NewPing()
		{
			return cmdPing.RetainedDuplicate();
		}

		private static readonly IByteBuffer cmdPong;


		internal static IByteBuffer NewPong()
		{
			return cmdPong.RetainedDuplicate();
		}

		public static IByteBuffer NewGetLastMessageId(long consumerId, long requestId)
		{
			CommandGetLastMessageId cmdBuilder = new CommandGetLastMessageId
			{
				ConsumerId = (ulong)consumerId,
				RequestId = (ulong)requestId
			};

			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.GetLastMessageId, getLastMessageId = cmdBuilder });
			cmdBuilder.recycle();
			return res;
		}

		public static IByteBuffer NewGetLastMessageIdResponse(long requestId, MessageIdData messageIdData)
		{
			CommandGetLastMessageIdResponse response = new CommandGetLastMessageIdResponse
			{

				LastMessageId = (messageIdData),
				RequestId = (ulong)(requestId)
			};

			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.GetLastMessageIdResponse, getLastMessageIdResponse = response });
			response.recycle();
			return res;
		}

		public static IByteBuffer NewGetSchema(long requestId, string topic, Option<SchemaVersion> version)
		{
			CommandGetSchema schema = new CommandGetSchema();
			schema.RequestId = (ulong)requestId;
			schema.Topic = topic;
			if (version.HasValue)
			{
				var ver = version.ValueOr(new EmptyVersion());
				schema.SchemaVersion = ByteString.CopyFrom((byte[])(Array)ver.Bytes()).ToArray();
			}

			CommandGetSchema getSchema = schema;

			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.GetSchema, getSchema = getSchema });
			schema.recycle();
			return res;
		}

		public static IByteBuffer NewGetSchemaResponse(long requestId, CommandGetSchemaResponse response)
		{
			var cmdRes = response;
			cmdRes.RequestId = (ulong)requestId;
			CommandGetSchemaResponse schemaResponseBuilder = cmdRes;

			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.GetSchemaResponse, getSchemaResponse = schemaResponseBuilder });
			schemaResponseBuilder.recycle();
			return res;
		}

		public static IByteBuffer NewGetSchemaResponse(long requestId, SchemaInfo schema, SchemaVersion version)
		{
			CommandGetSchemaResponse schemaResponse = new CommandGetSchemaResponse();
			schemaResponse.RequestId = (ulong)requestId;
			schemaResponse.SchemaVersion = ByteString.CopyFrom((byte[])(Array)version.Bytes()).ToArray();
			schemaResponse.Schema = GetSchema(schema);

			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.GetSchemaResponse, getSchemaResponse = schemaResponse });
			schemaResponse.recycle();
			return res;
		}

		public static IByteBuffer NewGetSchemaResponseError(long requestId, ServerError error, string errorMessage)
		{
			CommandGetSchemaResponse schemaResponse = new CommandGetSchemaResponse
			{
				RequestId = (ulong)(requestId),
				ErrorCode = error,
				ErrorMessage = errorMessage
			};

			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.GetSchemaResponse, getSchemaResponse = schemaResponse });
			schemaResponse.recycle();
			return res;
		}

		public static IByteBuffer NewGetOrCreateSchema(long requestId, string topic, SchemaInfo schemaInfo)
		{
			CommandGetOrCreateSchema getOrCreateSchema = new CommandGetOrCreateSchema();
			getOrCreateSchema.RequestId = (ulong)requestId;
			getOrCreateSchema.Topic = topic;
			getOrCreateSchema.Schema = GetSchema(schemaInfo);
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.GetOrCreateSchema, getOrCreateSchema = getOrCreateSchema });
			getOrCreateSchema.recycle();
			return res;
		}

		public static IByteBuffer NewGetOrCreateSchemaResponse(long requestId, SchemaVersion schemaVersion)
		{
			CommandGetOrCreateSchemaResponse schemaResponse = new CommandGetOrCreateSchemaResponse
			{
				RequestId = (ulong)(requestId),
				SchemaVersion = ByteString.CopyFrom((byte[])(object)schemaVersion.Bytes()).ToArray()
			};
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.GetOrCreateSchemaResponse, getOrCreateSchemaResponse = schemaResponse });
			schemaResponse.recycle();
			return res;
		}

		public static IByteBuffer NewGetOrCreateSchemaResponseError(long requestId, ServerError error, string errorMessage)
		{
			CommandGetOrCreateSchemaResponse schemaResponse = new CommandGetOrCreateSchemaResponse
			{
				RequestId = (ulong)(requestId),
				ErrorCode = (error),
				ErrorMessage = errorMessage
			};
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.GetOrCreateSchemaResponse, getOrCreateSchemaResponse = schemaResponse });
			schemaResponse.recycle();
			return res;
		}

		// ---- transaction related ----

		public static IByteBuffer NewTxn(long tcId, long requestId, long ttlSeconds)
		{
			CommandNewTxn commandNewTxn = new CommandNewTxn
			{
				TcId = (ulong)tcId,
				RequestId = (ulong)(requestId),
				TxnTtlSeconds = (ulong)ttlSeconds
			};
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.NewTxn, newTxn = commandNewTxn });
			commandNewTxn.recycle();
			return res;
		}

		public static IByteBuffer NewTxnResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandNewTxnResponse commandNewTxnResponse = new CommandNewTxnResponse
			{
				RequestId = (ulong)requestId,
				TxnidMostBits = (ulong)(txnIdMostBits),
				TxnidLeastBits = (ulong)txnIdLeastBits
			};
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.NewTxnResponse, newTxnResponse = commandNewTxnResponse });
			commandNewTxnResponse.recycle();

			return res;
		}

		public static IByteBuffer NewTxnResponse(long requestId, long txnIdMostBits, ServerError error, string errorMsg)
		{
			CommandNewTxnResponse builder = new CommandNewTxnResponse
			{
				RequestId = (ulong)requestId,
				TxnidMostBits = (ulong)txnIdMostBits,
				Error = error
			};
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.Message = (errorMsg);
			}
			CommandNewTxnResponse errorResponse = builder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.NewTxnResponse, newTxnResponse = errorResponse });
			builder.recycle();
			errorResponse.recycle();

			return res;
		}

		public static IByteBuffer NewAddPartitionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandAddPartitionToTxn commandAddPartitionToTxn = new CommandAddPartitionToTxn
			{
				RequestId = (ulong)(requestId),
				TxnidLeastBits = (ulong)(txnIdLeastBits),
				TxnidMostBits = (ulong)txnIdMostBits
			};
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.AddPartitionToTxn, addPartitionToTxn = commandAddPartitionToTxn });
			commandAddPartitionToTxn.recycle();
			return res;
		}

		public static IByteBuffer NewAddPartitionToTxnResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandAddPartitionToTxnResponse commandAddPartitionToTxnResponse = new CommandAddPartitionToTxnResponse
			{
				RequestId = (ulong)(requestId),
				TxnidLeastBits = (ulong)(txnIdLeastBits),
				TxnidMostBits = (ulong)txnIdMostBits
			};
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.AddPartitionToTxnResponse, addPartitionToTxnResponse = commandAddPartitionToTxnResponse });
			commandAddPartitionToTxnResponse.recycle();
			return res;
		}

		public static IByteBuffer NewAddPartitionToTxnResponse(long requestId, long txnIdMostBits, PulsarApi.ServerError error, string errorMsg)
		{
			CommandAddPartitionToTxnResponse builder = new CommandAddPartitionToTxnResponse
			{
				RequestId = (ulong)requestId,
				TxnidMostBits = (ulong)txnIdMostBits,
				Error = error
			};
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.Message = errorMsg;
			}
			CommandAddPartitionToTxnResponse commandAddPartitionToTxnResponse = builder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.AddPartitionToTxnResponse, addPartitionToTxnResponse = commandAddPartitionToTxnResponse });
			builder.recycle();
			commandAddPartitionToTxnResponse.recycle();
			return res;
		}

		public static IByteBuffer NewAddSubscriptionToTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, IList<Subscription> subscription)
		{
			CommandAddSubscriptionToTxn commandAddSubscriptionToTxn = new CommandAddSubscriptionToTxn
			{
				RequestId = (ulong)(requestId),
				TxnidLeastBits = (ulong)(txnIdLeastBits),
				TxnidMostBits = (ulong)txnIdMostBits,
				addAllSubscription = (subscription)
			};
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.AddSubscriptionToTxn, addSubscriptionToTxn = commandAddSubscriptionToTxn });
			commandAddSubscriptionToTxn.recycle();
			return res;
		}

		public static IByteBuffer NewAddSubscriptionToTxnResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			CommandAddSubscriptionToTxnResponse command = new CommandAddSubscriptionToTxnResponse
			{
				RequestId = (ulong)(requestId),
				TxnidLeastBits = (ulong)(txnIdLeastBits),
				TxnidMostBits = (ulong)(txnIdMostBits)
			};
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.AddSubscriptionToTxnResponse, addSubscriptionToTxnResponse = command });
			command.recycle();
			return res;
		}

		public static IByteBuffer NewAddSubscriptionToTxnResponse(long requestId, long txnIdMostBits, ServerError error, string errorMsg)
		{
			CommandAddSubscriptionToTxnResponse builder = new CommandAddSubscriptionToTxnResponse
			{
				RequestId = (ulong)requestId,
				TxnidMostBits = (ulong)txnIdMostBits,
				Error = error
			};
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.Message = (errorMsg);
			}
			CommandAddSubscriptionToTxnResponse errorResponse = builder;
			IByteBuffer res = SerializeWithSize(new BaseCommand { type = BaseCommand.Type.AddSubscriptionToTxnResponse, addSubscriptionToTxnResponse = errorResponse });
			builder.recycle();
			errorResponse.recycle();
			return res;
		}

		public static IByteBuffer newEndTxn(long requestId, long txnIdLeastBits, long txnIdMostBits, PulsarApi.TxnAction txnAction)
		{
			PulsarApi.CommandEndTxn commandEndTxn = PulsarApi.CommandEndTxn().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).setTxnAction(txnAction).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand().setType(PulsarApi.BaseCommand.Type.END_TXN).setEndTxn(commandEndTxn));
			commandEndTxn.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			PulsarApi.CommandEndTxnResponse commandEndTxnResponse = PulsarApi.CommandEndTxnResponse().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand().setType(PulsarApi.BaseCommand.Type.END_TXN_RESPONSE).setEndTxnResponse(commandEndTxnResponse));
			commandEndTxnResponse.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnResponse(long requestId, long txnIdMostBits, PulsarApi.ServerError error, string errorMsg)
		{
			PulsarApi.CommandEndTxnResponse builder = PulsarApi.CommandEndTxnResponse();
			builder.RequestId = requestId;
			builder.TxnidMostBits = txnIdMostBits;
			builder.Error = error;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.setMessage(errorMsg);
			}
			PulsarApi.CommandEndTxnResponse commandEndTxnResponse = builder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand().setType(PulsarApi.BaseCommand.Type.END_TXN_RESPONSE).setEndTxnResponse(commandEndTxnResponse));
			builder.recycle();
			commandEndTxnResponse.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnOnPartition(long requestId, long txnIdLeastBits, long txnIdMostBits, string topic, PulsarApi.TxnAction txnAction)
		{
			PulsarApi.CommandEndTxnOnPartition txnEndOnPartition = PulsarApi.CommandEndTxnOnPartition().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).setTopic(topic).setTxnAction(txnAction);
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_PARTITION).setEndTxnOnPartition(txnEndOnPartition));
			txnEndOnPartition.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnOnPartitionResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			PulsarApi.CommandEndTxnOnPartitionResponse commandEndTxnOnPartitionResponse = PulsarApi.CommandEndTxnOnPartitionResponse().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_PARTITION_RESPONSE).setEndTxnOnPartitionResponse(commandEndTxnOnPartitionResponse));
			commandEndTxnOnPartitionResponse.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnOnPartitionResponse(long requestId, PulsarApi.ServerError error, string errorMsg)
		{
			PulsarApi.CommandEndTxnOnPartitionResponse builder = PulsarApi.CommandEndTxnOnPartitionResponse();
			builder.RequestId = requestId;
			builder.Error = error;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.setMessage(errorMsg);
			}
			PulsarApi.CommandEndTxnOnPartitionResponse commandEndTxnOnPartitionResponse = builder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_PARTITION_RESPONSE).setEndTxnOnPartitionResponse(commandEndTxnOnPartitionResponse));
			builder.recycle();
			commandEndTxnOnPartitionResponse.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnOnSubscription(long requestId, long txnIdLeastBits, long txnIdMostBits, PulsarApi.Subscription subscription, PulsarApi.TxnAction txnAction)
		{
			PulsarApi.CommandEndTxnOnSubscription commandEndTxnOnSubscription = PulsarApi.CommandEndTxnOnSubscription().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).setSubscription(subscription).setTxnAction(txnAction).build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_SUBSCRIPTION).setEndTxnOnSubscription(commandEndTxnOnSubscription));
			commandEndTxnOnSubscription.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnOnSubscriptionResponse(long requestId, long txnIdLeastBits, long txnIdMostBits)
		{
			PulsarApi.CommandEndTxnOnSubscriptionResponse response = PulsarApi.CommandEndTxnOnSubscriptionResponse().setRequestId(requestId).setTxnidLeastBits(txnIdLeastBits).setTxnidMostBits(txnIdMostBits).build();
			IByteBuffer res = SerializeWithSize(PulsarApi.BaseCommand().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_SUBSCRIPTION_RESPONSE).setEndTxnOnSubscriptionResponse(response));
			response.recycle();
			return res;
		}

		public static IByteBuffer newEndTxnOnSubscriptionResponse(long requestId, PulsarApi.ServerError error, string errorMsg)
		{
			PulsarApi.CommandEndTxnOnSubscriptionResponse builder = PulsarApi.CommandEndTxnOnSubscriptionResponse();
			builder.RequestId = requestId;
			builder.Error = error;
			if (!string.ReferenceEquals(errorMsg, null))
			{
				builder.setMessage(errorMsg);
			}
			PulsarApi.CommandEndTxnOnSubscriptionResponse response = builder.build();
			IByteBuffer res = serializeWithSize(PulsarApi.BaseCommand().setType(PulsarApi.BaseCommand.Type.END_TXN_ON_SUBSCRIPTION_RESPONSE).setEndTxnOnSubscriptionResponse(response));
			builder.recycle();
			response.recycle();
			return res;
		}
		public static IByteBuffer SerializeWithSize(BaseCommand cmdBuilder)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD]
			BaseCommand cmd = cmdBuilder;

			int cmdSize = cmd.SerializedSize;
			int totalSize = cmdSize + 4;
			int frameSize = totalSize + 4;

			IByteBuffer buf = PulsarByteBufAllocator.DEFAULT.buffer(frameSize, frameSize);

			// Prepend 2 lengths to the buffer
			buf.WriteInt(totalSize);
			buf.WriteInt(cmdSize);

			ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.Get(buf);

			try
			{
				cmd.writeTo(outStream);
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw e;
			}
			finally
			{
				cmd.recycle();
				cmdBuilder.recycle();
				outStream.Recycle();
			}

			return buf;
		}

		private static ByteBufPair SerializeCommandSendWithSize(BaseCommand cmdBuilder, ChecksumType checksumType, PulsarApi.MessageMetadata msgMetadata, IByteBuffer payload)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]

			PulsarApi.BaseCommand cmd = cmdBuilder.build();
			int cmdSize = cmd.SerializedSize;
			int msgMetadataSize = msgMetadata.SerializedSize;
			int payloadSize = payload.ReadableBytes;
			int magicAndChecksumLength = ChecksumType.Crc32c.Equals(checksumType) ? (2 + 4) : 0;
			bool includeChecksum = magicAndChecksumLength > 0;
			// cmdLength + cmdSize + magicLength +
			// checksumSize + msgMetadataLength +
			// msgMetadataSize
			int headerContentSize = 4 + cmdSize + magicAndChecksumLength + 4 + msgMetadataSize;
			int totalSize = headerContentSize + payloadSize;
			int headersSize = 4 + headerContentSize; // totalSize + headerLength
			int checksumReaderIndex = -1;

			IByteBuffer headers = PulsarByteBufAllocator.DEFAULT.buffer(headersSize, headersSize);
			headers.WriteInt(totalSize); // External frame

			try
			{
				// Write cmd
				headers.WriteInt(cmdSize);

				ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.Get(headers);
				cmd.writeTo(outStream);
				cmd.recycle();
				cmdBuilder.recycle();

				//Create checksum placeholder
				if (includeChecksum)
				{
					headers.WriteShort(magicCrc32c);
					checksumReaderIndex = headers.WriterIndex();
					headers.writerIndex(headers.writerIndex() + checksumSize); //skip 4 bytes of checksum
				}

				// Write metadata
				headers.writeInt(msgMetadataSize);
				msgMetadata.writeTo(outStream);
				outStream.recycle();
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw new Exception(e);
			}

			ByteBufPair command = ByteBufPair.Get(headers, payload);

			// write checksum at created checksum-placeholder
			if (includeChecksum)
			{
				headers.MarkReaderIndex();
				headers.readerIndex(checksumReaderIndex + checksumSize);
				int metadataChecksum = ComputeChecksum(headers);
				int computedChecksum = ResumeChecksum(metadataChecksum, payload);
				// set computed checksum
				headers.SetInt(checksumReaderIndex, computedChecksum);
				headers.ResetReaderIndex();
			}
			return command;
		}

		public static IByteBuffer SerializeMetadataAndPayload(ChecksumType checksumType, PulsarApi.MessageMetadata msgMetadata, IByteBuffer payload)
		{
			// / Wire format
			// [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
			int msgMetadataSize = msgMetadata.SerializedSize;
			int payloadSize = payload.ReadableBytes;
			int magicAndChecksumLength = ChecksumType.Crc32c.Equals(checksumType) ? (2 + 4) : 0;
			bool includeChecksum = magicAndChecksumLength > 0;
			int headerContentSize = magicAndChecksumLength + 4 + msgMetadataSize; // magicLength +
																				  // checksumSize + msgMetadataLength +
																				  // msgMetadataSize
			int checksumReaderIndex = -1;
			int totalSize = headerContentSize + payloadSize;
			IByteBuffer metadataAndPayload = PulsarByteBufAllocator.DEFAULT.buffer(totalSize, totalSize);
			try
			{
				IByteBufferCodedOutputStream outStream = IByteBufferCodedOutputStream.get(metadataAndPayload);

				//Create checksum placeholder
				if (includeChecksum)
				{
					metadataAndPayload.writeShort(magicCrc32c);
					checksumReaderIndex = metadataAndPayload.writerIndex();
					metadataAndPayload.writerIndex(metadataAndPayload.writerIndex() + checksumSize); //skip 4 bytes of checksum
				}

				// Write metadata
				metadataAndPayload.writeInt(msgMetadataSize);
				msgMetadata.writeTo(outStream);
				outStream.recycle();
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw new Exception(e);
			}

			// write checksum at created checksum-placeholder
			if (includeChecksum)
			{
				metadataAndPayload.markReaderIndex();
				metadataAndPayload.readerIndex(checksumReaderIndex + checksumSize);
				int metadataChecksum = computeChecksum(metadataAndPayload);
				int computedChecksum = resumeChecksum(metadataChecksum, payload);
				// set computed checksum
				metadataAndPayload.setInt(checksumReaderIndex, computedChecksum);
				metadataAndPayload.resetReaderIndex();
			}
			metadataAndPayload.writeBytes(payload);

			return metadataAndPayload;
		}

		public static long initBatchMessageMetadata(PulsarApi.MessageMetadata messageMetadata, PulsarApi.MessageMetadata builder)
		{
			messageMetadata.PublishTime = builder.PublishTime;
			messageMetadata.setProducerName(builder.getProducerName());
			messageMetadata.SequenceId = builder.SequenceId;
			if (builder.hasReplicatedFrom())
			{
				messageMetadata.setReplicatedFrom(builder.getReplicatedFrom());
			}
			if (builder.ReplicateToCount > 0)
			{
				messageMetadata.addAllReplicateTo(builder.ReplicateToList);
			}
			if (builder.hasSchemaVersion())
			{
				messageMetadata.SchemaVersion = builder.SchemaVersion;
			}
			return builder.SequenceId;
		}

		public static IByteBuffer serializeSingleMessageInBatchWithPayload(PulsarApi.SingleMessageMetadata singleMessageMetadataBuilder, IByteBuffer payload, IByteBuffer batchBuffer)
		{
			int payLoadSize = payload.readableBytes();
			PulsarApi.SingleMessageMetadata singleMessageMetadata = singleMessageMetadataBuilder.setPayloadSize(payLoadSize).build();
			// serialize meta-data size, meta-data and payload for single message in batch
			int singleMsgMetadataSize = singleMessageMetadata.SerializedSize;
			try
			{
				batchBuffer.writeInt(singleMsgMetadataSize);
				IByteBufferCodedOutputStream outStream = IByteBufferCodedOutputStream.get(batchBuffer);
				singleMessageMetadata.writeTo(outStream);
				singleMessageMetadata.recycle();
				outStream.recycle();
			}
			catch (IOException e)
			{
				throw new Exception(e);
			}
			return batchBuffer.writeBytes(payload);
		}

		public static IByteBuffer serializeSingleMessageInBatchWithPayload(PulsarApi.MessageMetadata msgBuilder, IByteBuffer payload, IByteBuffer batchBuffer)
		{

			// build single message meta-data
			PulsarApi.SingleMessageMetadata singleMessageMetadataBuilder = PulsarApi.SingleMessageMetadata();
			if (msgBuilder.hasPartitionKey())
			{
				singleMessageMetadataBuilder = singleMessageMetadataBuilder.setPartitionKey(msgBuilder.getPartitionKey()).setPartitionKeyB64Encoded(msgBuilder.PartitionKeyB64Encoded);
			}
			if (msgBuilder.hasOrderingKey())
			{
				singleMessageMetadataBuilder = singleMessageMetadataBuilder.setOrderingKey(msgBuilder.OrderingKey);
			}
			if (msgBuilder.PropertiesList.Count > 0)
			{
				singleMessageMetadataBuilder = singleMessageMetadataBuilder.addAllProperties(msgBuilder.PropertiesList);
			}

			if (msgBuilder.hasEventTime())
			{
				singleMessageMetadataBuilder.EventTime = msgBuilder.EventTime;
			}

			if (msgBuilder.hasSequenceId())
			{
				singleMessageMetadataBuilder.SequenceId = msgBuilder.SequenceId;
			}

			try
			{
				return serializeSingleMessageInBatchWithPayload(singleMessageMetadataBuilder, payload, batchBuffer);
			}
			finally
			{
				singleMessageMetadataBuilder.recycle();
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public static io.netty.buffer.IByteBuffer deSerializeSingleMessageInBatch(io.netty.buffer.IByteBuffer uncompressedPayload, org.apache.pulsar.common.api.proto.PulsarApi.SingleMessageMetadata singleMessageMetadataBuilder, int index, int batchSize) throws java.io.IOException
		public static IByteBuffer deSerializeSingleMessageInBatch(IByteBuffer uncompressedPayload, PulsarApi.SingleMessageMetadata singleMessageMetadataBuilder, int index, int batchSize)
		{
			int singleMetaSize = (int)uncompressedPayload.readUnsignedInt();
			int writerIndex = uncompressedPayload.writerIndex();
			int beginIndex = uncompressedPayload.readerIndex() + singleMetaSize;
			uncompressedPayload.writerIndex(beginIndex);
			IByteBufferCodedInputStream stream = IByteBufferCodedInputStream.get(uncompressedPayload);
			singleMessageMetadataBuilder.mergeFrom(stream, null);
			stream.recycle();

			int singleMessagePayloadSize = singleMessageMetadataBuilder.PayloadSize;

			int readerIndex = uncompressedPayload.readerIndex();
			IByteBuffer singleMessagePayload = uncompressedPayload.retainedSlice(readerIndex, singleMessagePayloadSize);
			uncompressedPayload.writerIndex(writerIndex);

			// reader now points to beginning of payload read; so move it past message payload just read
			if (index < batchSize)
			{
				uncompressedPayload.readerIndex(readerIndex + singleMessagePayloadSize);
			}

			return singleMessagePayload;
		}

		private static ByteBufPair SerializeCommandMessageWithSize(BaseCommand cmd, IByteBuffer metadataAndPayload)
		{
			// / Wire format
			// [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
			//
			// metadataAndPayload contains from magic-number to the payload included


			int cmdSize = cmd.SerializedSize;
			int totalSize = 4 + cmdSize + metadataAndPayload.readableBytes();
			int headersSize = 4 + 4 + cmdSize;

			IByteBuffer headers = PulsarIByteBufferAllocator.DEFAULT.buffer(headersSize);
			headers.writeInt(totalSize); // External frame

			try
			{
				// Write cmd
				headers.writeInt(cmdSize);

				IByteBufferCodedOutputStream outStream = IByteBufferCodedOutputStream.get(headers);
				cmd.writeTo(outStream);
				outStream.recycle();
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw new Exception(e);
			}

			return (IByteBufferPair)IByteBufferPair.get(headers, metadataAndPayload);
		}

		public static int getNumberOfMessagesInBatch(IByteBuffer metadataAndPayload, string subscription, long consumerId)
		{
			PulsarApi.MessageMetadata msgMetadata = peekMessageMetadata(metadataAndPayload, subscription, consumerId);
			if (msgMetadata == null)
			{
				return -1;
			}
			else
			{
				int numMessagesInBatch = msgMetadata.NumMessagesInBatch;
				msgMetadata.recycle();
				return numMessagesInBatch;
			}
		}

		public static PulsarApi.MessageMetadata peekMessageMetadata(IByteBuffer metadataAndPayload, string subscription, long consumerId)
		{
			try
			{
				// save the reader index and restore after parsing
				int readerIdx = metadataAndPayload.ReaderIndex;
				PulsarApi.MessageMetadata.Builder metadata = Commands.ParseMessageMetadata(metadataAndPayload);
				metadataAndPayload.SetReaderIndex(readerIdx);

				return metadata;
			}
			catch (System.Exception t)
			{
				log.error("[{}] [{}] Failed to parse message metadata", subscription, consumerId, t);
				return null;
			}
		}

		public static int CurrentProtocolVersion
		{
			get
			{
				// Return the last ProtocolVersion enum value
				return PulsarApi.ProtocolVersion.values()[PulsarApi.ProtocolVersion.values().length - 1].Number;
			}
		}

		/// <summary>
		/// Definition of possible checksum types.
		/// </summary>
		public enum ChecksumType
		{
			Crc32c,
			None
		}

		public static bool PeerSupportsGetLastMessageId(int peerVersion)
		{
			return peerVersion >= (int)PulsarApi.ProtocolVersion.V12;
		}

		public static bool PeerSupportsActiveConsumerListener(int peerVersion)
		{
			return peerVersion >= (int)ProtocolVersion.V12;
		}

		public static bool PeerSupportsMultiMessageAcknowledgment(int peerVersion)
		{
			return peerVersion >= (int)PulsarApi.ProtocolVersion.V12;
		}

		public static bool PeerSupportJsonSchemaAvroFormat(int peerVersion)
		{
			return peerVersion >= (int)PulsarApi.ProtocolVersion.V13;
		}

		public static bool PeerSupportsGetOrCreateSchema(int peerVersion)
		{
			return peerVersion >= (int)PulsarApi.ProtocolVersion.V15;
		}
	}

}