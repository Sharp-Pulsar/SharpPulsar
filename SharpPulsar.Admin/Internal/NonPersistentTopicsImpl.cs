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
namespace Org.Apache.Pulsar.Client.Admin.@internal
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;



	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using TopicName = Org.Apache.Pulsar.Common.Naming.TopicName;
	using NamespaceName = Org.Apache.Pulsar.Common.Naming.NamespaceName;
	using PartitionedTopicMetadata = Org.Apache.Pulsar.Common.Partition.PartitionedTopicMetadata;
	using NonPersistentTopicStats = Org.Apache.Pulsar.Common.Policies.Data.NonPersistentTopicStats;
	using PersistentTopicInternalStats = Org.Apache.Pulsar.Common.Policies.Data.PersistentTopicInternalStats;

	public class NonPersistentTopicsImpl : BaseResource, NonPersistentTopics
	{

		private readonly WebTarget adminNonPersistentTopics;
		private readonly WebTarget adminV2NonPersistentTopics;

		public NonPersistentTopicsImpl(WebTarget Web, Authentication Auth, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
			adminNonPersistentTopics = Web.path("/admin");
			adminV2NonPersistentTopics = Web.path("/admin/v2");
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

		public override CompletableFuture<Void> CreatePartitionedTopicAsync(string Topic, int NumPartitions)
		{
			checkArgument(NumPartitions > 0, "Number of partitions should be more than 0");
			TopicName TopicName = ValidateTopic(Topic);
			WebTarget Path = TopicPath(TopicName, "partitions");
			return AsyncPutRequest(Path, Entity.entity(NumPartitions, MediaType.APPLICATION_JSON));
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
			TopicName TopicName = ValidateTopic(Topic);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.partition.PartitionedTopicMetadata> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<PartitionedTopicMetadata> Future = new CompletableFuture<PartitionedTopicMetadata>();
			WebTarget Path = TopicPath(TopicName, "partitions");
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass(this, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass : InvocationCallback<PartitionedTopicMetadata>
		{
			private readonly NonPersistentTopicsImpl outerInstance;

			private CompletableFuture<PartitionedTopicMetadata> future;

			public InvocationCallbackAnonymousInnerClass(NonPersistentTopicsImpl OuterInstance, CompletableFuture<PartitionedTopicMetadata> Future)
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
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.NonPersistentTopicStats getStats(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override NonPersistentTopicStats GetStats(string Topic)
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

		public override CompletableFuture<NonPersistentTopicStats> GetStatsAsync(string Topic)
		{
			TopicName TopicName = ValidateTopic(Topic);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.policies.data.NonPersistentTopicStats> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<NonPersistentTopicStats> Future = new CompletableFuture<NonPersistentTopicStats>();
			WebTarget Path = TopicPath(TopicName, "stats");
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass2(this, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass2 : InvocationCallback<NonPersistentTopicStats>
		{
			private readonly NonPersistentTopicsImpl outerInstance;

			private CompletableFuture<NonPersistentTopicStats> future;

			public InvocationCallbackAnonymousInnerClass2(NonPersistentTopicsImpl OuterInstance, CompletableFuture<NonPersistentTopicStats> Future)
			{
				this.outerInstance = OuterInstance;
				this.future = Future;
			}


			public override void completed(NonPersistentTopicStats Response)
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
			TopicName TopicName = ValidateTopic(Topic);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<org.apache.pulsar.common.policies.data.PersistentTopicInternalStats> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<PersistentTopicInternalStats> Future = new CompletableFuture<PersistentTopicInternalStats>();
			WebTarget Path = TopicPath(TopicName, "internalStats");
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass3(this, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass3 : InvocationCallback<PersistentTopicInternalStats>
		{
			private readonly NonPersistentTopicsImpl outerInstance;

			private CompletableFuture<PersistentTopicInternalStats> future;

			public InvocationCallbackAnonymousInnerClass3(NonPersistentTopicsImpl OuterInstance, CompletableFuture<PersistentTopicInternalStats> Future)
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
			TopicName TopicName = ValidateTopic(Topic);
			WebTarget Path = TopicPath(TopicName, "unload");
			return AsyncPutRequest(Path, Entity.entity("", MediaType.APPLICATION_JSON));
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
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass4(this, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass4 : InvocationCallback<IList<string>>
		{
			private readonly NonPersistentTopicsImpl outerInstance;

			private CompletableFuture<IList<string>> future;

			public InvocationCallbackAnonymousInnerClass4(NonPersistentTopicsImpl OuterInstance, CompletableFuture<IList<string>> Future)
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
//ORIGINAL LINE: @Override public java.util.List<String> getList(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> GetList(string Namespace)
		{
			try
			{
				return GetListAsync(Namespace).get(this.ReadTimeoutMs, TimeUnit.MILLISECONDS);
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

		public override CompletableFuture<IList<string>> GetListAsync(string Namespace)
		{
			NamespaceName Ns = NamespaceName.get(Namespace);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CompletableFuture<java.util.List<String>> future = new java.util.concurrent.CompletableFuture<>();
			CompletableFuture<IList<string>> Future = new CompletableFuture<IList<string>>();
			WebTarget Path = NamespacePath("non-persistent", Ns);
			AsyncGetRequest(Path, new InvocationCallbackAnonymousInnerClass5(this, Future));
			return Future;
		}

		public class InvocationCallbackAnonymousInnerClass5 : InvocationCallback<IList<string>>
		{
			private readonly NonPersistentTopicsImpl outerInstance;

			private CompletableFuture<IList<string>> future;

			public InvocationCallbackAnonymousInnerClass5(NonPersistentTopicsImpl OuterInstance, CompletableFuture<IList<string>> Future)
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

		/*
		 * returns topic name with encoded Local Name
		 */
		private TopicName ValidateTopic(string Topic)
		{
			// Parsing will throw exception if name is not valid
			return TopicName.get(Topic);
		}

		private WebTarget NamespacePath(string Domain, NamespaceName Namespace, params string[] Parts)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget super = namespace.isV2() ? adminV2NonPersistentTopics : adminNonPersistentTopics;
			WebTarget Base = Namespace.V2 ? adminV2NonPersistentTopics : adminNonPersistentTopics;
			WebTarget NamespacePath = Base.path(Domain).path(Namespace.ToString());
			NamespacePath = WebTargets.AddParts(NamespacePath, Parts);
			return NamespacePath;
		}

		private WebTarget TopicPath(TopicName Topic, params string[] Parts)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget super = topic.isV2() ? adminV2NonPersistentTopics : adminNonPersistentTopics;
			WebTarget Base = Topic.V2 ? adminV2NonPersistentTopics : adminNonPersistentTopics;
			WebTarget TopicPath = Base.path(Topic.RestPath);
			TopicPath = WebTargets.AddParts(TopicPath, Parts);
			return TopicPath;
		}
	}

}