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
namespace org.apache.pulsar.client.admin.@internal
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	using Lists = com.google.common.collect.Lists;
	using Maps = com.google.common.collect.Maps;
	using Gson = com.google.gson.Gson;
	using JsonObject = com.google.gson.JsonObject;

	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;



	using NotFoundException = org.apache.pulsar.client.admin.PulsarAdminException.NotFoundException;
	using Authentication = org.apache.pulsar.client.api.Authentication;
	using Message = org.apache.pulsar.client.api.Message;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using Schema = org.apache.pulsar.client.api.Schema;
	using MessageIdImpl = org.apache.pulsar.client.impl.MessageIdImpl;
	using MessageImpl = org.apache.pulsar.client.impl.MessageImpl;
	using Commands = org.apache.pulsar.common.protocol.Commands;
	using PulsarApi = org.apache.pulsar.common.api.proto.PulsarApi;
	using KeyValue = org.apache.pulsar.common.api.proto.PulsarApi.KeyValue;
	using SingleMessageMetadata = org.apache.pulsar.common.api.proto.PulsarApi.SingleMessageMetadata;
	using NamespaceName = org.apache.pulsar.common.naming.NamespaceName;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using PartitionedTopicMetadata = org.apache.pulsar.common.partition.PartitionedTopicMetadata;
	using AuthAction = org.apache.pulsar.common.policies.data.AuthAction;
	using ErrorData = org.apache.pulsar.common.policies.data.ErrorData;
	using PartitionedTopicInternalStats = org.apache.pulsar.common.policies.data.PartitionedTopicInternalStats;
	using PartitionedTopicStats = org.apache.pulsar.common.policies.data.PartitionedTopicStats;
	using PersistentTopicInternalStats = org.apache.pulsar.common.policies.data.PersistentTopicInternalStats;
	using TopicStats = org.apache.pulsar.common.policies.data.TopicStats;
	using Codec = org.apache.pulsar.common.util.Codec;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class TopicsImpl : BaseResource, Topics
	{
		private readonly WebTarget adminTopics;
		private readonly WebTarget adminV2Topics;
		private readonly string BATCH_HEADER = "X-Pulsar-num-batch-message";
		public TopicsImpl(WebTarget web, Authentication auth, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			adminTopics = web.path("/admin");
			adminV2Topics = web.path("/admin/v2");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getList(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> getList(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget persistentPath = namespacePath("persistent", ns);
				WebTarget nonPersistentPath = namespacePath("non-persistent", ns);

				IList<string> persistentTopics = request(persistentPath).get(new GenericTypeAnonymousInnerClass(this));

				IList<string> nonPersistentTopics = request(nonPersistentPath).get(new GenericTypeAnonymousInnerClass2(this));
				return new List<string>(Stream.concat(persistentTopics.stream(), nonPersistentTopics.stream()).collect(Collectors.toSet()));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass : GenericType<IList<string>>
		{
			private readonly TopicsImpl outerInstance;

			public GenericTypeAnonymousInnerClass(TopicsImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

		private class GenericTypeAnonymousInnerClass2 : GenericType<IList<string>>
		{
			private readonly TopicsImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(TopicsImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getPartitionedTopicList(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> getPartitionedTopicList(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);

				WebTarget persistentPath = namespacePath("persistent", ns, "partitioned");
				WebTarget nonPersistentPath = namespacePath("non-persistent", ns, "partitioned");
				IList<string> persistentTopics = request(persistentPath).get(new GenericTypeAnonymousInnerClass3(this));

				IList<string> nonPersistentTopics = request(nonPersistentPath).get(new GenericTypeAnonymousInnerClass4(this));
				return new List<string>(Stream.concat(persistentTopics.stream(), nonPersistentTopics.stream()).collect(Collectors.toSet()));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass3 : GenericType<IList<string>>
		{
			private readonly TopicsImpl outerInstance;

			public GenericTypeAnonymousInnerClass3(TopicsImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

		private class GenericTypeAnonymousInnerClass4 : GenericType<IList<string>>
		{
			private readonly TopicsImpl outerInstance;

			public GenericTypeAnonymousInnerClass4(TopicsImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getListInBundle(String namespace, String bundleRange) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> getListInBundle(string @namespace, string bundleRange)
		{
			try
			{
				return getListInBundleAsync(@namespace, bundleRange).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<IList<string>> getListInBundleAsync(string @namespace, string bundleRange)
		{
			NamespaceName ns = NamespaceName.get(@namespace);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<java.util.List<String>> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<IList<string>> future = new CompletableFuture<IList<string>>();
			WebTarget path = namespacePath("non-persistent", ns, bundleRange);

			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass(this, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass : InvocationCallback<IList<string>>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<IList<string>> future;

			public InvocationCallbackAnonymousInnerClass(TopicsImpl outerInstance, CompletableFuture<IList<string>> future)
			{
				this.outerInstance = outerInstance;
				this.future = future;
			}

			public override void completed(IList<string> response)
			{
				future.complete(response);
			}
			public override void failed(Exception throwable)
			{
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction>> getPermissions(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IDictionary<string, ISet<AuthAction>> getPermissions(string topic)
		{
			try
			{
				TopicName tn = TopicName.get(topic);
				WebTarget path = topicPath(tn, "permissions");
				return request(path).get(new GenericTypeAnonymousInnerClass5(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass5 : GenericType<IDictionary<string, ISet<AuthAction>>>
		{
			private readonly TopicsImpl outerInstance;

			public GenericTypeAnonymousInnerClass5(TopicsImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void grantPermission(String topic, String role, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction> actions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void grantPermission(string topic, string role, ISet<AuthAction> actions)
		{
			try
			{
				TopicName tn = TopicName.get(topic);
				WebTarget path = topicPath(tn, "permissions", role);
				request(path).post(Entity.entity(actions, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void revokePermissions(String topic, String role) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void revokePermissions(string topic, string role)
		{
			try
			{
				TopicName tn = TopicName.get(topic);
				WebTarget path = topicPath(tn, "permissions", role);
				request(path).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createPartitionedTopic(String topic, int numPartitions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createPartitionedTopic(string topic, int numPartitions)
		{
			try
			{
				createPartitionedTopicAsync(topic, numPartitions).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNonPartitionedTopic(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createNonPartitionedTopic(string topic)
		{
			try
			{
				createNonPartitionedTopicAsync(topic).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createMissedPartitions(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createMissedPartitions(string topic)
		{
			try
			{
				createMissedPartitionsAsync(topic).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<Void> createNonPartitionedTopicAsync(string topic)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn);
			return asyncPutRequest(path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

		public virtual CompletableFuture<Void> createPartitionedTopicAsync(string topic, int numPartitions)
		{
			checkArgument(numPartitions > 0, "Number of partitions should be more than 0");
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "partitions");
			return asyncPutRequest(path, Entity.entity(numPartitions, MediaType.APPLICATION_JSON));
		}

		public virtual CompletableFuture<Void> createMissedPartitionsAsync(string topic)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "createMissedPartitions");
			return asyncPostRequest(path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updatePartitionedTopic(String topic, int numPartitions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updatePartitionedTopic(string topic, int numPartitions)
		{
			try
			{
				updatePartitionedTopicAsync(topic, numPartitions).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<Void> updatePartitionedTopicAsync(string topic, int numPartitions)
		{
			return updatePartitionedTopicAsync(topic, numPartitions, false);
		}

		public virtual CompletableFuture<Void> updatePartitionedTopicAsync(string topic, int numPartitions, bool updateLocalTopicOnly)
		{
			checkArgument(numPartitions > 0, "Number of partitions must be more than 0");
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "partitions");
			path = path.queryParam("updateLocalTopicOnly", Convert.ToString(updateLocalTopicOnly));
			return asyncPostRequest(path, Entity.entity(numPartitions, MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.partition.PartitionedTopicMetadata getPartitionedTopicMetadata(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual PartitionedTopicMetadata getPartitionedTopicMetadata(string topic)
		{
			try
			{
				return getPartitionedTopicMetadataAsync(topic).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<PartitionedTopicMetadata> getPartitionedTopicMetadataAsync(string topic)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "partitions");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.partition.PartitionedTopicMetadata> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<PartitionedTopicMetadata> future = new CompletableFuture<PartitionedTopicMetadata>();
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass2(this, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass2 : InvocationCallback<PartitionedTopicMetadata>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<PartitionedTopicMetadata> future;

			public InvocationCallbackAnonymousInnerClass2(TopicsImpl outerInstance, CompletableFuture<PartitionedTopicMetadata> future)
			{
				this.outerInstance = outerInstance;
				this.future = future;
			}


			public override void completed(PartitionedTopicMetadata response)
			{
				future.complete(response);
			}

			public override void failed(Exception throwable)
			{
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deletePartitionedTopic(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deletePartitionedTopic(string topic)
		{
			deletePartitionedTopic(topic, false);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deletePartitionedTopic(String topic, boolean force) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deletePartitionedTopic(string topic, bool force)
		{
			try
			{
				deletePartitionedTopicAsync(topic, force).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<Void> deletePartitionedTopicAsync(string topic, bool force)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "partitions");
			path = path.queryParam("force", force);
			return asyncDeleteRequest(path);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void delete(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void delete(string topic)
		{
			delete(topic, false);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void delete(String topic, boolean force) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void delete(string topic, bool force)
		{
			try
			{
				deleteAsync(topic, force).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<Void> deleteAsync(string topic, bool force)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn);
			path = path.queryParam("force", Convert.ToString(force));
			return asyncDeleteRequest(path);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void unload(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void unload(string topic)
		{
			try
			{
				unloadAsync(topic).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<Void> unloadAsync(string topic)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "unload");
			return asyncPutRequest(path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getSubscriptions(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> getSubscriptions(string topic)
		{
			try
			{
				return getSubscriptionsAsync(topic).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<IList<string>> getSubscriptionsAsync(string topic)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "subscriptions");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<java.util.List<String>> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<IList<string>> future = new CompletableFuture<IList<string>>();
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass3(this, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass3 : InvocationCallback<IList<string>>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<IList<string>> future;

			public InvocationCallbackAnonymousInnerClass3(TopicsImpl outerInstance, CompletableFuture<IList<string>> future)
			{
				this.outerInstance = outerInstance;
				this.future = future;
			}


			public override void completed(IList<string> response)
			{
				future.complete(response);
			}

			public override void failed(Exception throwable)
			{
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.TopicStats getStats(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual TopicStats getStats(string topic)
		{
			try
			{
				return getStatsAsync(topic).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<TopicStats> getStatsAsync(string topic)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "stats");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.policies.data.TopicStats> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<TopicStats> future = new CompletableFuture<TopicStats>();
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass4(this, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass4 : InvocationCallback<TopicStats>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<TopicStats> future;

			public InvocationCallbackAnonymousInnerClass4(TopicsImpl outerInstance, CompletableFuture<TopicStats> future)
			{
				this.outerInstance = outerInstance;
				this.future = future;
			}


			public override void completed(TopicStats response)
			{
				future.complete(response);
			}

			public override void failed(Exception throwable)
			{
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.PersistentTopicInternalStats getInternalStats(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual PersistentTopicInternalStats getInternalStats(string topic)
		{
			try
			{
				return getInternalStatsAsync(topic).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<PersistentTopicInternalStats> getInternalStatsAsync(string topic)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "internalStats");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.policies.data.PersistentTopicInternalStats> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<PersistentTopicInternalStats> future = new CompletableFuture<PersistentTopicInternalStats>();
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass5(this, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass5 : InvocationCallback<PersistentTopicInternalStats>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<PersistentTopicInternalStats> future;

			public InvocationCallbackAnonymousInnerClass5(TopicsImpl outerInstance, CompletableFuture<PersistentTopicInternalStats> future)
			{
				this.outerInstance = outerInstance;
				this.future = future;
			}


			public override void completed(PersistentTopicInternalStats response)
			{
				future.complete(response);
			}

			public override void failed(Exception throwable)
			{
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public com.google.gson.JsonObject getInternalInfo(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual JsonObject getInternalInfo(string topic)
		{
			try
			{
				return getInternalInfoAsync(topic).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<JsonObject> getInternalInfoAsync(string topic)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "internal-info");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<com.google.gson.JsonObject> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<JsonObject> future = new CompletableFuture<JsonObject>();
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass6(this, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass6 : InvocationCallback<string>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<JsonObject> future;

			public InvocationCallbackAnonymousInnerClass6(TopicsImpl outerInstance, CompletableFuture<JsonObject> future)
			{
				this.outerInstance = outerInstance;
				this.future = future;
			}

			public override void completed(string response)
			{
				JsonObject json = (new Gson()).fromJson(response, typeof(JsonObject));
				future.complete(json);
			}

			public override void failed(Exception throwable)
			{
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.PartitionedTopicStats getPartitionedStats(String topic, boolean perPartition) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual PartitionedTopicStats getPartitionedStats(string topic, bool perPartition)
		{
			try
			{
				return getPartitionedStatsAsync(topic, perPartition).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<PartitionedTopicStats> getPartitionedStatsAsync(string topic, bool perPartition)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "partitioned-stats");
			path = path.queryParam("perPartition", perPartition);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.policies.data.PartitionedTopicStats> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<PartitionedTopicStats> future = new CompletableFuture<PartitionedTopicStats>();
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass7(this, perPartition, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass7 : InvocationCallback<PartitionedTopicStats>
		{
			private readonly TopicsImpl outerInstance;

			private bool perPartition;
			private CompletableFuture<PartitionedTopicStats> future;

			public InvocationCallbackAnonymousInnerClass7(TopicsImpl outerInstance, bool perPartition, CompletableFuture<PartitionedTopicStats> future)
			{
				this.outerInstance = outerInstance;
				this.perPartition = perPartition;
				this.future = future;
			}


			public override void completed(PartitionedTopicStats response)
			{
				if (!perPartition)
				{
					response.partitions.clear();
				}
				future.complete(response);
			}

			public override void failed(Exception throwable)
			{
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.PartitionedTopicInternalStats getPartitionedInternalStats(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual PartitionedTopicInternalStats getPartitionedInternalStats(string topic)
		{
			try
			{
				return getPartitionedInternalStatsAsync(topic).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<PartitionedTopicInternalStats> getPartitionedInternalStatsAsync(string topic)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "partitioned-internalStats");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.policies.data.PartitionedTopicInternalStats> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<PartitionedTopicInternalStats> future = new CompletableFuture<PartitionedTopicInternalStats>();
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass8(this, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass8 : InvocationCallback<PartitionedTopicInternalStats>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<PartitionedTopicInternalStats> future;

			public InvocationCallbackAnonymousInnerClass8(TopicsImpl outerInstance, CompletableFuture<PartitionedTopicInternalStats> future)
			{
				this.outerInstance = outerInstance;
				this.future = future;
			}


			public override void completed(PartitionedTopicInternalStats response)
			{
				future.complete(response);
			}

			public override void failed(Exception throwable)
			{
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteSubscription(String topic, String subName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteSubscription(string topic, string subName)
		{
			try
			{
				deleteSubscriptionAsync(topic, subName).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<Void> deleteSubscriptionAsync(string topic, string subName)
		{
			TopicName tn = validateTopic(topic);
			string encodedSubName = Codec.encode(subName);
			WebTarget path = topicPath(tn, "subscription", encodedSubName);
			return asyncDeleteRequest(path);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void skipAllMessages(String topic, String subName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void skipAllMessages(string topic, string subName)
		{
			try
			{
				skipAllMessagesAsync(topic, subName).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<Void> skipAllMessagesAsync(string topic, string subName)
		{
			TopicName tn = validateTopic(topic);
			string encodedSubName = Codec.encode(subName);
			WebTarget path = topicPath(tn, "subscription", encodedSubName, "skip_all");
			return asyncPostRequest(path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void skipMessages(String topic, String subName, long numMessages) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void skipMessages(string topic, string subName, long numMessages)
		{
			try
			{
				skipMessagesAsync(topic, subName, numMessages).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<Void> skipMessagesAsync(string topic, string subName, long numMessages)
		{
			TopicName tn = validateTopic(topic);
			string encodedSubName = Codec.encode(subName);
			WebTarget path = topicPath(tn, "subscription", encodedSubName, "skip", numMessages.ToString());
			return asyncPostRequest(path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void expireMessages(String topic, String subName, long expireTimeInSeconds) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void expireMessages(string topic, string subName, long expireTimeInSeconds)
		{
			try
			{
				expireMessagesAsync(topic, subName, expireTimeInSeconds).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<Void> expireMessagesAsync(string topic, string subName, long expireTimeInSeconds)
		{
			TopicName tn = validateTopic(topic);
			string encodedSubName = Codec.encode(subName);
			WebTarget path = topicPath(tn, "subscription", encodedSubName, "expireMessages", expireTimeInSeconds.ToString());
			return asyncPostRequest(path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void expireMessagesForAllSubscriptions(String topic, long expireTimeInSeconds) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void expireMessagesForAllSubscriptions(string topic, long expireTimeInSeconds)
		{
			try
			{
				expireMessagesForAllSubscriptionsAsync(topic, expireTimeInSeconds).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<Void> expireMessagesForAllSubscriptionsAsync(string topic, long expireTimeInSeconds)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "all_subscription", "expireMessages", expireTimeInSeconds.ToString());
			return asyncPostRequest(path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

		private CompletableFuture<IList<Message<sbyte[]>>> peekNthMessage(string topic, string subName, int messagePosition)
		{
			TopicName tn = validateTopic(topic);
			string encodedSubName = Codec.encode(subName);
			WebTarget path = topicPath(tn, "subscription", encodedSubName, "position", messagePosition.ToString());
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<java.util.List<org.apache.pulsar.client.api.Message<byte[]>>> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<IList<Message<sbyte[]>>> future = new CompletableFuture<IList<Message<sbyte[]>>>();
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass9(this, tn, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass9 : InvocationCallback<Response>
		{
			private readonly TopicsImpl outerInstance;

			private TopicName tn;
			private CompletableFuture<IList<Message<sbyte[]>>> future;

			public InvocationCallbackAnonymousInnerClass9(TopicsImpl outerInstance, TopicName tn, CompletableFuture<IList<Message<sbyte[]>>> future)
			{
				this.outerInstance = outerInstance;
				this.tn = tn;
				this.future = future;
			}


			public override void completed(Response response)
			{
				try
				{
					future.complete(outerInstance.getMessageFromHttpResponse(tn.ToString(), response));
				}
				catch (Exception e)
				{
					future.completeExceptionally(outerInstance.getApiException(e));
				}
			}

			public override void failed(Exception throwable)
			{
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<org.apache.pulsar.client.api.Message<byte[]>> peekMessages(String topic, String subName, int numMessages) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<Message<sbyte[]>> peekMessages(string topic, string subName, int numMessages)
		{
			try
			{
				return peekMessagesAsync(topic, subName, numMessages).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<IList<Message<sbyte[]>>> peekMessagesAsync(string topic, string subName, int numMessages)
		{
			checkArgument(numMessages > 0);
			CompletableFuture<IList<Message<sbyte[]>>> future = new CompletableFuture<IList<Message<sbyte[]>>>();
			peekMessagesAsync(topic, subName, numMessages, Lists.newArrayList(), future, 1);
			return future;
		}


		private void peekMessagesAsync(string topic, string subName, int numMessages, IList<Message<sbyte[]>> messages, CompletableFuture<IList<Message<sbyte[]>>> future, int nthMessage)
		{
			if (numMessages <= 0)
			{
				future.complete(messages);
				return;
			}

			// if peeking first message succeeds, we know that the topic and subscription exists
			peekNthMessage(topic, subName, nthMessage).handle((r, ex) =>
			{
			if (ex != null)
			{
				if (ex is NotFoundException)
				{
					log.warn("Exception '{}' occured while trying to peek Messages.", ex.Message);
					future.complete(messages);
				}
				else
				{
					future.completeExceptionally(ex);
				}
				return null;
			}
			for (int i = 0; i < Math.Min(r.size(), numMessages); i++)
			{
				messages.Add(r.get(i));
			}
			peekMessagesAsync(topic, subName, numMessages - r.size(), messages, future, nthMessage + 1);
			return null;
			});
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createSubscription(String topic, String subscriptionName, org.apache.pulsar.client.api.MessageId messageId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createSubscription(string topic, string subscriptionName, MessageId messageId)
		{
			try
			{
				TopicName tn = validateTopic(topic);
				string encodedSubName = Codec.encode(subscriptionName);
				WebTarget path = topicPath(tn, "subscription", encodedSubName);
				request(path).put(Entity.entity(messageId, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		public virtual CompletableFuture<Void> createSubscriptionAsync(string topic, string subscriptionName, MessageId messageId)
		{
			TopicName tn = validateTopic(topic);
			string encodedSubName = Codec.encode(subscriptionName);
			WebTarget path = topicPath(tn, "subscription", encodedSubName);
			return asyncPutRequest(path, Entity.entity(messageId, MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void resetCursor(String topic, String subName, long timestamp) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void resetCursor(string topic, string subName, long timestamp)
		{
			try
			{
				TopicName tn = validateTopic(topic);
				string encodedSubName = Codec.encode(subName);
				WebTarget path = topicPath(tn, "subscription", encodedSubName, "resetcursor", timestamp.ToString());
				request(path).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		public virtual CompletableFuture<Void> resetCursorAsync(string topic, string subName, long timestamp)
		{
			TopicName tn = validateTopic(topic);
			string encodedSubName = Codec.encode(subName);
			WebTarget path = topicPath(tn, "subscription", encodedSubName, "resetcursor", timestamp.ToString());
			return asyncPostRequest(path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void resetCursor(String topic, String subName, org.apache.pulsar.client.api.MessageId messageId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void resetCursor(string topic, string subName, MessageId messageId)
		{
			try
			{
				TopicName tn = validateTopic(topic);
				string encodedSubName = Codec.encode(subName);
				WebTarget path = topicPath(tn, "subscription", encodedSubName, "resetcursor");
				request(path).post(Entity.entity(messageId, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		public virtual CompletableFuture<Void> resetCursorAsync(string topic, string subName, MessageId messageId)
		{
			TopicName tn = validateTopic(topic);
			string encodedSubName = Codec.encode(subName);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget path = topicPath(tn, "subscription", encodedSubName, "resetcursor");
			WebTarget path = topicPath(tn, "subscription", encodedSubName, "resetcursor");
			return asyncPostRequest(path, Entity.entity(messageId, MediaType.APPLICATION_JSON));
		}

		public virtual CompletableFuture<MessageId> terminateTopicAsync(string topic)
		{
			TopicName tn = validateTopic(topic);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.client.api.MessageId> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<MessageId> future = new CompletableFuture<MessageId>();
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget path = topicPath(tn, "terminate");
				WebTarget path = topicPath(tn, "terminate");

				request(path).async().post(Entity.entity("", MediaType.APPLICATION_JSON), new InvocationCallbackAnonymousInnerClass10(this, future, path));
			}
			catch (PulsarAdminException cae)
			{
				future.completeExceptionally(cae);
			}

			return future;
		}

		private class InvocationCallbackAnonymousInnerClass10 : InvocationCallback<MessageIdImpl>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<MessageId> future;
			private WebTarget path;

			public InvocationCallbackAnonymousInnerClass10(TopicsImpl outerInstance, CompletableFuture<MessageId> future, WebTarget path)
			{
				this.outerInstance = outerInstance;
				this.future = future;
				this.path = path;
			}


			public override void completed(MessageIdImpl messageId)
			{
				future.complete(messageId);
			}

			public override void failed(Exception throwable)
			{
				log.warn("[{}] Failed to perform http post request: {}", path.Uri, throwable.Message);
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void triggerCompaction(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void triggerCompaction(string topic)
		{
			try
			{
				TopicName tn = validateTopic(topic);
				request(topicPath(tn, "compaction")).put(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.admin.LongRunningProcessStatus compactionStatus(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual LongRunningProcessStatus compactionStatus(string topic)
		{
			try
			{
				TopicName tn = validateTopic(topic);
				return request(topicPath(tn, "compaction")).get(typeof(LongRunningProcessStatus));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void triggerOffload(String topic, org.apache.pulsar.client.api.MessageId messageId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void triggerOffload(string topic, MessageId messageId)
		{
			try
			{
				TopicName tn = validateTopic(topic);
				WebTarget path = topicPath(tn, "offload");
				request(path).put(Entity.entity(messageId, MediaType.APPLICATION_JSON), typeof(MessageIdImpl));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.admin.OffloadProcessStatus offloadStatus(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual OffloadProcessStatus offloadStatus(string topic)
		{
			try
			{
				TopicName tn = validateTopic(topic);
				return request(topicPath(tn, "offload")).get(typeof(OffloadProcessStatus));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private WebTarget namespacePath(string domain, NamespaceName @namespace, params string[] parts)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget super = namespace.isV2() ? adminV2Topics : adminTopics;
			WebTarget @base = @namespace.V2 ? adminV2Topics : adminTopics;
			WebTarget namespacePath = @base.path(domain).path(@namespace.ToString());
			namespacePath = WebTargets.addParts(namespacePath, parts);
			return namespacePath;
		}

		private WebTarget topicPath(TopicName topic, params string[] parts)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget super = topic.isV2() ? adminV2Topics : adminTopics;
			WebTarget @base = topic.V2 ? adminV2Topics : adminTopics;
			WebTarget topicPath = @base.path(topic.RestPath);
			topicPath = WebTargets.addParts(topicPath, parts);
			return topicPath;
		}

		/*
		 * returns topic name with encoded Local Name
		 */
		private TopicName validateTopic(string topic)
		{
			// Parsing will throw exception if name is not valid
			return TopicName.get(topic);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<org.apache.pulsar.client.api.Message<byte[]>> getMessageFromHttpResponse(String topic, javax.ws.rs.core.Response response) throws Exception
		private IList<Message<sbyte[]>> getMessageFromHttpResponse(string topic, Response response)
		{

			if (response.Status != Response.Status.OK.StatusCode)
			{
				throw getApiException(response);
			}

			string msgId = response.getHeaderString("X-Pulsar-Message-ID");
			using (Stream stream = (Stream) response.Entity)
			{
				sbyte[] data = new sbyte[stream.available()];
				stream.Read(data, 0, data.Length);

				IDictionary<string, string> properties = Maps.newTreeMap();
				MultivaluedMap<string, object> headers = response.Headers;
				object tmp = headers.getFirst("X-Pulsar-publish-time");
				if (tmp != null)
				{
					properties["publish-time"] = (string) tmp;
				}
				tmp = headers.getFirst(BATCH_HEADER);
				if (response.getHeaderString(BATCH_HEADER) != null)
				{
					properties[BATCH_HEADER] = (string) tmp;
					return getIndividualMsgsFromBatch(topic, msgId, data, properties);
				}
				foreach (KeyValuePair<string, IList<object>> entry in headers.entrySet())
				{
					string header = entry.Key;
					if (header.Contains("X-Pulsar-PROPERTY-"))
					{
						string keyName = StringHelper.SubstringSpecial(header, "X-Pulsar-PROPERTY-".Length, header.Length);
						properties[keyName] = (string) entry.Value.get(0);
					}
				}

				return Collections.singletonList(new MessageImpl<sbyte[]>(topic, msgId, properties, Unpooled.wrappedBuffer(data), Schema.BYTES));
			}
		}

		private IList<Message<sbyte[]>> getIndividualMsgsFromBatch(string topic, string msgId, sbyte[] data, IDictionary<string, string> properties)
		{
			IList<Message<sbyte[]>> ret = new List<Message<sbyte[]>>();
			int batchSize = int.Parse(properties[BATCH_HEADER]);
			for (int i = 0; i < batchSize; i++)
			{
				string batchMsgId = msgId + ":" + i;
				PulsarApi.SingleMessageMetadata.Builder singleMessageMetadataBuilder = PulsarApi.SingleMessageMetadata.newBuilder();
				ByteBuf buf = Unpooled.wrappedBuffer(data);
				try
				{
					ByteBuf singleMessagePayload = Commands.deSerializeSingleMessageInBatch(buf, singleMessageMetadataBuilder, i, batchSize);
					PulsarApi.SingleMessageMetadata singleMessageMetadata = singleMessageMetadataBuilder.build();
					if (singleMessageMetadata.PropertiesCount > 0)
					{
						foreach (PulsarApi.KeyValue entry in singleMessageMetadata.PropertiesList)
						{
							properties[entry.Key] = entry.Value;
						}
					}
					ret.Add(new MessageImpl<>(topic, batchMsgId, properties, singleMessagePayload, Schema.BYTES));
				}
				catch (Exception ex)
				{
					log.error("Exception occured while trying to get BatchMsgId: {}", batchMsgId, ex);
				}
				buf.release();
				singleMessageMetadataBuilder.recycle();
			}
			return ret;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.MessageId getLastMessageId(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual MessageId getLastMessageId(string topic)
		{
			try
			{
				return getLastMessageIdAsync(topic).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
			}
			catch (ExecutionException e)
			{
				throw (PulsarAdminException) e.InnerException;
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				throw new PulsarAdminException(e);
			}
			catch (TimeoutException e)
			{
				throw new PulsarAdminException.TimeoutException(e);
			}
		}

		public virtual CompletableFuture<MessageId> getLastMessageIdAsync(string topic)
		{
			TopicName tn = validateTopic(topic);
			WebTarget path = topicPath(tn, "lastMessageId");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.client.api.MessageId> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<MessageId> future = new CompletableFuture<MessageId>();
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass11(this, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass11 : InvocationCallback<MessageIdImpl>
		{
			private readonly TopicsImpl outerInstance;

			private CompletableFuture<MessageId> future;

			public InvocationCallbackAnonymousInnerClass11(TopicsImpl outerInstance, CompletableFuture<MessageId> future)
			{
				this.outerInstance = outerInstance;
				this.future = future;
			}


			public override void completed(MessageIdImpl response)
			{
				future.complete(response);
			}

			public override void failed(Exception throwable)
			{
				future.completeExceptionally(outerInstance.getApiException(throwable.InnerException));
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(TopicsImpl));
	}

}