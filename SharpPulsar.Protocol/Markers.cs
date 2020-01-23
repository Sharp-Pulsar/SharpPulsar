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
namespace SharpPulsar.Protocol
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using PooledByteBufAllocator = io.netty.buffer.PooledByteBufAllocator;
	using SneakyThrows = lombok.SneakyThrows;
	using UtilityClass = lombok.experimental.UtilityClass;
	using MessageMetadata = SharpPulsar.Api.Proto.PulsarApi.MessageMetadata;
	using PulsarMarkers = SharpPulsar.Api.Proto.PulsarMarkers;
	using ClusterMessageId = SharpPulsar.Api.Proto.PulsarMarkers.ClusterMessageId;
	using MarkerType = SharpPulsar.Api.Proto.PulsarMarkers.MarkerType;
	using MessageIdData = SharpPulsar.Api.Proto.PulsarMarkers.MessageIdData;
	using ReplicatedSubscriptionsSnapshot = SharpPulsar.Api.Proto.PulsarMarkers.ReplicatedSubscriptionsSnapshot;
	using ReplicatedSubscriptionsSnapshotRequest = SharpPulsar.Api.Proto.PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest;
	using ReplicatedSubscriptionsSnapshotResponse = SharpPulsar.Api.Proto.PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse;
	using ReplicatedSubscriptionsUpdate = SharpPulsar.Api.Proto.PulsarMarkers.ReplicatedSubscriptionsUpdate;
	using ChecksumType = SharpPulsar.Protocol.Commands.ChecksumType;
	using ByteBufCodedInputStream = SharpPulsar.Util.Protobuf.ByteBufCodedInputStream;
	using ByteBufCodedOutputStream = SharpPulsar.Util.Protobuf.ByteBufCodedOutputStream;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UtilityClass @SuppressWarnings("checkstyle:JavadocType") public class Markers
	public class Markers
	{

		private static ByteBuf NewMessage(PulsarMarkers.MarkerType MarkerType, Optional<string> RestrictToCluster, ByteBuf Payload)
		{
			MessageMetadata.Builder MsgMetadataBuilder = MessageMetadata.newBuilder();
			MsgMetadataBuilder.PublishTime = DateTimeHelper.CurrentUnixTimeMillis();
			MsgMetadataBuilder.setProducerName("pulsar.marker");
			MsgMetadataBuilder.SequenceId = 0;
			MsgMetadataBuilder.MarkerType = MarkerType.Number;

			if (RestrictToCluster.Present)
			{
				MsgMetadataBuilder.addReplicateTo(RestrictToCluster.get());
			}

			MessageMetadata MsgMetadata = MsgMetadataBuilder.build();
			try
			{
				return Commands.SerializeMetadataAndPayload(ChecksumType.Crc32c, MsgMetadata, Payload);
			}
			finally
			{
				MsgMetadata.recycle();
				MsgMetadataBuilder.recycle();
			}
		}

		public static bool IsServerOnlyMarker(MessageMetadata MsgMetadata)
		{
			// In future, if we add more marker types that can be also sent to clients
			// we'll have to do finer check here.
			return MsgMetadata.hasMarkerType();
		}

		public static bool IsReplicatedSubscriptionSnapshotMarker(MessageMetadata MsgMetadata)
		{
			return MsgMetadata != null && MsgMetadata.hasMarkerType() && MsgMetadata.MarkerType == PulsarMarkers.MarkerType.REPLICATED_SUBSCRIPTION_SNAPSHOT_VALUE;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SneakyThrows public static io.netty.buffer.ByteBuf newReplicatedSubscriptionsSnapshotRequest(String snapshotId, String sourceCluster)
		public static ByteBuf NewReplicatedSubscriptionsSnapshotRequest(string SnapshotId, string SourceCluster)
		{
			PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest.Builder Builder = PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest.newBuilder();
			Builder.setSnapshotId(SnapshotId);
			Builder.setSourceCluster(SourceCluster);

			PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest Req = Builder.build();

			int Size = Req.SerializedSize;

			ByteBuf Payload = PooledByteBufAllocator.DEFAULT.buffer(Size);
			ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.get(Payload);
			try
			{
				Req.writeTo(OutStream);
				return NewMessage(PulsarMarkers.MarkerType.REPLICATED_SUBSCRIPTION_SNAPSHOT_REQUEST, null, Payload);
			}
			finally
			{
				Payload.release();
				Builder.recycle();
				Req.recycle();
				OutStream.recycle();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static SharpPulsar.api.proto.PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest parseReplicatedSubscriptionsSnapshotRequest(io.netty.buffer.ByteBuf payload) throws java.io.IOException
		public static PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest ParseReplicatedSubscriptionsSnapshotRequest(ByteBuf Payload)
		{
			ByteBufCodedInputStream InStream = ByteBufCodedInputStream.get(Payload);
			PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest.Builder Builder = null;

			try
			{
				Builder = PulsarMarkers.ReplicatedSubscriptionsSnapshotRequest.newBuilder();
				return Builder.mergeFrom(InStream, null).build();
			}
			finally
			{
				Builder.recycle();
				InStream.recycle();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SneakyThrows public static io.netty.buffer.ByteBuf newReplicatedSubscriptionsSnapshotResponse(String snapshotId, String replyToCluster, String cluster, long ledgerId, long entryId)
		public static ByteBuf NewReplicatedSubscriptionsSnapshotResponse(string SnapshotId, string ReplyToCluster, string Cluster, long LedgerId, long EntryId)
		{
			PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse.Builder Builder = PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse.newBuilder();
			Builder.setSnapshotId(SnapshotId);

			PulsarMarkers.MessageIdData.Builder MsgIdBuilder = PulsarMarkers.MessageIdData.newBuilder();
			MsgIdBuilder.LedgerId = LedgerId;
			MsgIdBuilder.EntryId = EntryId;

			PulsarMarkers.ClusterMessageId.Builder ClusterMessageIdBuilder = PulsarMarkers.ClusterMessageId.newBuilder();
			ClusterMessageIdBuilder.setCluster(Cluster);
			ClusterMessageIdBuilder.setMessageId(MsgIdBuilder);

			Builder.setCluster(ClusterMessageIdBuilder);
			PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse Response = Builder.build();

			int Size = Response.SerializedSize;

			ByteBuf Payload = PooledByteBufAllocator.DEFAULT.buffer(Size);
			ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.get(Payload);
			try
			{
				Response.writeTo(OutStream);
				return NewMessage(PulsarMarkers.MarkerType.REPLICATED_SUBSCRIPTION_SNAPSHOT_RESPONSE, ReplyToCluster, Payload);
			}
			finally
			{
				MsgIdBuilder.recycle();
				ClusterMessageIdBuilder.recycle();
				Payload.release();
				Builder.recycle();
				Response.recycle();
				OutStream.recycle();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static SharpPulsar.api.proto.PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse parseReplicatedSubscriptionsSnapshotResponse(io.netty.buffer.ByteBuf payload) throws java.io.IOException
		public static PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse ParseReplicatedSubscriptionsSnapshotResponse(ByteBuf Payload)
		{
			ByteBufCodedInputStream InStream = ByteBufCodedInputStream.get(Payload);
			PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse.Builder Builder = null;

			try
			{
				Builder = PulsarMarkers.ReplicatedSubscriptionsSnapshotResponse.newBuilder();
				return Builder.mergeFrom(InStream, null).build();
			}
			finally
			{
				Builder.recycle();
				InStream.recycle();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SneakyThrows public static io.netty.buffer.ByteBuf newReplicatedSubscriptionsSnapshot(String snapshotId, String sourceCluster, long ledgerId, long entryId, java.util.Map<String, SharpPulsar.api.proto.PulsarMarkers.MessageIdData> clusterIds)
		public static ByteBuf NewReplicatedSubscriptionsSnapshot(string SnapshotId, string SourceCluster, long LedgerId, long EntryId, IDictionary<string, PulsarMarkers.MessageIdData> ClusterIds)
		{
			PulsarMarkers.ReplicatedSubscriptionsSnapshot.Builder Builder = PulsarMarkers.ReplicatedSubscriptionsSnapshot.newBuilder();
			Builder.setSnapshotId(SnapshotId);

			PulsarMarkers.MessageIdData.Builder MsgIdBuilder = PulsarMarkers.MessageIdData.newBuilder();
			MsgIdBuilder.LedgerId = LedgerId;
			MsgIdBuilder.EntryId = EntryId;
			Builder.setLocalMessageId(MsgIdBuilder);

			ClusterIds.forEach((cluster, msgId) =>
			{
			ClusterMessageId.Builder ClusterMessageIdBuilder = ClusterMessageId.newBuilder().setCluster(cluster).setMessageId(msgId);
			Builder.addClusters(ClusterMessageIdBuilder);
			ClusterMessageIdBuilder.recycle();
			});

			PulsarMarkers.ReplicatedSubscriptionsSnapshot Snapshot = Builder.build();

			int Size = Snapshot.SerializedSize;

			ByteBuf Payload = PooledByteBufAllocator.DEFAULT.buffer(Size);
			ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.get(Payload);
			try
			{
				Snapshot.writeTo(OutStream);
				return NewMessage(PulsarMarkers.MarkerType.REPLICATED_SUBSCRIPTION_SNAPSHOT, SourceCluster, Payload);
			}
			finally
			{
				Payload.release();
				Builder.recycle();
				Snapshot.recycle();
				OutStream.recycle();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static SharpPulsar.api.proto.PulsarMarkers.ReplicatedSubscriptionsSnapshot parseReplicatedSubscriptionsSnapshot(io.netty.buffer.ByteBuf payload) throws java.io.IOException
		public static PulsarMarkers.ReplicatedSubscriptionsSnapshot ParseReplicatedSubscriptionsSnapshot(ByteBuf Payload)
		{
			ByteBufCodedInputStream InStream = ByteBufCodedInputStream.get(Payload);
			PulsarMarkers.ReplicatedSubscriptionsSnapshot.Builder Builder = null;

			try
			{
				Builder = PulsarMarkers.ReplicatedSubscriptionsSnapshot.newBuilder();
				return Builder.mergeFrom(InStream, null).build();
			}
			finally
			{
				Builder.recycle();
				InStream.recycle();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SneakyThrows public static io.netty.buffer.ByteBuf newReplicatedSubscriptionsUpdate(String subscriptionName, java.util.Map<String, SharpPulsar.api.proto.PulsarMarkers.MessageIdData> clusterIds)
		public static ByteBuf NewReplicatedSubscriptionsUpdate(string SubscriptionName, IDictionary<string, PulsarMarkers.MessageIdData> ClusterIds)
		{
			PulsarMarkers.ReplicatedSubscriptionsUpdate.Builder Builder = PulsarMarkers.ReplicatedSubscriptionsUpdate.newBuilder();
			Builder.setSubscriptionName(SubscriptionName);

			ClusterIds.forEach((cluster, msgId) =>
			{
			ClusterMessageId.Builder ClusterMessageIdBuilder = ClusterMessageId.newBuilder().setCluster(cluster).setMessageId(msgId);
			Builder.addClusters(ClusterMessageIdBuilder);
			ClusterMessageIdBuilder.recycle();
			});

			PulsarMarkers.ReplicatedSubscriptionsUpdate Update = Builder.build();

			int Size = Update.SerializedSize;

			ByteBuf Payload = PooledByteBufAllocator.DEFAULT.buffer(Size);
			ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.get(Payload);
			try
			{
				Update.writeTo(OutStream);
				return NewMessage(PulsarMarkers.MarkerType.REPLICATED_SUBSCRIPTION_UPDATE, null, Payload);
			}
			finally
			{
				Payload.release();
				Builder.recycle();
				Update.recycle();
				OutStream.recycle();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static SharpPulsar.api.proto.PulsarMarkers.ReplicatedSubscriptionsUpdate parseReplicatedSubscriptionsUpdate(io.netty.buffer.ByteBuf payload) throws java.io.IOException
		public static PulsarMarkers.ReplicatedSubscriptionsUpdate ParseReplicatedSubscriptionsUpdate(ByteBuf Payload)
		{
			ByteBufCodedInputStream InStream = ByteBufCodedInputStream.get(Payload);
			PulsarMarkers.ReplicatedSubscriptionsUpdate.Builder Builder = null;

			try
			{
				Builder = PulsarMarkers.ReplicatedSubscriptionsUpdate.newBuilder();
				return Builder.mergeFrom(InStream, null).build();
			}
			finally
			{
				Builder.recycle();
				InStream.recycle();
			}
		}

		public static bool IsTxnCommitMarker(MessageMetadata MsgMetadata)
		{
			return MsgMetadata != null && MsgMetadata.hasMarkerType() && MsgMetadata.MarkerType == PulsarMarkers.MarkerType.TXN_COMMIT_VALUE;
		}

		public static ByteBuf NewTxnCommitMarker(long SequenceId, long TxnMostBits, long TxnLeastBits, PulsarMarkers.MessageIdData MessageIdData)
		{
			return NewTxnMarker(PulsarMarkers.MarkerType.TXN_COMMIT, SequenceId, TxnMostBits, TxnLeastBits, MessageIdData);
		}

		public static bool IsTxnAbortMarker(MessageMetadata MsgMetadata)
		{
			return MsgMetadata != null && MsgMetadata.hasMarkerType() && MsgMetadata.MarkerType == PulsarMarkers.MarkerType.TXN_ABORT_VALUE;
		}

		public static ByteBuf NewTxnAbortMarker(long SequenceId, long TxnMostBits, long TxnLeastBits)
		{
			return NewTxnMarker(PulsarMarkers.MarkerType.TXN_ABORT, SequenceId, TxnMostBits, TxnLeastBits, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static SharpPulsar.api.proto.PulsarMarkers.TxnCommitMarker parseCommitMarker(io.netty.buffer.ByteBuf payload) throws java.io.IOException
		public static PulsarMarkers.TxnCommitMarker ParseCommitMarker(ByteBuf Payload)
		{
			ByteBufCodedInputStream InStream = ByteBufCodedInputStream.get(Payload);

			PulsarMarkers.TxnCommitMarker.Builder Builder = null;

			try
			{
				Builder = PulsarMarkers.TxnCommitMarker.newBuilder();
				return Builder.mergeFrom(InStream, null).build();
			}
			finally
			{
				Builder.recycle();
				InStream.recycle();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SneakyThrows private static io.netty.buffer.ByteBuf newTxnMarker(SharpPulsar.api.proto.PulsarMarkers.MarkerType markerType, long sequenceId, long txnMostBits, long txnLeastBits, java.util.Optional<SharpPulsar.api.proto.PulsarMarkers.MessageIdData> messageIdData)
		private static ByteBuf NewTxnMarker(PulsarMarkers.MarkerType MarkerType, long SequenceId, long TxnMostBits, long TxnLeastBits, Optional<PulsarMarkers.MessageIdData> MessageIdData)
		{
			MessageMetadata.Builder MsgMetadataBuilder = MessageMetadata.newBuilder();
			MsgMetadataBuilder.PublishTime = DateTimeHelper.CurrentUnixTimeMillis();
			MsgMetadataBuilder.setProducerName("pulsar.txn.marker");
			MsgMetadataBuilder.SequenceId = SequenceId;
			MsgMetadataBuilder.MarkerType = MarkerType.Number;
			MsgMetadataBuilder.TxnidMostBits = TxnMostBits;
			MsgMetadataBuilder.TxnidLeastBits = TxnLeastBits;

			MessageMetadata MsgMetadata = MsgMetadataBuilder.build();

			ByteBuf Payload;
			if (MessageIdData.Present)
			{
				PulsarMarkers.TxnCommitMarker CommitMarker = PulsarMarkers.TxnCommitMarker.newBuilder().setMessageId(MessageIdData.get()).build();
				int Size = CommitMarker.SerializedSize;
				Payload = PooledByteBufAllocator.DEFAULT.buffer(Size);
				ByteBufCodedOutputStream OutStream = ByteBufCodedOutputStream.get(Payload);
				CommitMarker.writeTo(OutStream);
			}
			else
			{
				Payload = PooledByteBufAllocator.DEFAULT.buffer();
			}

			try
			{
				return Commands.SerializeMetadataAndPayload(ChecksumType.Crc32c, MsgMetadata, Payload);
			}
			finally
			{
				Payload.release();
				MsgMetadata.recycle();
				MsgMetadataBuilder.recycle();
			}
		}
	}

}