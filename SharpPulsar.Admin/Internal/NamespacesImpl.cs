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
	using NamespaceName = Org.Apache.Pulsar.Common.Naming.NamespaceName;
	using AuthAction = Org.Apache.Pulsar.Common.Policies.Data.AuthAction;
	using BacklogQuota = Org.Apache.Pulsar.Common.Policies.Data.BacklogQuota;
	using BookieAffinityGroupData = Org.Apache.Pulsar.Common.Policies.Data.BookieAffinityGroupData;
	using BacklogQuotaType = Org.Apache.Pulsar.Common.Policies.Data.BacklogQuota.BacklogQuotaType;
	using BundlesData = Org.Apache.Pulsar.Common.Policies.Data.BundlesData;
	using DispatchRate = Org.Apache.Pulsar.Common.Policies.Data.DispatchRate;
	using ErrorData = Org.Apache.Pulsar.Common.Policies.Data.ErrorData;
	using PersistencePolicies = Org.Apache.Pulsar.Common.Policies.Data.PersistencePolicies;
	using Policies = Org.Apache.Pulsar.Common.Policies.Data.Policies;
	using PublishRate = Org.Apache.Pulsar.Common.Policies.Data.PublishRate;
	using RetentionPolicies = Org.Apache.Pulsar.Common.Policies.Data.RetentionPolicies;
	using SchemaAutoUpdateCompatibilityStrategy = Org.Apache.Pulsar.Common.Policies.Data.SchemaAutoUpdateCompatibilityStrategy;
	using SchemaCompatibilityStrategy = Org.Apache.Pulsar.Common.Policies.Data.SchemaCompatibilityStrategy;
	using SubscribeRate = Org.Apache.Pulsar.Common.Policies.Data.SubscribeRate;
	using SubscriptionAuthMode = Org.Apache.Pulsar.Common.Policies.Data.SubscriptionAuthMode;

	public class NamespacesImpl : BaseResource, Namespaces
	{

		private readonly WebTarget adminNamespaces;
		private readonly WebTarget adminV2Namespaces;

		public NamespacesImpl(WebTarget Web, Authentication Auth, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
			adminNamespaces = Web.path("/admin/namespaces");
			adminV2Namespaces = Web.path("/admin/v2/namespaces");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getNamespaces(String tenant) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> GetNamespaces(string Tenant)
		{
			try
			{
				WebTarget Path = adminV2Namespaces.path(Tenant);
				return Request(Path).get(new GenericTypeAnonymousInnerClass(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass : GenericType<IList<string>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass(NamespacesImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getNamespaces(String tenant, String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> GetNamespaces(string Tenant, string Cluster)
		{
			try
			{
				WebTarget Path = adminNamespaces.path(Tenant).path(Cluster);
				return Request(Path).get(new GenericTypeAnonymousInnerClass2(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass2 : GenericType<IList<string>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(NamespacesImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getTopics(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> GetTopics(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				string Action = Ns.V2 ? "topics" : "destinations";
				WebTarget Path = NamespacePath(Ns, Action);
				return Request(Path).get(new GenericTypeAnonymousInnerClass3(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass3 : GenericType<IList<string>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass3(NamespacesImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.Policies getPolicies(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override Policies GetPolicies(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns);
				return Request(Path).get(typeof(Policies));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNamespace(String namespace, java.util.Set<String> clusters) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateNamespace(string Namespace, ISet<string> Clusters)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns);

				if (Ns.V2)
				{
					// For V2 API we pass full Policy class instance
					Policies Policies = new Policies();
					Policies.ReplicationClusters = Clusters;
					Request(Path).put(Entity.entity(Policies, MediaType.APPLICATION_JSON), typeof(ErrorData));
				}
				else
				{
					// For V1 API, we pass the BundlesData on creation
					Request(Path).put(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
					// For V1, we need to do it in 2 steps
					SetNamespaceReplicationClusters(Namespace, Clusters);
				}
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNamespace(String namespace, int numBundles) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateNamespace(string Namespace, int NumBundles)
		{
			CreateNamespace(Namespace, new BundlesData(NumBundles));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNamespace(String namespace, org.apache.pulsar.common.policies.data.Policies policies) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateNamespace(string Namespace, Policies Policies)
		{
			NamespaceName Ns = NamespaceName.get(Namespace);
			checkArgument(Ns.V2, "Create namespace with policies is only supported on newer namespaces");

			try
			{
				WebTarget Path = NamespacePath(Ns);

				// For V2 API we pass full Policy class instance
				Request(Path).put(Entity.entity(Policies, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNamespace(String namespace, org.apache.pulsar.common.policies.data.BundlesData bundlesData) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateNamespace(string Namespace, BundlesData BundlesData)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns);

				if (Ns.V2)
				{
					// For V2 API we pass full Policy class instance
					Policies Policies = new Policies();
					Policies.Bundles = BundlesData;
					Request(Path).put(Entity.entity(Policies, MediaType.APPLICATION_JSON), typeof(ErrorData));
				}
				else
				{
					// For V1 API, we pass the BundlesData on creation
					Request(Path).put(Entity.entity(BundlesData, MediaType.APPLICATION_JSON), typeof(ErrorData));
				}
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNamespace(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateNamespace(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns);
				Request(Path).put(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteNamespace(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteNamespace(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns);
				Request(Path).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteNamespaceBundle(String namespace, String bundleRange) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteNamespaceBundle(string Namespace, string BundleRange)
		{
			try
			{
				DeleteNamespaceBundleAsync(Namespace, BundleRange).get();
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
		}

		public override CompletableFuture<Void> DeleteNamespaceBundleAsync(string Namespace, string BundleRange)
		{
			NamespaceName Ns = NamespaceName.get(Namespace);
			WebTarget Path = NamespacePath(Ns, BundleRange);
			return AsyncDeleteRequest(Path);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction>> getPermissions(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IDictionary<string, ISet<AuthAction>> GetPermissions(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "permissions");
				return Request(Path).get(new GenericTypeAnonymousInnerClass4(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass4 : GenericType<IDictionary<string, ISet<AuthAction>>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass4(NamespacesImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void grantPermissionOnNamespace(String namespace, String role, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction> actions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void GrantPermissionOnNamespace(string Namespace, string Role, ISet<AuthAction> Actions)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "permissions", Role);
				Request(Path).post(Entity.entity(Actions, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void revokePermissionsOnNamespace(String namespace, String role) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void RevokePermissionsOnNamespace(string Namespace, string Role)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "permissions", Role);
				Request(Path).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void grantPermissionOnSubscription(String namespace, String subscription, java.util.Set<String> roles) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void GrantPermissionOnSubscription(string Namespace, string Subscription, ISet<string> Roles)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "permissions", "subscription", Subscription);
				Request(Path).post(Entity.entity(Roles, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void revokePermissionOnSubscription(String namespace, String subscription, String role) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void RevokePermissionOnSubscription(string Namespace, string Subscription, string Role)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "permissions", Subscription, Role);
				Request(Path).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getNamespaceReplicationClusters(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> GetNamespaceReplicationClusters(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "replication");
				return Request(Path).get(new GenericTypeAnonymousInnerClass5(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass5 : GenericType<IList<string>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass5(NamespacesImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setNamespaceReplicationClusters(String namespace, java.util.Set<String> clusterIds) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetNamespaceReplicationClusters(string Namespace, ISet<string> ClusterIds)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "replication");
				Request(Path).post(Entity.entity(ClusterIds, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public int getNamespaceMessageTTL(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override int GetNamespaceMessageTTL(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "messageTTL");
				return Request(Path).get(new GenericTypeAnonymousInnerClass6(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass6 : GenericType<int>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass6(NamespacesImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setNamespaceMessageTTL(String namespace, int ttlInSeconds) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetNamespaceMessageTTL(string Namespace, int TtlInSeconds)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "messageTTL");
				Request(Path).post(Entity.entity(TtlInSeconds, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setNamespaceAntiAffinityGroup(String namespace, String namespaceAntiAffinityGroup) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetNamespaceAntiAffinityGroup(string Namespace, string NamespaceAntiAffinityGroup)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "antiAffinity");
				Request(Path).post(Entity.entity(NamespaceAntiAffinityGroup, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public String getNamespaceAntiAffinityGroup(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override string GetNamespaceAntiAffinityGroup(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "antiAffinity");
				return Request(Path).get(new GenericTypeAnonymousInnerClass7(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass7 : GenericType<string>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass7(NamespacesImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getAntiAffinityNamespaces(String tenant, String cluster, String namespaceAntiAffinityGroup) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> GetAntiAffinityNamespaces(string Tenant, string Cluster, string NamespaceAntiAffinityGroup)
		{
			try
			{
				WebTarget Path = adminNamespaces.path(Cluster).path("antiAffinity").path(NamespaceAntiAffinityGroup);
				return Request(Path.queryParam("property", Tenant)).get(new GenericTypeAnonymousInnerClass8(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass8 : GenericType<IList<string>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass8(NamespacesImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteNamespaceAntiAffinityGroup(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteNamespaceAntiAffinityGroup(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "antiAffinity");
				Request(Path).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setDeduplicationStatus(String namespace, boolean enableDeduplication) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetDeduplicationStatus(string Namespace, bool EnableDeduplication)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "deduplication");
				Request(Path).post(Entity.entity(EnableDeduplication, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<org.apache.pulsar.common.policies.data.BacklogQuota.BacklogQuotaType, org.apache.pulsar.common.policies.data.BacklogQuota> getBacklogQuotaMap(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IDictionary<BacklogQuota.BacklogQuotaType, BacklogQuota> GetBacklogQuotaMap(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "backlogQuotaMap");
				return Request(Path).get(new GenericTypeAnonymousInnerClass9(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass9 : GenericType<IDictionary<BacklogQuota.BacklogQuotaType, BacklogQuota>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass9(NamespacesImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setBacklogQuota(String namespace, org.apache.pulsar.common.policies.data.BacklogQuota backlogQuota) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetBacklogQuota(string Namespace, BacklogQuota BacklogQuota)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "backlogQuota");
				Request(Path).post(Entity.entity(BacklogQuota, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void removeBacklogQuota(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void RemoveBacklogQuota(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "backlogQuota");
				Request(Path.queryParam("backlogQuotaType", BacklogQuota.BacklogQuotaType.destination_storage.ToString())).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setPersistence(String namespace, org.apache.pulsar.common.policies.data.PersistencePolicies persistence) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetPersistence(string Namespace, PersistencePolicies Persistence)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "persistence");
				Request(Path).post(Entity.entity(Persistence, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setBookieAffinityGroup(String namespace, org.apache.pulsar.common.policies.data.BookieAffinityGroupData bookieAffinityGroup) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetBookieAffinityGroup(string Namespace, BookieAffinityGroupData BookieAffinityGroup)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "persistence", "bookieAffinity");
				Request(Path).post(Entity.entity(BookieAffinityGroup, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteBookieAffinityGroup(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteBookieAffinityGroup(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "persistence", "bookieAffinity");
				Request(Path).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.BookieAffinityGroupData getBookieAffinityGroup(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override BookieAffinityGroupData GetBookieAffinityGroup(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "persistence", "bookieAffinity");
				return Request(Path).get(typeof(BookieAffinityGroupData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.PersistencePolicies getPersistence(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override PersistencePolicies GetPersistence(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "persistence");
				return Request(Path).get(typeof(PersistencePolicies));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setRetention(String namespace, org.apache.pulsar.common.policies.data.RetentionPolicies retention) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetRetention(string Namespace, RetentionPolicies Retention)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "retention");
				Request(Path).post(Entity.entity(Retention, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.RetentionPolicies getRetention(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override RetentionPolicies GetRetention(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "retention");
				return Request(Path).get(typeof(RetentionPolicies));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void unload(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void Unload(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "unload");
				Request(Path).put(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public String getReplicationConfigVersion(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override string GetReplicationConfigVersion(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "configversion");
				return Request(Path).get(typeof(string));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void unloadNamespaceBundle(String namespace, String bundle) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UnloadNamespaceBundle(string Namespace, string Bundle)
		{
			try
			{
				UnloadNamespaceBundleAsync(Namespace, Bundle).get();
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
		}

		public override CompletableFuture<Void> UnloadNamespaceBundleAsync(string Namespace, string Bundle)
		{
			NamespaceName Ns = NamespaceName.get(Namespace);
			WebTarget Path = NamespacePath(Ns, Bundle, "unload");
			return AsyncPutRequest(Path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void splitNamespaceBundle(String namespace, String bundle, boolean unloadSplitBundles) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SplitNamespaceBundle(string Namespace, string Bundle, bool UnloadSplitBundles)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, Bundle, "split");
				Request(Path.queryParam("unload", Convert.ToString(UnloadSplitBundles))).put(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setPublishRate(String namespace, org.apache.pulsar.common.policies.data.PublishRate publishMsgRate) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetPublishRate(string Namespace, PublishRate PublishMsgRate)
		{

			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "publishRate");
				Request(Path).post(Entity.entity(PublishMsgRate, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.PublishRate getPublishRate(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override PublishRate GetPublishRate(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "publishRate");
				return Request(Path).get(typeof(PublishRate));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setDispatchRate(String namespace, org.apache.pulsar.common.policies.data.DispatchRate dispatchRate) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetDispatchRate(string Namespace, DispatchRate DispatchRate)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "dispatchRate");
				Request(Path).post(Entity.entity(DispatchRate, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.DispatchRate getDispatchRate(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override DispatchRate GetDispatchRate(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "dispatchRate");
				return Request(Path).get(typeof(DispatchRate));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setSubscribeRate(String namespace, org.apache.pulsar.common.policies.data.SubscribeRate subscribeRate) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetSubscribeRate(string Namespace, SubscribeRate SubscribeRate)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "subscribeRate");
				Request(Path).post(Entity.entity(SubscribeRate, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SubscribeRate getSubscribeRate(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override SubscribeRate GetSubscribeRate(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "subscribeRate");
				return Request(Path).get(typeof(SubscribeRate));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setSubscriptionDispatchRate(String namespace, org.apache.pulsar.common.policies.data.DispatchRate dispatchRate) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetSubscriptionDispatchRate(string Namespace, DispatchRate DispatchRate)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "subscriptionDispatchRate");
				Request(Path).post(Entity.entity(DispatchRate, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.DispatchRate getSubscriptionDispatchRate(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override DispatchRate GetSubscriptionDispatchRate(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "subscriptionDispatchRate");
				return Request(Path).get(typeof(DispatchRate));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setReplicatorDispatchRate(String namespace, org.apache.pulsar.common.policies.data.DispatchRate dispatchRate) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetReplicatorDispatchRate(string Namespace, DispatchRate DispatchRate)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "replicatorDispatchRate");
				Request(Path).post(Entity.entity(DispatchRate, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.DispatchRate getReplicatorDispatchRate(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override DispatchRate GetReplicatorDispatchRate(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "replicatorDispatchRate");
				return Request(Path).get(typeof(DispatchRate));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearNamespaceBacklog(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void ClearNamespaceBacklog(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "clearBacklog");
				Request(Path).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearNamespaceBacklogForSubscription(String namespace, String subscription) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void ClearNamespaceBacklogForSubscription(string Namespace, string Subscription)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "clearBacklog", Subscription);
				Request(Path).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearNamespaceBundleBacklog(String namespace, String bundle) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void ClearNamespaceBundleBacklog(string Namespace, string Bundle)
		{
			try
			{
				ClearNamespaceBundleBacklogAsync(Namespace, Bundle).get();
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
		}

		public override CompletableFuture<Void> ClearNamespaceBundleBacklogAsync(string Namespace, string Bundle)
		{
			NamespaceName Ns = NamespaceName.get(Namespace);
			WebTarget Path = NamespacePath(Ns, Bundle, "clearBacklog");
			return AsyncPostRequest(Path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearNamespaceBundleBacklogForSubscription(String namespace, String bundle, String subscription) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void ClearNamespaceBundleBacklogForSubscription(string Namespace, string Bundle, string Subscription)
		{
			try
			{
				ClearNamespaceBundleBacklogForSubscriptionAsync(Namespace, Bundle, Subscription).get();
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
		}

		public override CompletableFuture<Void> ClearNamespaceBundleBacklogForSubscriptionAsync(string Namespace, string Bundle, string Subscription)
		{
			NamespaceName Ns = NamespaceName.get(Namespace);
			WebTarget Path = NamespacePath(Ns, Bundle, "clearBacklog", Subscription);
			return AsyncPostRequest(Path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void unsubscribeNamespace(String namespace, String subscription) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UnsubscribeNamespace(string Namespace, string Subscription)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "unsubscribe", Subscription);
				Request(Path).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void unsubscribeNamespaceBundle(String namespace, String bundle, String subscription) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UnsubscribeNamespaceBundle(string Namespace, string Bundle, string Subscription)
		{
			try
			{
				UnsubscribeNamespaceBundleAsync(Namespace, Bundle, Subscription).get();
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
		}

		public override CompletableFuture<Void> UnsubscribeNamespaceBundleAsync(string Namespace, string Bundle, string Subscription)
		{
			NamespaceName Ns = NamespaceName.get(Namespace);
			WebTarget Path = NamespacePath(Ns, Bundle, "unsubscribe", Subscription);
			return AsyncPostRequest(Path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setSubscriptionAuthMode(String namespace, org.apache.pulsar.common.policies.data.SubscriptionAuthMode subscriptionAuthMode) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetSubscriptionAuthMode(string Namespace, SubscriptionAuthMode SubscriptionAuthMode)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "subscriptionAuthMode");
				Request(Path).post(Entity.entity(SubscriptionAuthMode, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setEncryptionRequiredStatus(String namespace, boolean encryptionRequired) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetEncryptionRequiredStatus(string Namespace, bool EncryptionRequired)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "encryptionRequired");
				Request(Path).post(Entity.entity(EncryptionRequired, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public int getMaxProducersPerTopic(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override int GetMaxProducersPerTopic(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "maxProducersPerTopic");
				return Request(Path).get(typeof(Integer));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setMaxProducersPerTopic(String namespace, int maxProducersPerTopic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetMaxProducersPerTopic(string Namespace, int MaxProducersPerTopic)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "maxProducersPerTopic");
				Request(Path).post(Entity.entity(MaxProducersPerTopic, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public int getMaxConsumersPerTopic(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override int GetMaxConsumersPerTopic(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "maxConsumersPerTopic");
				return Request(Path).get(typeof(Integer));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setMaxConsumersPerTopic(String namespace, int maxConsumersPerTopic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetMaxConsumersPerTopic(string Namespace, int MaxConsumersPerTopic)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "maxConsumersPerTopic");
				Request(Path).post(Entity.entity(MaxConsumersPerTopic, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public int getMaxConsumersPerSubscription(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override int GetMaxConsumersPerSubscription(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "maxConsumersPerSubscription");
				return Request(Path).get(typeof(Integer));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setMaxConsumersPerSubscription(String namespace, int maxConsumersPerSubscription) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetMaxConsumersPerSubscription(string Namespace, int MaxConsumersPerSubscription)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "maxConsumersPerSubscription");
				Request(Path).post(Entity.entity(MaxConsumersPerSubscription, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public long getCompactionThreshold(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override long GetCompactionThreshold(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "compactionThreshold");
				return Request(Path).get(typeof(Long));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setCompactionThreshold(String namespace, long compactionThreshold) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetCompactionThreshold(string Namespace, long CompactionThreshold)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "compactionThreshold");
				Request(Path).put(Entity.entity(CompactionThreshold, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public long getOffloadThreshold(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override long GetOffloadThreshold(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "offloadThreshold");
				return Request(Path).get(typeof(Long));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setOffloadThreshold(String namespace, long offloadThreshold) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetOffloadThreshold(string Namespace, long OffloadThreshold)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "offloadThreshold");
				Request(Path).put(Entity.entity(OffloadThreshold, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public System.Nullable<long> getOffloadDeleteLagMs(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override long? GetOffloadDeleteLagMs(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "offloadDeletionLagMs");
				return Request(Path).get(typeof(Long));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setOffloadDeleteLag(String namespace, long lag, java.util.concurrent.TimeUnit unit) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetOffloadDeleteLag(string Namespace, long Lag, TimeUnit Unit)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "offloadDeletionLagMs");
				Request(Path).put(Entity.entity(TimeUnit.MILLISECONDS.convert(Lag, Unit), MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearOffloadDeleteLag(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void ClearOffloadDeleteLag(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "offloadDeletionLagMs");
				Request(Path).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SchemaAutoUpdateCompatibilityStrategy getSchemaAutoUpdateCompatibilityStrategy(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override SchemaAutoUpdateCompatibilityStrategy? GetSchemaAutoUpdateCompatibilityStrategy(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "schemaAutoUpdateCompatibilityStrategy");
				return Request(Path).get(typeof(SchemaAutoUpdateCompatibilityStrategy));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setSchemaAutoUpdateCompatibilityStrategy(String namespace, org.apache.pulsar.common.policies.data.SchemaAutoUpdateCompatibilityStrategy strategy) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetSchemaAutoUpdateCompatibilityStrategy(string Namespace, SchemaAutoUpdateCompatibilityStrategy Strategy)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "schemaAutoUpdateCompatibilityStrategy");
				Request(Path).put(Entity.entity(Strategy, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public boolean getSchemaValidationEnforced(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override bool GetSchemaValidationEnforced(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "schemaValidationEnforced");
				return Request(Path).get(typeof(Boolean));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setSchemaValidationEnforced(String namespace, boolean schemaValidationEnforced) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetSchemaValidationEnforced(string Namespace, bool SchemaValidationEnforced)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "schemaValidationEnforced");
				Request(Path).post(Entity.entity(SchemaValidationEnforced, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SchemaCompatibilityStrategy getSchemaCompatibilityStrategy(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override SchemaCompatibilityStrategy GetSchemaCompatibilityStrategy(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "schemaCompatibilityStrategy");
				return Request(Path).get(typeof(SchemaCompatibilityStrategy));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setSchemaCompatibilityStrategy(String namespace, org.apache.pulsar.common.policies.data.SchemaCompatibilityStrategy strategy) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetSchemaCompatibilityStrategy(string Namespace, SchemaCompatibilityStrategy Strategy)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "schemaCompatibilityStrategy");
				Request(Path).put(Entity.entity(Strategy, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public boolean getIsAllowAutoUpdateSchema(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override bool GetIsAllowAutoUpdateSchema(string Namespace)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "isAllowAutoUpdateSchema");
				return Request(Path).get(typeof(Boolean));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setIsAllowAutoUpdateSchema(String namespace, boolean isAllowAutoUpdateSchema) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void SetIsAllowAutoUpdateSchema(string Namespace, bool IsAllowAutoUpdateSchema)
		{
			try
			{
				NamespaceName Ns = NamespaceName.get(Namespace);
				WebTarget Path = NamespacePath(Ns, "isAllowAutoUpdateSchema");
				Request(Path).post(Entity.entity(IsAllowAutoUpdateSchema, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		private WebTarget NamespacePath(NamespaceName Namespace, params string[] Parts)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget super = namespace.isV2() ? adminV2Namespaces : adminNamespaces;
			WebTarget Base = Namespace.V2 ? adminV2Namespaces : adminNamespaces;
			WebTarget NamespacePath = Base.path(Namespace.ToString());
			NamespacePath = WebTargets.AddParts(NamespacePath, Parts);
			return NamespacePath;
		}
	}

}