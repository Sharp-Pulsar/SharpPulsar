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
namespace org.apache.pulsar.common.protocol
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using PooledByteBufAllocator = io.netty.buffer.PooledByteBufAllocator;
	using SneakyThrows = lombok.SneakyThrows;
	using UtilityClass = lombok.experimental.UtilityClass;
	using MessageMetadata = org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata;
	using PulsarMarkers = org.apache.pulsar.common.api.proto.PulsarMarkers;
	using ClusterMessageId = org.apache.pulsar.common.api.proto.PulsarMarkers.ClusterMessageId;
	using MarkerType = org.apache.pulsar.common.api.proto.PulsarMarkers.MarkerType;
	using MessageIdData = org.apache.pulsar.common.api.proto.PulsarMarkers.MessageIdData;
	using ReplicatedSubscriptionsSnapshot = org.apache.pulsar.common.api.proto.PulsarMarkers.ReplicatedSubscriptionsSnapshot;
	using ReplicatedSubscriptionsSnapshotRequest = org.apache.pulsar.common.api.proto.PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest;
	using ReplicatedSubscriptionsSnapshotResponse = org.apache.pulsar.common.api.proto.PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse;
	using ReplicatedSubscriptionsUpdate = org.apache.pulsar.common.api.proto.PulsarMarkers.ReplicatedSubscriptionsUpdate;
	using ChecksumType = org.apache.pulsar.common.protocol.Commands.ChecksumType;
	using ByteBufCodedInputStream = org.apache.pulsar.common.util.protobuf.ByteBufCodedInputStream;
	using ByteBufCodedOutputStream = org.apache.pulsar.common.util.protobuf.ByteBufCodedOutputStream;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UtilityClass @SuppressWarnings("checkstyle:JavadocType") public class Markers
	public class Markers
	{

		private static ByteBuf newMessage(PulsarMarkers.MarkerType markerType, Optional<string> restrictToCluster, ByteBuf payload)
		{
			MessageMetadata.Builder msgMetadataBuilder = MessageMetadata.newBuilder();
			msgMetadataBuilder.PublishTime = DateTimeHelper.CurrentUnixTimeMillis();
			msgMetadataBuilder.setProducerName("pulsar.marker");
			msgMetadataBuilder.SequenceId = 0;
			msgMetadataBuilder.MarkerType = markerType.Number;

			if (restrictToCluster.Present)
			{
				msgMetadataBuilder.addReplicateTo(restrictToCluster.get());
			}

			MessageMetadata msgMetadata = msgMetadataBuilder.build();
			try
			{
				return Commands.serializeMetadataAndPayload(ChecksumType.Crc32c, msgMetadata, payload);
			}
			finally
			{
				msgMetadata.recycle();
				msgMetadataBuilder.recycle();
			}
		}

		public static bool isServerOnlyMarker(MessageMetadata msgMetadata)
		{
			// In future, if we add more marker types that can be also sent to clients
			// we'll have to do finer check here.
			return msgMetadata.hasMarkerType();
		}

		public static bool isReplicatedSubscriptionSnapshotMarker(MessageMetadata msgMetadata)
		{
			return msgMetadata != null && msgMetadata.hasMarkerType() && msgMetadata.MarkerType == PulsarMarkers.MarkerType.REPLICATED_SUBSCRIPTION_SNAPSHOT_VALUE;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SneakyThrows public static io.netty.buffer.ByteBuf newReplicatedSubscriptionsSnapshotRequest(String snapshotId, String sourceCluster)
		public static ByteBuf newReplicatedSubscriptionsSnapshotRequest(string snapshotId, string sourceCluster)
		{
			PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest.Builder builder = PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest.newBuilder();
			builder.setSnapshotId(snapshotId);
			builder.setSourceCluster(sourceCluster);

			PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest req = builder.build();

			int size = req.SerializedSize;

			ByteBuf payload = PooledByteBufAllocator.DEFAULT.buffer(size);
			ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.get(payload);
			try
			{
				req.writeTo(outStream);
				return newMessage(PulsarMarkers.MarkerType.REPLICATED_SUBSCRIPTION_SNAPSHOT_REQUEST, null, payload);
			}
			finally
			{
				payload.release();
				builder.recycle();
				req.recycle();
				outStream.recycle();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest parseReplicatedSubscriptionsSnapshotRequest(io.netty.buffer.ByteBuf payload) throws java.io.IOException
		public static PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest parseReplicatedSubscriptionsSnapshotRequest(ByteBuf payload)
		{
			ByteBufCodedInputStream inStream = ByteBufCodedInputStream.get(payload);
			PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest.Builder builder = null;

			try
			{
				builder = PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest.newBuilder();
				return builder.mergeFrom(inStream, null).build();
			}
			finally
			{
				builder.recycle();
				inStream.recycle();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SneakyThrows public static io.netty.buffer.ByteBuf newReplicatedSubscriptionsSnapshotResponse(String snapshotId, String replyToCluster, String cluster, long ledgerId, long entryId)
		public static ByteBuf newReplicatedSubscriptionsSnapshotResponse(string snapshotId, string replyToCluster, string cluster, long ledgerId, long entryId)
		{
			PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse.Builder builder = PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse.newBuilder();
			builder.setSnapshotId(snapshotId);

			PulsarMarkers.MessageIdData.Builder msgIdBuilder = PulsarMarkers.MessageIdData.newBuilder();
			msgIdBuilder.LedgerId = ledgerId;
			msgIdBuilder.EntryId = entryId;

			PulsarMarkers.ClusterMessageId.Builder clusterMessageIdBuilder = PulsarMarkers.ClusterMessageId.newBuilder();
			clusterMessageIdBuilder.setCluster(cluster);
			clusterMessageIdBuilder.setMessageId(msgIdBuilder);

			builder.setCluster(clusterMessageIdBuilder);
			PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse response = builder.build();

			int size = response.SerializedSize;

			ByteBuf payload = PooledByteBufAllocator.DEFAULT.buffer(size);
			ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.get(payload);
			try
			{
				response.writeTo(outStream);
				return newMessage(PulsarMarkers.MarkerType.REPLICATED_SUBSCRIPTION_SNAPSHOT_RESPONSE, replyToCluster, payload);
			}
			finally
			{
				msgIdBuilder.recycle();
				clusterMessageIdBuilder.recycle();
				payload.release();
				builder.recycle();
				response.recycle();
				outStream.recycle();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse parseReplicatedSubscriptionsSnapshotResponse(io.netty.buffer.ByteBuf payload) throws java.io.IOException
		public static PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse parseReplicatedSubscriptionsSnapshotResponse(ByteBuf payload)
		{
			ByteBufCodedInputStream inStream = ByteBufCodedInputStream.get(payload);
			PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse.Builder builder = null;

			try
			{
				builder = PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse.newBuilder();
				return builder.mergeFrom(inStream, null).build();
			}
			finally
			{
				builder.recycle();
				inStream.recycle();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SneakyThrows public static io.netty.buffer.ByteBuf newReplicatedSubscriptionsSnapshot(String snapshotId, String sourceCluster, long ledgerId, long entryId, java.util.Map<String, org.apache.pulsar.common.api.proto.PulsarMarkers.MessageIdData> clusterIds)
		public static ByteBuf newReplicatedSubscriptionsSnapshot(string snapshotId, string sourceCluster, long ledgerId, long entryId, IDictionary<string, PulsarMarkers.MessageIdData> clusterIds)
		{
			PulsarMarkers.ReplicatedSubscriptionsSnapshot.Builder builder = PulsarMarkers.ReplicatedSubscriptionsSnapshot.newBuilder();
			builder.setSnapshotId(snapshotId);

			PulsarMarkers.MessageIdData.Builder msgIdBuilder = PulsarMarkers.MessageIdData.newBuilder();
			msgIdBuilder.LedgerId = ledgerId;
			msgIdBuilder.EntryId = entryId;
			builder.setLocalMessageId(msgIdBuilder);

			clusterIds.forEach((cluster, msgId) =>
			{
			ClusterMessageId.Builder clusterMessageIdBuilder = ClusterMessageId.newBuilder().setCluster(cluster).setMessageId(msgId);
			builder.addClusters(clusterMessageIdBuilder);
			clusterMessageIdBuilder.recycle();
			});

			PulsarMarkers.ReplicatedSubscriptionsSnapshot snapshot = builder.build();

			int size = snapshot.SerializedSize;

			ByteBuf payload = PooledByteBufAllocator.DEFAULT.buffer(size);
			ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.get(payload);
			try
			{
				snapshot.writeTo(outStream);
				return newMessage(PulsarMarkers.MarkerType.REPLICATED_SUBSCRIPTION_SNAPSHOT, sourceCluster, payload);
			}
			finally
			{
				payload.release();
				builder.recycle();
				snapshot.recycle();
				outStream.recycle();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.PulsarMarkers.ReplicatedSubscriptionsSnapshot parseReplicatedSubscriptionsSnapshot(io.netty.buffer.ByteBuf payload) throws java.io.IOException
		public static PulsarMarkers.ReplicatedSubscriptionsSnapshot parseReplicatedSubscriptionsSnapshot(ByteBuf payload)
		{
			ByteBufCodedInputStream inStream = ByteBufCodedInputStream.get(payload);
			PulsarMarkers.ReplicatedSubscriptionsSnapshot.Builder builder = null;

			try
			{
				builder = PulsarMarkers.ReplicatedSubscriptionsSnapshot.newBuilder();
				return builder.mergeFrom(inStream, null).build();
			}
			finally
			{
				builder.recycle();
				inStream.recycle();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SneakyThrows public static io.netty.buffer.ByteBuf newReplicatedSubscriptionsUpdate(String subscriptionName, java.util.Map<String, org.apache.pulsar.common.api.proto.PulsarMarkers.MessageIdData> clusterIds)
		public static ByteBuf newReplicatedSubscriptionsUpdate(string subscriptionName, IDictionary<string, PulsarMarkers.MessageIdData> clusterIds)
		{
			PulsarMarkers.ReplicatedSubscriptionsUpdate.Builder builder = PulsarMarkers.ReplicatedSubscriptionsUpdate.newBuilder();
			builder.setSubscriptionName(subscriptionName);

			clusterIds.forEach((cluster, msgId) =>
			{
			ClusterMessageId.Builder clusterMessageIdBuilder = ClusterMessageId.newBuilder().setCluster(cluster).setMessageId(msgId);
			builder.addClusters(clusterMessageIdBuilder);
			clusterMessageIdBuilder.recycle();
			});

			PulsarMarkers.ReplicatedSubscriptionsUpdate update = builder.build();

			int size = update.SerializedSize;

			ByteBuf payload = PooledByteBufAllocator.DEFAULT.buffer(size);
			ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.get(payload);
			try
			{
				update.writeTo(outStream);
				return newMessage(PulsarMarkers.MarkerType.REPLICATED_SUBSCRIPTION_UPDATE, null, payload);
			}
			finally
			{
				payload.release();
				builder.recycle();
				update.recycle();
				outStream.recycle();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.PulsarMarkers.ReplicatedSubscriptionsUpdate parseReplicatedSubscriptionsUpdate(io.netty.buffer.ByteBuf payload) throws java.io.IOException
		public static PulsarMarkers.ReplicatedSubscriptionsUpdate parseReplicatedSubscriptionsUpdate(ByteBuf payload)
		{
			ByteBufCodedInputStream inStream = ByteBufCodedInputStream.get(payload);
			PulsarMarkers.ReplicatedSubscriptionsUpdate.Builder builder = null;

			try
			{
				builder = PulsarMarkers.ReplicatedSubscriptionsUpdate.newBuilder();
				return builder.mergeFrom(inStream, null).build();
			}
			finally
			{
				builder.recycle();
				inStream.recycle();
			}
		}

		public static bool isTxnCommitMarker(MessageMetadata msgMetadata)
		{
			return msgMetadata != null && msgMetadata.hasMarkerType() && msgMetadata.MarkerType == PulsarMarkers.MarkerType.TXN_COMMIT_VALUE;
		}

		public static ByteBuf newTxnCommitMarker(long sequenceId, long txnMostBits, long txnLeastBits, PulsarMarkers.MessageIdData messageIdData)
		{
			return newTxnMarker(PulsarMarkers.MarkerType.TXN_COMMIT, sequenceId, txnMostBits, txnLeastBits, messageIdData);
		}

		public static bool isTxnAbortMarker(MessageMetadata msgMetadata)
		{
			return msgMetadata != null && msgMetadata.hasMarkerType() && msgMetadata.MarkerType == PulsarMarkers.MarkerType.TXN_ABORT_VALUE;
		}

		public static ByteBuf newTxnAbortMarker(long sequenceId, long txnMostBits, long txnLeastBits)
		{
			return newTxnMarker(PulsarMarkers.MarkerType.TXN_ABORT, sequenceId, txnMostBits, txnLeastBits, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.apache.pulsar.common.api.proto.PulsarMarkers.TxnCommitMarker parseCommitMarker(io.netty.buffer.ByteBuf payload) throws java.io.IOException
		public static PulsarMarkers.TxnCommitMarker parseCommitMarker(ByteBuf payload)
		{
			ByteBufCodedInputStream inStream = ByteBufCodedInputStream.get(payload);

			PulsarMarkers.TxnCommitMarker.Builder builder = null;

			try
			{
				builder = PulsarMarkers.TxnCommitMarker.newBuilder();
				return builder.mergeFrom(inStream, null).build();
			}
			finally
			{
				builder.recycle();
				inStream.recycle();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SneakyThrows private static io.netty.buffer.ByteBuf newTxnMarker(org.apache.pulsar.common.api.proto.PulsarMarkers.MarkerType markerType, long sequenceId, long txnMostBits, long txnLeastBits, java.util.Optional<org.apache.pulsar.common.api.proto.PulsarMarkers.MessageIdData> messageIdData)
		private static ByteBuf newTxnMarker(PulsarMarkers.MarkerType markerType, long sequenceId, long txnMostBits, long txnLeastBits, Optional<PulsarMarkers.MessageIdData> messageIdData)
		{
			MessageMetadata.Builder msgMetadataBuilder = MessageMetadata.newBuilder();
			msgMetadataBuilder.PublishTime = DateTimeHelper.CurrentUnixTimeMillis();
			msgMetadataBuilder.setProducerName("pulsar.txn.marker");
			msgMetadataBuilder.SequenceId = sequenceId;
			msgMetadataBuilder.MarkerType = markerType.Number;
			msgMetadataBuilder.TxnidMostBits = txnMostBits;
			msgMetadataBuilder.TxnidLeastBits = txnLeastBits;

			MessageMetadata msgMetadata = msgMetadataBuilder.build();

			ByteBuf payload;
			if (messageIdData.Present)
			{
				PulsarMarkers.TxnCommitMarker commitMarker = PulsarMarkers.TxnCommitMarker.newBuilder().setMessageId(messageIdData.get()).build();
				int size = commitMarker.SerializedSize;
				payload = PooledByteBufAllocator.DEFAULT.buffer(size);
				ByteBufCodedOutputStream outStream = ByteBufCodedOutputStream.get(payload);
				commitMarker.writeTo(outStream);
			}
			else
			{
				payload = PooledByteBufAllocator.DEFAULT.buffer();
			}

			try
			{
				return Commands.serializeMetadataAndPayload(ChecksumType.Crc32c, msgMetadata, payload);
			}
			finally
			{
				payload.release();
				msgMetadata.recycle();
				msgMetadataBuilder.recycle();
			}
		}
	}

}