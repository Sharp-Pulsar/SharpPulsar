using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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
namespace Org.Apache.Pulsar.Client.Admin.@internal
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	using Lists = com.google.common.collect.Lists;
	using Maps = com.google.common.collect.Maps;
	using Gson = com.google.gson.Gson;
	using JsonObject = com.google.gson.JsonObject;

	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;



	using NotFoundException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.NotFoundException;
	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using Org.Apache.Pulsar.Client.Api;
	using MessageId = Org.Apache.Pulsar.Client.Api.MessageId;
	using Org.Apache.Pulsar.Client.Api;
	using MessageIdImpl = Org.Apache.Pulsar.Client.Impl.MessageIdImpl;
	using Org.Apache.Pulsar.Client.Impl;
	using Commands = Org.Apache.Pulsar.Common.Protocol.Commands;
	using PulsarApi = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi;
	using KeyValue = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.KeyValue;
	using SingleMessageMetadata = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.SingleMessageMetadata;
	using NamespaceName = Org.Apache.Pulsar.Common.Naming.NamespaceName;
	using TopicName = Org.Apache.Pulsar.Common.Naming.TopicName;
	using PartitionedTopicMetadata = Org.Apache.Pulsar.Common.Partition.PartitionedTopicMetadata;
	using AuthAction = Org.Apache.Pulsar.Common.Policies.Data.AuthAction;
	using ErrorData = Org.Apache.Pulsar.Common.Policies.Data.ErrorData;
	using PartitionedTopicInternalStats = Org.Apache.Pulsar.Common.Policies.Data.PartitionedTopicInternalStats;
	using PartitionedTopicStats = Org.Apache.Pulsar.Common.Policies.Data.PartitionedTopicStats;
	using PersistentTopicInternalStats = Org.Apache.Pulsar.Common.Policies.Data.PersistentTopicInternalStats;
	using TopicStats = Org.Apache.Pulsar.Common.Policies.Data.TopicStats;
	using Codec = Org.Apache.Pulsar.Common.Util.Codec;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class TopicsImpl : BaseResource, Topics
	{
		private readonly WebTarget adminTopics;
		private readonly WebTarget adminV2Topics;
		private readonly string BATCH_HEADER = "X-Pulsar-num-batch-message";
		public TopicsImpl(WebTarget Web, Authentication Auth, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
			adminTopics = Web.path("/admin");
			adminV2Topics = Web.path("/admin/v2");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getList(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> GetList(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget PersistentPath = NamespacePath("persistent", Ns);
				WebTarget NonPersistentPath = NamespacePath("non-persistent", Ns);

				IList<string> PersistentTopics = Request(PersistentPath).get(new GenericTypeAnonymousInnerClass(this));

				IList<string> NonPersistentTopics = Request(NonPersistentPath).get(new GenericTypeAnonymousInnerClass2(this));
				return new List<string>(Stream.concat(PersistentTopics.stream(), NonPersistentTopics.stream()).collect(Collectors.toSet()));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass : GenericType<IList<string>>
		{
			private readonly TopicsImpl outerInstance;

			public GenericTypeAnonymousInnerClass(TopicsImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

		public class GenericTypeAnonymousInnerClass2 : GenericType<IList<string>>
		{
			private readonly TopicsImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(TopicsImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getPartitionedTopicList(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> GetPartitionedTopicList(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);

				WebTarget PersistentPath = NamespacePath("persistent", Ns, "partitioned");
				WebTarget NonPersistentPath = NamespacePath("non-persistent", Ns, "partitioned");
				IList<string> PersistentTopics = Request(PersistentPath).get(new GenericTypeAnonymousInnerClass3(this));

				IList<string> NonPersistentTopics = Request(NonPersistentPath).get(new GenericTypeAnonymousInnerClass4(this));
				return new List<string>(Stream.concat(PersistentTopics.stream(), NonPersistentTopics.stream()).collect(Collectors.toSet()));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass3 : GenericType<IList<string>>
		{
			private readonly TopicsImpl outerInstance;

			public GenericTypeAnonymousInnerClass3(TopicsImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

		public class GenericTypeAnonymousInnerClass4 : GenericType<IList<string>>
		{
			private readonly TopicsImpl outerInstance;

			public GenericTypeAnonymousInnerClass4(TopicsImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getListInBundle(String namespace, String bundleRange) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> GetListInBundle(string Namespace, string BundleRange)
		{
			try
			{
				return GetListInBundleAsync(Namespace, BundleRange).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<IList<string>> GetListInBundleAsync(string Namespace, string BundleRange)
		{
			NamespaceName Ns = NamespaceName.get(Namespace);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<java.util.List<String>> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<IList<string>> Future = new CompletableFuture<IList<string>>();
			WebTarget Path = NamespacePath("non-persistent", Ns, BundleRange);

			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass(this, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass : InvocationCallback<IList<string>>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<IList<string>> future;

			public InvocationCallbackAnonymousInnerClass(TopicsImpl OuterInstance, CompletableFuture<IList<string>> Future)
			{
				this.outerInstance = OuterInstance;
				this.future = Future;
			}

			public override void completed(IList<string> Response)
			{
				future.complete(Response);
			}
			public override void failed(Exception Throwable)
			{
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction>> getPermissions(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IDictionary<string, ISet<AuthAction>> GetPermissions(string Topic)
		{
			try
			{
				TopicName Tn = TopicName.get(Topic);
				WebTarget Path = TopicPath(Tn, "permissions");
				return Request(Path).get(new GenericTypeAnonymousInnerClass5(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass5 : GenericType<IDictionary<string, ISet<AuthAction>>>
		{
			private readonly TopicsImpl outerInstance;

			public GenericTypeAnonymousInnerClass5(TopicsImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void grantPermission(String topic, String role, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction> actions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void GrantPermission(string Topic, string Role, ISet<AuthAction> Actions)
		{
			try
			{
				TopicName Tn = TopicName.get(Topic);
				WebTarget Path = TopicPath(Tn, "permissions", Role);
				Request(Path).post(Entity.entity(Actions, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void revokePermissions(String topic, String role) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void RevokePermissions(string Topic, string Role)
		{
			try
			{
				TopicName Tn = TopicName.get(Topic);
				WebTarget Path = TopicPath(Tn, "permissions", Role);
				Request(Path).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createPartitionedTopic(String topic, int numPartitions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreatePartitionedTopic(string Topic, int NumPartitions)
		{
			try
			{
				CreatePartitionedTopicAsync(Topic, NumPartitions).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNonPartitionedTopic(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateNonPartitionedTopic(string Topic)
		{
			try
			{
				CreateNonPartitionedTopicAsync(Topic).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<Void> CreateNonPartitionedTopicAsync(string Topic)
		{
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn);
			return AsyncPutRequest(Path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

		public override CompletableFuture<Void> CreatePartitionedTopicAsync(string Topic, int NumPartitions)
		{
			checkArgument(NumPartitions > 0, "Number of partitions should be more than 0");
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn, "partitions");
			return AsyncPutRequest(Path, Entity.entity(NumPartitions, MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updatePartitionedTopic(String topic, int numPartitions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdatePartitionedTopic(string Topic, int NumPartitions)
		{
			try
			{
				UpdatePartitionedTopicAsync(Topic, NumPartitions).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<Void> UpdatePartitionedTopicAsync(string Topic, int NumPartitions)
		{
			return UpdatePartitionedTopicAsync(Topic, NumPartitions, false);
		}

		public override CompletableFuture<Void> UpdatePartitionedTopicAsync(string Topic, int NumPartitions, bool UpdateLocalTopicOnly)
		{
			checkArgument(NumPartitions > 0, "Number of partitions must be more than 0");
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn, "partitions");
			Path = Path.queryParam("updateLocalTopicOnly", Convert.ToString(UpdateLocalTopicOnly));
			return AsyncPostRequest(Path, Entity.entity(NumPartitions, MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.partition.PartitionedTopicMetadata getPartitionedTopicMetadata(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override PartitionedTopicMetadata GetPartitionedTopicMetadata(string Topic)
		{
			try
			{
				return GetPartitionedTopicMetadataAsync(Topic).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<PartitionedTopicMetadata> GetPartitionedTopicMetadataAsync(string Topic)
		{
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn, "partitions");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.partition.PartitionedTopicMetadata> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<PartitionedTopicMetadata> Future = new CompletableFuture<PartitionedTopicMetadata>();
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass2(this, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass2 : InvocationCallback<PartitionedTopicMetadata>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<PartitionedTopicMetadata> future;

			public InvocationCallbackAnonymousInnerClass2(TopicsImpl OuterInstance, CompletableFuture<PartitionedTopicMetadata> Future)
			{
				this.outerInstance = OuterInstance;
				this.future = Future;
			}


			public override void completed(PartitionedTopicMetadata Response)
			{
				future.complete(Response);
			}

			public override void failed(Exception Throwable)
			{
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deletePartitionedTopic(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeletePartitionedTopic(string Topic)
		{
			DeletePartitionedTopic(Topic, false);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deletePartitionedTopic(String topic, boolean force) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeletePartitionedTopic(string Topic, bool Force)
		{
			try
			{
				DeletePartitionedTopicAsync(Topic, Force).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<Void> DeletePartitionedTopicAsync(string Topic, bool Force)
		{
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn, "partitions");
			Path = Path.queryParam("force", Force);
			return AsyncDeleteRequest(Path);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void delete(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void Delete(string Topic)
		{
			Delete(Topic, false);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void delete(String topic, boolean force) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void Delete(string Topic, bool Force)
		{
			try
			{
				DeleteAsync(Topic, Force).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<Void> DeleteAsync(string Topic, bool Force)
		{
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn);
			Path = Path.queryParam("force", Convert.ToString(Force));
			return AsyncDeleteRequest(Path);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void unload(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void Unload(string Topic)
		{
			try
			{
				UnloadAsync(Topic).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<Void> UnloadAsync(string Topic)
		{
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn, "unload");
			return AsyncPutRequest(Path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getSubscriptions(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> GetSubscriptions(string Topic)
		{
			try
			{
				return GetSubscriptionsAsync(Topic).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<IList<string>> GetSubscriptionsAsync(string Topic)
		{
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn, "subscriptions");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<java.util.List<String>> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<IList<string>> Future = new CompletableFuture<IList<string>>();
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass3(this, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass3 : InvocationCallback<IList<string>>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<IList<string>> future;

			public InvocationCallbackAnonymousInnerClass3(TopicsImpl OuterInstance, CompletableFuture<IList<string>> Future)
			{
				this.outerInstance = OuterInstance;
				this.future = Future;
			}


			public override void completed(IList<string> Response)
			{
				future.complete(Response);
			}

			public override void failed(Exception Throwable)
			{
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.TopicStats getStats(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override TopicStats GetStats(string Topic)
		{
			try
			{
				return GetStatsAsync(Topic).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<TopicStats> GetStatsAsync(string Topic)
		{
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn, "stats");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.policies.data.TopicStats> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<TopicStats> Future = new CompletableFuture<TopicStats>();
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass4(this, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass4 : InvocationCallback<TopicStats>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<TopicStats> future;

			public InvocationCallbackAnonymousInnerClass4(TopicsImpl OuterInstance, CompletableFuture<TopicStats> Future)
			{
				this.outerInstance = OuterInstance;
				this.future = Future;
			}


			public override void completed(TopicStats Response)
			{
				future.complete(Response);
			}

			public override void failed(Exception Throwable)
			{
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.PersistentTopicInternalStats getInternalStats(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override PersistentTopicInternalStats GetInternalStats(string Topic)
		{
			try
			{
				return GetInternalStatsAsync(Topic).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<PersistentTopicInternalStats> GetInternalStatsAsync(string Topic)
		{
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn, "internalStats");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.policies.data.PersistentTopicInternalStats> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<PersistentTopicInternalStats> Future = new CompletableFuture<PersistentTopicInternalStats>();
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass5(this, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass5 : InvocationCallback<PersistentTopicInternalStats>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<PersistentTopicInternalStats> future;

			public InvocationCallbackAnonymousInnerClass5(TopicsImpl OuterInstance, CompletableFuture<PersistentTopicInternalStats> Future)
			{
				this.outerInstance = OuterInstance;
				this.future = Future;
			}


			public override void completed(PersistentTopicInternalStats Response)
			{
				future.complete(Response);
			}

			public override void failed(Exception Throwable)
			{
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public com.google.gson.JsonObject getInternalInfo(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override JsonObject GetInternalInfo(string Topic)
		{
			try
			{
				return GetInternalInfoAsync(Topic).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<JsonObject> GetInternalInfoAsync(string Topic)
		{
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn, "internal-info");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<com.google.gson.JsonObject> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<JsonObject> Future = new CompletableFuture<JsonObject>();
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass6(this, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass6 : InvocationCallback<string>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<JsonObject> future;

			public InvocationCallbackAnonymousInnerClass6(TopicsImpl OuterInstance, CompletableFuture<JsonObject> Future)
			{
				this.outerInstance = OuterInstance;
				this.future = Future;
			}

			public override void completed(string Response)
			{
				JsonObject Json = (new Gson()).fromJson(Response, typeof(JsonObject));
				future.complete(Json);
			}

			public override void failed(Exception Throwable)
			{
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.PartitionedTopicStats getPartitionedStats(String topic, boolean perPartition) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override PartitionedTopicStats GetPartitionedStats(string Topic, bool PerPartition)
		{
			try
			{
				return GetPartitionedStatsAsync(Topic, PerPartition).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<PartitionedTopicStats> GetPartitionedStatsAsync(string Topic, bool PerPartition)
		{
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn, "partitioned-stats");
			Path = Path.queryParam("perPartition", PerPartition);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.policies.data.PartitionedTopicStats> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<PartitionedTopicStats> Future = new CompletableFuture<PartitionedTopicStats>();
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass7(this, PerPartition, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass7 : InvocationCallback<PartitionedTopicStats>
		{
			private readonly TopicsImpl outerInstance;

			private bool perPartition;
			private CompletableFuture<PartitionedTopicStats> future;

			public InvocationCallbackAnonymousInnerClass7(TopicsImpl OuterInstance, bool PerPartition, CompletableFuture<PartitionedTopicStats> Future)
			{
				this.outerInstance = OuterInstance;
				this.perPartition = PerPartition;
				this.future = Future;
			}


			public override void completed(PartitionedTopicStats Response)
			{
				if (!perPartition)
				{
					Response.Partitions.Clear();
				}
				future.complete(Response);
			}

			public override void failed(Exception Throwable)
			{
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.PartitionedTopicInternalStats getPartitionedInternalStats(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override PartitionedTopicInternalStats GetPartitionedInternalStats(string Topic)
		{
			try
			{
				return GetPartitionedInternalStatsAsync(Topic).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<PartitionedTopicInternalStats> GetPartitionedInternalStatsAsync(string Topic)
		{
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn, "partitioned-internalStats");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.policies.data.PartitionedTopicInternalStats> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<PartitionedTopicInternalStats> Future = new CompletableFuture<PartitionedTopicInternalStats>();
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass8(this, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass8 : InvocationCallback<PartitionedTopicInternalStats>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<PartitionedTopicInternalStats> future;

			public InvocationCallbackAnonymousInnerClass8(TopicsImpl OuterInstance, CompletableFuture<PartitionedTopicInternalStats> Future)
			{
				this.outerInstance = OuterInstance;
				this.future = Future;
			}


			public override void completed(PartitionedTopicInternalStats Response)
			{
				future.complete(Response);
			}

			public override void failed(Exception Throwable)
			{
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteSubscription(String topic, String subName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteSubscription(string Topic, string SubName)
		{
			try
			{
				DeleteSubscriptionAsync(Topic, SubName).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<Void> DeleteSubscriptionAsync(string Topic, string SubName)
		{
			TopicName Tn = ValidateTopic(Topic);
			string EncodedSubName = Codec.encode(SubName);
			WebTarget Path = TopicPath(Tn, "subscription", EncodedSubName);
			return AsyncDeleteRequest(Path);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void skipAllMessages(String topic, String subName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SkipAllMessages(string Topic, string SubName)
		{
			try
			{
				SkipAllMessagesAsync(Topic, SubName).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<Void> SkipAllMessagesAsync(string Topic, string SubName)
		{
			TopicName Tn = ValidateTopic(Topic);
			string EncodedSubName = Codec.encode(SubName);
			WebTarget Path = TopicPath(Tn, "subscription", EncodedSubName, "skip_all");
			return AsyncPostRequest(Path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void skipMessages(String topic, String subName, long numMessages) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SkipMessages(string Topic, string SubName, long NumMessages)
		{
			try
			{
				SkipMessagesAsync(Topic, SubName, NumMessages).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<Void> SkipMessagesAsync(string Topic, string SubName, long NumMessages)
		{
			TopicName Tn = ValidateTopic(Topic);
			string EncodedSubName = Codec.encode(SubName);
			WebTarget Path = TopicPath(Tn, "subscription", EncodedSubName, "skip", NumMessages.ToString());
			return AsyncPostRequest(Path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void expireMessages(String topic, String subName, long expireTimeInSeconds) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void ExpireMessages(string Topic, string SubName, long ExpireTimeInSeconds)
		{
			try
			{
				ExpireMessagesAsync(Topic, SubName, ExpireTimeInSeconds).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<Void> ExpireMessagesAsync(string Topic, string SubName, long ExpireTimeInSeconds)
		{
			TopicName Tn = ValidateTopic(Topic);
			string EncodedSubName = Codec.encode(SubName);
			WebTarget Path = TopicPath(Tn, "subscription", EncodedSubName, "expireMessages", ExpireTimeInSeconds.ToString());
			return AsyncPostRequest(Path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void expireMessagesForAllSubscriptions(String topic, long expireTimeInSeconds) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void ExpireMessagesForAllSubscriptions(string Topic, long ExpireTimeInSeconds)
		{
			try
			{
				ExpireMessagesForAllSubscriptionsAsync(Topic, ExpireTimeInSeconds).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<Void> ExpireMessagesForAllSubscriptionsAsync(string Topic, long ExpireTimeInSeconds)
		{
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn, "all_subscription", "expireMessages", ExpireTimeInSeconds.ToString());
			return AsyncPostRequest(Path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

		private CompletableFuture<IList<Message<sbyte[]>>> PeekNthMessage(string Topic, string SubName, int MessagePosition)
		{
			TopicName Tn = ValidateTopic(Topic);
			string EncodedSubName = Codec.encode(SubName);
			WebTarget Path = TopicPath(Tn, "subscription", EncodedSubName, "position", MessagePosition.ToString());
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<java.util.List<org.apache.pulsar.client.api.Message<byte[]>>> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<IList<Message<sbyte[]>>> Future = new CompletableFuture<IList<Message<sbyte[]>>>();
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass9(this, Tn, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass9 : InvocationCallback<Response>
		{
			private readonly TopicsImpl outerInstance;

			private TopicName tn;
			private CompletableFuture<IList<Message<sbyte[]>>> future;

			public InvocationCallbackAnonymousInnerClass9(TopicsImpl OuterInstance, TopicName Tn, CompletableFuture<IList<Message<sbyte[]>>> Future)
			{
				this.outerInstance = OuterInstance;
				this.tn = Tn;
				this.future = Future;
			}


			public override void completed(Response Response)
			{
				try
				{
					future.complete(outerInstance.getMessageFromHttpResponse(tn.ToString(), Response));
				}
				catch (Exception E)
				{
					future.completeExceptionally(outerInstance.GetApiException(E));
				}
			}

			public override void failed(Exception Throwable)
			{
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<org.apache.pulsar.client.api.Message<byte[]>> peekMessages(String topic, String subName, int numMessages) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<Message<sbyte[]>> PeekMessages(string Topic, string SubName, int NumMessages)
		{
			try
			{
				return PeekMessagesAsync(Topic, SubName, NumMessages).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public override CompletableFuture<IList<Message<sbyte[]>>> PeekMessagesAsync(string Topic, string SubName, int NumMessages)
		{
			checkArgument(NumMessages > 0);
			CompletableFuture<IList<Message<sbyte[]>>> Future = new CompletableFuture<IList<Message<sbyte[]>>>();
			PeekMessagesAsync(Topic, SubName, NumMessages, Lists.newArrayList(), Future, 1);
			return Future;
		}


		private void PeekMessagesAsync(string Topic, string SubName, int NumMessages, IList<Message<sbyte[]>> Messages, CompletableFuture<IList<Message<sbyte[]>>> Future, int NthMessage)
		{
			if (NumMessages <= 0)
			{
				Future.complete(Messages);
				return;
			}

			// if peeking first message succeeds, we know that the topic and subscription exists
			PeekNthMessage(Topic, SubName, NthMessage).handle((r, ex) =>
			{
			if (ex != null)
			{
				if (ex is NotFoundException)
				{
					log.warn("Exception '{}' occured while trying to peek Messages.", ex.Message);
					Future.complete(Messages);
				}
				else
				{
					Future.completeExceptionally(ex);
				}
				return null;
			}
			for (int I = 0; I < Math.Min(r.size(), NumMessages); I++)
			{
				Messages.Add(r.get(I));
			}
			PeekMessagesAsync(Topic, SubName, NumMessages - r.size(), Messages, Future, NthMessage + 1);
			return null;
			});
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createSubscription(String topic, String subscriptionName, org.apache.pulsar.client.api.MessageId messageId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateSubscription(string Topic, string SubscriptionName, MessageId MessageId)
		{
			try
			{
				TopicName Tn = ValidateTopic(Topic);
				string EncodedSubName = Codec.encode(SubscriptionName);
				WebTarget Path = TopicPath(Tn, "subscription", EncodedSubName);
				Request(Path).put(Entity.entity(MessageId, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public override CompletableFuture<Void> CreateSubscriptionAsync(string Topic, string SubscriptionName, MessageId MessageId)
		{
			TopicName Tn = ValidateTopic(Topic);
			string EncodedSubName = Codec.encode(SubscriptionName);
			WebTarget Path = TopicPath(Tn, "subscription", EncodedSubName);
			return AsyncPutRequest(Path, Entity.entity(MessageId, MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void resetCursor(String topic, String subName, long timestamp) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void ResetCursor(string Topic, string SubName, long Timestamp)
		{
			try
			{
				TopicName Tn = ValidateTopic(Topic);
				string EncodedSubName = Codec.encode(SubName);
				WebTarget Path = TopicPath(Tn, "subscription", EncodedSubName, "resetcursor", Timestamp.ToString());
				Request(Path).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public override CompletableFuture<Void> ResetCursorAsync(string Topic, string SubName, long Timestamp)
		{
			TopicName Tn = ValidateTopic(Topic);
			string EncodedSubName = Codec.encode(SubName);
			WebTarget Path = TopicPath(Tn, "subscription", EncodedSubName, "resetcursor", Timestamp.ToString());
			return AsyncPostRequest(Path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void resetCursor(String topic, String subName, org.apache.pulsar.client.api.MessageId messageId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void ResetCursor(string Topic, string SubName, MessageId MessageId)
		{
			try
			{
				TopicName Tn = ValidateTopic(Topic);
				string EncodedSubName = Codec.encode(SubName);
				WebTarget Path = TopicPath(Tn, "subscription", EncodedSubName, "resetcursor");
				Request(Path).post(Entity.entity(MessageId, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public override CompletableFuture<Void> ResetCursorAsync(string Topic, string SubName, MessageId MessageId)
		{
			TopicName Tn = ValidateTopic(Topic);
			string EncodedSubName = Codec.encode(SubName);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget path = topicPath(tn, "subscription", encodedSubName, "resetcursor");
			WebTarget Path = TopicPath(Tn, "subscription", EncodedSubName, "resetcursor");
			return AsyncPostRequest(Path, Entity.entity(MessageId, MediaType.APPLICATION_JSON));
		}

		public override CompletableFuture<MessageId> TerminateTopicAsync(string Topic)
		{
			TopicName Tn = ValidateTopic(Topic);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.client.api.MessageId> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<MessageId> Future = new CompletableFuture<MessageId>();
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget path = topicPath(tn, "terminate");
				WebTarget Path = TopicPath(Tn, "terminate");

				Request(Path).async().post(Entity.entity("", MediaType.APPLICATION_JSON), new InvocationCallbackAnonymousInnerClass10(this, Future, Path));
			}
			catch (PulsarAdminException Cae)
			{
				Future.completeExceptionally(Cae);
			}

			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass10 : InvocationCallback<MessageIdImpl>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<MessageId> future;
			private WebTarget path;

			public InvocationCallbackAnonymousInnerClass10(TopicsImpl OuterInstance, CompletableFuture<MessageId> Future, WebTarget Path)
			{
				this.outerInstance = OuterInstance;
				this.future = Future;
				this.path = Path;
			}


			public override void completed(MessageIdImpl MessageId)
			{
				future.complete(MessageId);
			}

			public override void failed(Exception Throwable)
			{
				log.warn("[{}] Failed to perform http post request: {}", path.Uri, Throwable.Message);
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void triggerCompaction(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void TriggerCompaction(string Topic)
		{
			try
			{
				TopicName Tn = ValidateTopic(Topic);
				Request(TopicPath(Tn, "compaction")).put(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.admin.LongRunningProcessStatus compactionStatus(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override LongRunningProcessStatus CompactionStatus(string Topic)
		{
			try
			{
				TopicName Tn = ValidateTopic(Topic);
				return Request(TopicPath(Tn, "compaction")).get(typeof(LongRunningProcessStatus));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void triggerOffload(String topic, org.apache.pulsar.client.api.MessageId messageId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void TriggerOffload(string Topic, MessageId MessageId)
		{
			try
			{
				TopicName Tn = ValidateTopic(Topic);
				WebTarget Path = TopicPath(Tn, "offload");
				Request(Path).put(Entity.entity(MessageId, MediaType.APPLICATION_JSON), typeof(MessageIdImpl));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.admin.OffloadProcessStatus offloadStatus(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override OffloadProcessStatus OffloadStatus(string Topic)
		{
			try
			{
				TopicName Tn = ValidateTopic(Topic);
				return Request(TopicPath(Tn, "offload")).get(typeof(OffloadProcessStatus));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		private WebTarget NamespacePath(string Domain, NamespaceName Namespace, params string[] Parts)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget super = namespace.isV2() ? adminV2Topics : adminTopics;
			WebTarget Base = Namespace.V2 ? adminV2Topics : adminTopics;
			WebTarget NamespacePath = Base.path(Domain).path(Namespace.ToString());
			NamespacePath = WebTargets.AddParts(NamespacePath, Parts);
			return NamespacePath;
		}

		private WebTarget TopicPath(TopicName Topic, params string[] Parts)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget super = topic.isV2() ? adminV2Topics : adminTopics;
			WebTarget Base = Topic.V2 ? adminV2Topics : adminTopics;
			WebTarget TopicPath = Base.path(Topic.RestPath);
			TopicPath = WebTargets.AddParts(TopicPath, Parts);
			return TopicPath;
		}

		/*
		 * returns topic name with encoded Local Name
		 */
		private TopicName ValidateTopic(string Topic)
		{
			// Parsing will throw exception if name is not valid
			return TopicName.get(Topic);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<org.apache.pulsar.client.api.Message<byte[]>> getMessageFromHttpResponse(String topic, javax.ws.rs.core.Response response) throws Exception
		private IList<Message<sbyte[]>> GetMessageFromHttpResponse(string Topic, Response Response)
		{

			if (Response.Status != Response.Status.OK.StatusCode)
			{
				throw GetApiException(Response);
			}

			string MsgId = Response.getHeaderString("X-Pulsar-Message-ID");
			using (Stream Stream = (Stream) Response.Entity)
			{
				sbyte[] Data = new sbyte[Stream.available()];
				Stream.Read(Data, 0, Data.Length);

				IDictionary<string, string> Properties = Maps.newTreeMap();
				MultivaluedMap<string, object> Headers = Response.Headers;
				object Tmp = Headers.getFirst("X-Pulsar-publish-time");
				if (Tmp != null)
				{
					Properties["publish-time"] = (string) Tmp;
				}
				Tmp = Headers.getFirst(BATCH_HEADER);
				if (Response.getHeaderString(BATCH_HEADER) != null)
				{
					Properties[BATCH_HEADER] = (string) Tmp;
					return GetIndividualMsgsFromBatch(Topic, MsgId, Data, Properties);
				}
				foreach (KeyValuePair<string, IList<object>> Entry in Headers.entrySet())
				{
					string Header = Entry.Key;
					if (Header.Contains("X-Pulsar-PROPERTY-"))
					{
						string KeyName = StringHelper.SubstringSpecial(Header, "X-Pulsar-PROPERTY-".Length, Header.Length);
						Properties[KeyName] = (string) Entry.Value.get(0);
					}
				}

				return Collections.singletonList(new MessageImpl<sbyte[]>(Topic, MsgId, Properties, Unpooled.wrappedBuffer(Data), SchemaFields.BYTES));
			}
		}

		private IList<Message<sbyte[]>> GetIndividualMsgsFromBatch(string Topic, string MsgId, sbyte[] Data, IDictionary<string, string> Properties)
		{
			IList<Message<sbyte[]>> Ret = new List<Message<sbyte[]>>();
			int BatchSize = int.Parse(Properties[BATCH_HEADER]);
			for (int I = 0; I < BatchSize; I++)
			{
				string BatchMsgId = MsgId + ":" + I;
				PulsarApi.SingleMessageMetadata.Builder SingleMessageMetadataBuilder = PulsarApi.SingleMessageMetadata.newBuilder();
				ByteBuf Buf = Unpooled.wrappedBuffer(Data);
				try
				{
					ByteBuf SingleMessagePayload = Commands.deSerializeSingleMessageInBatch(Buf, SingleMessageMetadataBuilder, I, BatchSize);
					PulsarApi.SingleMessageMetadata SingleMessageMetadata = SingleMessageMetadataBuilder.build();
					if (SingleMessageMetadata.PropertiesCount > 0)
					{
						foreach (PulsarApi.KeyValue Entry in SingleMessageMetadata.PropertiesList)
						{
							Properties[Entry.Key] = Entry.Value;
						}
					}
					Ret.Add(new MessageImpl<>(Topic, BatchMsgId, Properties, SingleMessagePayload, SchemaFields.BYTES));
				}
				catch (Exception Ex)
				{
					log.error("Exception occured while trying to get BatchMsgId: {}", BatchMsgId, Ex);
				}
				Buf.release();
				SingleMessageMetadataBuilder.recycle();
			}
			return Ret;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.MessageId getLastMessageId(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override MessageId GetLastMessageId(string Topic)
		{
			try
			{
				return GetLastMessageIdAsync(Topic).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException E)
			{
				throw (PulsarAdminException) E.InnerException;
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(E);
			}
			catch (TimeoutException E)
			{
				throw new PulsarAdminException.TimeoutException(E);
			}
		}

		public virtual CompletableFuture<MessageId> GetLastMessageIdAsync(string Topic)
		{
			TopicName Tn = ValidateTopic(Topic);
			WebTarget Path = TopicPath(Tn, "lastMessageId");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.client.api.MessageId> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<MessageId> Future = new CompletableFuture<MessageId>();
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass11(this, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass11 : InvocationCallback<MessageIdImpl>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<MessageId> future;

			public InvocationCallbackAnonymousInnerClass11(TopicsImpl OuterInstance, CompletableFuture<MessageId> Future)
			{
				this.outerInstance = OuterInstance;
				this.future = Future;
			}


			public override void completed(MessageIdImpl Response)
			{
				future.complete(Response);
			}

			public override void failed(Exception Throwable)
			{
				future.completeExceptionally(outerInstance.GetApiException(Throwable.InnerException));
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(TopicsImpl));
	}

}