using System;
using System.Collections.Generic;
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



	using Authentication = org.apache.pulsar.client.api.Authentication;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using NamespaceName = org.apache.pulsar.common.naming.NamespaceName;
	using PartitionedTopicMetadata = org.apache.pulsar.common.partition.PartitionedTopicMetadata;
	using NonPersistentTopicStats = org.apache.pulsar.common.policies.data.NonPersistentTopicStats;
	using PersistentTopicInternalStats = org.apache.pulsar.common.policies.data.PersistentTopicInternalStats;

	public class NonPersistentTopicsImpl : BaseResource, NonPersistentTopics
	{

		private readonly WebTarget adminNonPersistentTopics;
		private readonly WebTarget adminV2NonPersistentTopics;

		public NonPersistentTopicsImpl(WebTarget web, Authentication auth, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			adminNonPersistentTopics = web.path("/admin");
			adminV2NonPersistentTopics = web.path("/admin/v2");
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

		public virtual CompletableFuture<Void> createPartitionedTopicAsync(string topic, int numPartitions)
		{
			checkArgument(numPartitions > 0, "Number of partitions should be more than 0");
			TopicName topicName = validateTopic(topic);
			WebTarget path = topicPath(topicName, "partitions");
			return asyncPutRequest(path, Entity.entity(numPartitions, MediaType.APPLICATION_JSON));
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
			TopicName topicName = validateTopic(topic);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.partition.PartitionedTopicMetadata> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<PartitionedTopicMetadata> future = new CompletableFuture<PartitionedTopicMetadata>();
			WebTarget path = topicPath(topicName, "partitions");
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass(this, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass : InvocationCallback<PartitionedTopicMetadata>
		{
			private readonly NonPersistentTopicsImpl outerInstance;

			private CompletableFuture<PartitionedTopicMetadata> future;

			public InvocationCallbackAnonymousInnerClass(NonPersistentTopicsImpl outerInstance, CompletableFuture<PartitionedTopicMetadata> future)
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
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.NonPersistentTopicStats getStats(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual NonPersistentTopicStats getStats(string topic)
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

		public virtual CompletableFuture<NonPersistentTopicStats> getStatsAsync(string topic)
		{
			TopicName topicName = validateTopic(topic);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.policies.data.NonPersistentTopicStats> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<NonPersistentTopicStats> future = new CompletableFuture<NonPersistentTopicStats>();
			WebTarget path = topicPath(topicName, "stats");
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass2(this, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass2 : InvocationCallback<NonPersistentTopicStats>
		{
			private readonly NonPersistentTopicsImpl outerInstance;

			private CompletableFuture<NonPersistentTopicStats> future;

			public InvocationCallbackAnonymousInnerClass2(NonPersistentTopicsImpl outerInstance, CompletableFuture<NonPersistentTopicStats> future)
			{
				this.outerInstance = outerInstance;
				this.future = future;
			}


			public override void completed(NonPersistentTopicStats response)
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
			TopicName topicName = validateTopic(topic);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.policies.data.PersistentTopicInternalStats> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<PersistentTopicInternalStats> future = new CompletableFuture<PersistentTopicInternalStats>();
			WebTarget path = topicPath(topicName, "internalStats");
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass3(this, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass3 : InvocationCallback<PersistentTopicInternalStats>
		{
			private readonly NonPersistentTopicsImpl outerInstance;

			private CompletableFuture<PersistentTopicInternalStats> future;

			public InvocationCallbackAnonymousInnerClass3(NonPersistentTopicsImpl outerInstance, CompletableFuture<PersistentTopicInternalStats> future)
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
			TopicName topicName = validateTopic(topic);
			WebTarget path = topicPath(topicName, "unload");
			return asyncPutRequest(path, Entity.entity("", MediaType.APPLICATION_JSON));
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
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass4(this, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass4 : InvocationCallback<IList<string>>
		{
			private readonly NonPersistentTopicsImpl outerInstance;

			private CompletableFuture<IList<string>> future;

			public InvocationCallbackAnonymousInnerClass4(NonPersistentTopicsImpl outerInstance, CompletableFuture<IList<string>> future)
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
//ORIGINAL LINE: @Override public java.util.List<String> getList(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> getList(string @namespace)
		{
			try
			{
				return getListAsync(@namespace).get(this.readTimeoutMs, TimeUnit.MILLISECONDS);
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

		public virtual CompletableFuture<IList<string>> getListAsync(string @namespace)
		{
			NamespaceName ns = NamespaceName.get(@namespace);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<java.util.List<String>> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<IList<string>> future = new CompletableFuture<IList<string>>();
			WebTarget path = namespacePath("non-persistent", ns);
			asyncGetRequest(path, new InvocationCallbackAnonymousInnerClass5(this, future));
			return future;
		}

		private class InvocationCallbackAnonymousInnerClass5 : InvocationCallback<IList<string>>
		{
			private readonly NonPersistentTopicsImpl outerInstance;

			private CompletableFuture<IList<string>> future;

			public InvocationCallbackAnonymousInnerClass5(NonPersistentTopicsImpl outerInstance, CompletableFuture<IList<string>> future)
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

		/*
		 * returns topic name with encoded Local Name
		 */
		private TopicName validateTopic(string topic)
		{
			// Parsing will throw exception if name is not valid
			return TopicName.get(topic);
		}

		private WebTarget namespacePath(string domain, NamespaceName @namespace, params string[] parts)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget super = namespace.isV2() ? adminV2NonPersistentTopics : adminNonPersistentTopics;
			WebTarget @base = @namespace.V2 ? adminV2NonPersistentTopics : adminNonPersistentTopics;
			WebTarget namespacePath = @base.path(domain).path(@namespace.ToString());
			namespacePath = WebTargets.addParts(namespacePath, parts);
			return namespacePath;
		}

		private WebTarget topicPath(TopicName topic, params string[] parts)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget super = topic.isV2() ? adminV2NonPersistentTopics : adminNonPersistentTopics;
			WebTarget @base = topic.V2 ? adminV2NonPersistentTopics : adminNonPersistentTopics;
			WebTarget topicPath = @base.path(topic.RestPath);
			topicPath = WebTargets.addParts(topicPath, parts);
			return topicPath;
		}
	}

}