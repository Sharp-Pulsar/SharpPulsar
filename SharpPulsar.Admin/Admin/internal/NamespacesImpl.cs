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
	using NamespaceName = org.apache.pulsar.common.naming.NamespaceName;
	using AuthAction = org.apache.pulsar.common.policies.data.AuthAction;
	using BacklogQuota = org.apache.pulsar.common.policies.data.BacklogQuota;
	using BookieAffinityGroupData = org.apache.pulsar.common.policies.data.BookieAffinityGroupData;
	using BacklogQuotaType = org.apache.pulsar.common.policies.data.BacklogQuota.BacklogQuotaType;
	using BundlesData = org.apache.pulsar.common.policies.data.BundlesData;
	using DispatchRate = org.apache.pulsar.common.policies.data.DispatchRate;
	using ErrorData = org.apache.pulsar.common.policies.data.ErrorData;
	using PersistencePolicies = org.apache.pulsar.common.policies.data.PersistencePolicies;
	using Policies = org.apache.pulsar.common.policies.data.Policies;
	using PublishRate = org.apache.pulsar.common.policies.data.PublishRate;
	using RetentionPolicies = org.apache.pulsar.common.policies.data.RetentionPolicies;
	using SchemaAutoUpdateCompatibilityStrategy = org.apache.pulsar.common.policies.data.SchemaAutoUpdateCompatibilityStrategy;
	using SchemaCompatibilityStrategy = org.apache.pulsar.common.policies.data.SchemaCompatibilityStrategy;
	using SubscribeRate = org.apache.pulsar.common.policies.data.SubscribeRate;
	using SubscriptionAuthMode = org.apache.pulsar.common.policies.data.SubscriptionAuthMode;

	public class NamespacesImpl : BaseResource, Namespaces
	{

		private readonly WebTarget adminNamespaces;
		private readonly WebTarget adminV2Namespaces;

		public NamespacesImpl(WebTarget web, Authentication auth, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			adminNamespaces = web.path("/admin/namespaces");
			adminV2Namespaces = web.path("/admin/v2/namespaces");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getNamespaces(String tenant) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> getNamespaces(string tenant)
		{
			try
			{
				WebTarget path = adminV2Namespaces.path(tenant);
				return request(path).get(new GenericTypeAnonymousInnerClass(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass : GenericType<IList<string>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass(NamespacesImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getNamespaces(String tenant, String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> getNamespaces(string tenant, string cluster)
		{
			try
			{
				WebTarget path = adminNamespaces.path(tenant).path(cluster);
				return request(path).get(new GenericTypeAnonymousInnerClass2(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass2 : GenericType<IList<string>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(NamespacesImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getTopics(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> getTopics(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				string action = ns.V2 ? "topics" : "destinations";
				WebTarget path = namespacePath(ns, action);
				return request(path).get(new GenericTypeAnonymousInnerClass3(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass3 : GenericType<IList<string>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass3(NamespacesImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.Policies getPolicies(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual Policies getPolicies(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns);
				return request(path).get(typeof(Policies));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNamespace(String namespace, java.util.Set<String> clusters) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createNamespace(string @namespace, ISet<string> clusters)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns);

				if (ns.V2)
				{
					// For V2 API we pass full Policy class instance
					Policies policies = new Policies();
					policies.replication_clusters = clusters;
					request(path).put(Entity.entity(policies, MediaType.APPLICATION_JSON), typeof(ErrorData));
				}
				else
				{
					// For V1 API, we pass the BundlesData on creation
					request(path).put(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
					// For V1, we need to do it in 2 steps
					setNamespaceReplicationClusters(@namespace, clusters);
				}
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNamespace(String namespace, int numBundles) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createNamespace(string @namespace, int numBundles)
		{
			createNamespace(@namespace, new BundlesData(numBundles));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNamespace(String namespace, org.apache.pulsar.common.policies.data.Policies policies) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createNamespace(string @namespace, Policies policies)
		{
			NamespaceName ns = NamespaceName.get(@namespace);
			checkArgument(ns.V2, "Create namespace with policies is only supported on newer namespaces");

			try
			{
				WebTarget path = namespacePath(ns);

				// For V2 API we pass full Policy class instance
				request(path).put(Entity.entity(policies, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNamespace(String namespace, org.apache.pulsar.common.policies.data.BundlesData bundlesData) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createNamespace(string @namespace, BundlesData bundlesData)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns);

				if (ns.V2)
				{
					// For V2 API we pass full Policy class instance
					Policies policies = new Policies();
					policies.bundles = bundlesData;
					request(path).put(Entity.entity(policies, MediaType.APPLICATION_JSON), typeof(ErrorData));
				}
				else
				{
					// For V1 API, we pass the BundlesData on creation
					request(path).put(Entity.entity(bundlesData, MediaType.APPLICATION_JSON), typeof(ErrorData));
				}
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createNamespace(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createNamespace(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns);
				request(path).put(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteNamespace(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteNamespace(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns);
				request(path).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteNamespaceBundle(String namespace, String bundleRange) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteNamespaceBundle(string @namespace, string bundleRange)
		{
			try
			{
				deleteNamespaceBundleAsync(@namespace, bundleRange).get();
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
		}

		public virtual CompletableFuture<Void> deleteNamespaceBundleAsync(string @namespace, string bundleRange)
		{
			NamespaceName ns = NamespaceName.get(@namespace);
			WebTarget path = namespacePath(ns, bundleRange);
			return asyncDeleteRequest(path);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction>> getPermissions(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IDictionary<string, ISet<AuthAction>> getPermissions(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "permissions");
				return request(path).get(new GenericTypeAnonymousInnerClass4(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass4 : GenericType<IDictionary<string, ISet<AuthAction>>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass4(NamespacesImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void grantPermissionOnNamespace(String namespace, String role, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction> actions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void grantPermissionOnNamespace(string @namespace, string role, ISet<AuthAction> actions)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "permissions", role);
				request(path).post(Entity.entity(actions, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void revokePermissionsOnNamespace(String namespace, String role) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void revokePermissionsOnNamespace(string @namespace, string role)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "permissions", role);
				request(path).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void grantPermissionOnSubscription(String namespace, String subscription, java.util.Set<String> roles) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void grantPermissionOnSubscription(string @namespace, string subscription, ISet<string> roles)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "permissions", "subscription", subscription);
				request(path).post(Entity.entity(roles, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void revokePermissionOnSubscription(String namespace, String subscription, String role) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void revokePermissionOnSubscription(string @namespace, string subscription, string role)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "permissions", subscription, role);
				request(path).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getNamespaceReplicationClusters(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> getNamespaceReplicationClusters(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "replication");
				return request(path).get(new GenericTypeAnonymousInnerClass5(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass5 : GenericType<IList<string>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass5(NamespacesImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setNamespaceReplicationClusters(String namespace, java.util.Set<String> clusterIds) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setNamespaceReplicationClusters(string @namespace, ISet<string> clusterIds)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "replication");
				request(path).post(Entity.entity(clusterIds, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public int getNamespaceMessageTTL(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual int getNamespaceMessageTTL(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "messageTTL");
				return request(path).get(new GenericTypeAnonymousInnerClass6(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass6 : GenericType<int>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass6(NamespacesImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setNamespaceMessageTTL(String namespace, int ttlInSeconds) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setNamespaceMessageTTL(string @namespace, int ttlInSeconds)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "messageTTL");
				request(path).post(Entity.entity(ttlInSeconds, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setNamespaceAntiAffinityGroup(String namespace, String namespaceAntiAffinityGroup) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setNamespaceAntiAffinityGroup(string @namespace, string namespaceAntiAffinityGroup)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "antiAffinity");
				request(path).post(Entity.entity(namespaceAntiAffinityGroup, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public String getNamespaceAntiAffinityGroup(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual string getNamespaceAntiAffinityGroup(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "antiAffinity");
				return request(path).get(new GenericTypeAnonymousInnerClass7(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass7 : GenericType<string>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass7(NamespacesImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getAntiAffinityNamespaces(String tenant, String cluster, String namespaceAntiAffinityGroup) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> getAntiAffinityNamespaces(string tenant, string cluster, string namespaceAntiAffinityGroup)
		{
			try
			{
				WebTarget path = adminNamespaces.path(cluster).path("antiAffinity").path(namespaceAntiAffinityGroup);
				return request(path.queryParam("property", tenant)).get(new GenericTypeAnonymousInnerClass8(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass8 : GenericType<IList<string>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass8(NamespacesImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteNamespaceAntiAffinityGroup(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteNamespaceAntiAffinityGroup(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "antiAffinity");
				request(path).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setDeduplicationStatus(String namespace, boolean enableDeduplication) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setDeduplicationStatus(string @namespace, bool enableDeduplication)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "deduplication");
				request(path).post(Entity.entity(enableDeduplication, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<org.apache.pulsar.common.policies.data.BacklogQuota.BacklogQuotaType, org.apache.pulsar.common.policies.data.BacklogQuota> getBacklogQuotaMap(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IDictionary<BacklogQuota.BacklogQuotaType, BacklogQuota> getBacklogQuotaMap(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "backlogQuotaMap");
				return request(path).get(new GenericTypeAnonymousInnerClass9(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass9 : GenericType<IDictionary<BacklogQuota.BacklogQuotaType, BacklogQuota>>
		{
			private readonly NamespacesImpl outerInstance;

			public GenericTypeAnonymousInnerClass9(NamespacesImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setBacklogQuota(String namespace, org.apache.pulsar.common.policies.data.BacklogQuota backlogQuota) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setBacklogQuota(string @namespace, BacklogQuota backlogQuota)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "backlogQuota");
				request(path).post(Entity.entity(backlogQuota, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void removeBacklogQuota(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void removeBacklogQuota(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "backlogQuota");
				request(path.queryParam("backlogQuotaType", BacklogQuota.BacklogQuotaType.destination_storage.ToString())).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setPersistence(String namespace, org.apache.pulsar.common.policies.data.PersistencePolicies persistence) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setPersistence(string @namespace, PersistencePolicies persistence)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "persistence");
				request(path).post(Entity.entity(persistence, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setBookieAffinityGroup(String namespace, org.apache.pulsar.common.policies.data.BookieAffinityGroupData bookieAffinityGroup) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setBookieAffinityGroup(string @namespace, BookieAffinityGroupData bookieAffinityGroup)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "persistence", "bookieAffinity");
				request(path).post(Entity.entity(bookieAffinityGroup, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteBookieAffinityGroup(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteBookieAffinityGroup(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "persistence", "bookieAffinity");
				request(path).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.BookieAffinityGroupData getBookieAffinityGroup(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual BookieAffinityGroupData getBookieAffinityGroup(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "persistence", "bookieAffinity");
				return request(path).get(typeof(BookieAffinityGroupData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.PersistencePolicies getPersistence(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual PersistencePolicies getPersistence(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "persistence");
				return request(path).get(typeof(PersistencePolicies));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setRetention(String namespace, org.apache.pulsar.common.policies.data.RetentionPolicies retention) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setRetention(string @namespace, RetentionPolicies retention)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "retention");
				request(path).post(Entity.entity(retention, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.RetentionPolicies getRetention(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual RetentionPolicies getRetention(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "retention");
				return request(path).get(typeof(RetentionPolicies));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void unload(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void unload(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "unload");
				request(path).put(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public String getReplicationConfigVersion(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual string getReplicationConfigVersion(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "configversion");
				return request(path).get(typeof(string));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void unloadNamespaceBundle(String namespace, String bundle) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void unloadNamespaceBundle(string @namespace, string bundle)
		{
			try
			{
				unloadNamespaceBundleAsync(@namespace, bundle).get();
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
		}

		public virtual CompletableFuture<Void> unloadNamespaceBundleAsync(string @namespace, string bundle)
		{
			NamespaceName ns = NamespaceName.get(@namespace);
			WebTarget path = namespacePath(ns, bundle, "unload");
			return asyncPutRequest(path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void splitNamespaceBundle(String namespace, String bundle, boolean unloadSplitBundles) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void splitNamespaceBundle(string @namespace, string bundle, bool unloadSplitBundles)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, bundle, "split");
				request(path.queryParam("unload", Convert.ToString(unloadSplitBundles))).put(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setPublishRate(String namespace, org.apache.pulsar.common.policies.data.PublishRate publishMsgRate) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setPublishRate(string @namespace, PublishRate publishMsgRate)
		{

			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "publishRate");
				request(path).post(Entity.entity(publishMsgRate, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.PublishRate getPublishRate(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual PublishRate getPublishRate(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "publishRate");
				return request(path).get(typeof(PublishRate));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setDispatchRate(String namespace, org.apache.pulsar.common.policies.data.DispatchRate dispatchRate) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setDispatchRate(string @namespace, DispatchRate dispatchRate)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "dispatchRate");
				request(path).post(Entity.entity(dispatchRate, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.DispatchRate getDispatchRate(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual DispatchRate getDispatchRate(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "dispatchRate");
				return request(path).get(typeof(DispatchRate));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setSubscribeRate(String namespace, org.apache.pulsar.common.policies.data.SubscribeRate subscribeRate) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setSubscribeRate(string @namespace, SubscribeRate subscribeRate)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "subscribeRate");
				request(path).post(Entity.entity(subscribeRate, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SubscribeRate getSubscribeRate(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual SubscribeRate getSubscribeRate(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "subscribeRate");
				return request(path).get(typeof(SubscribeRate));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setSubscriptionDispatchRate(String namespace, org.apache.pulsar.common.policies.data.DispatchRate dispatchRate) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setSubscriptionDispatchRate(string @namespace, DispatchRate dispatchRate)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "subscriptionDispatchRate");
				request(path).post(Entity.entity(dispatchRate, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.DispatchRate getSubscriptionDispatchRate(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual DispatchRate getSubscriptionDispatchRate(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "subscriptionDispatchRate");
				return request(path).get(typeof(DispatchRate));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setReplicatorDispatchRate(String namespace, org.apache.pulsar.common.policies.data.DispatchRate dispatchRate) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setReplicatorDispatchRate(string @namespace, DispatchRate dispatchRate)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "replicatorDispatchRate");
				request(path).post(Entity.entity(dispatchRate, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.DispatchRate getReplicatorDispatchRate(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual DispatchRate getReplicatorDispatchRate(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "replicatorDispatchRate");
				return request(path).get(typeof(DispatchRate));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearNamespaceBacklog(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void clearNamespaceBacklog(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "clearBacklog");
				request(path).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearNamespaceBacklogForSubscription(String namespace, String subscription) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void clearNamespaceBacklogForSubscription(string @namespace, string subscription)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "clearBacklog", subscription);
				request(path).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearNamespaceBundleBacklog(String namespace, String bundle) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void clearNamespaceBundleBacklog(string @namespace, string bundle)
		{
			try
			{
				clearNamespaceBundleBacklogAsync(@namespace, bundle).get();
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
		}

		public virtual CompletableFuture<Void> clearNamespaceBundleBacklogAsync(string @namespace, string bundle)
		{
			NamespaceName ns = NamespaceName.get(@namespace);
			WebTarget path = namespacePath(ns, bundle, "clearBacklog");
			return asyncPostRequest(path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearNamespaceBundleBacklogForSubscription(String namespace, String bundle, String subscription) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void clearNamespaceBundleBacklogForSubscription(string @namespace, string bundle, string subscription)
		{
			try
			{
				clearNamespaceBundleBacklogForSubscriptionAsync(@namespace, bundle, subscription).get();
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
		}

		public virtual CompletableFuture<Void> clearNamespaceBundleBacklogForSubscriptionAsync(string @namespace, string bundle, string subscription)
		{
			NamespaceName ns = NamespaceName.get(@namespace);
			WebTarget path = namespacePath(ns, bundle, "clearBacklog", subscription);
			return asyncPostRequest(path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void unsubscribeNamespace(String namespace, String subscription) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void unsubscribeNamespace(string @namespace, string subscription)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "unsubscribe", subscription);
				request(path).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void unsubscribeNamespaceBundle(String namespace, String bundle, String subscription) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void unsubscribeNamespaceBundle(string @namespace, string bundle, string subscription)
		{
			try
			{
				unsubscribeNamespaceBundleAsync(@namespace, bundle, subscription).get();
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
		}

		public virtual CompletableFuture<Void> unsubscribeNamespaceBundleAsync(string @namespace, string bundle, string subscription)
		{
			NamespaceName ns = NamespaceName.get(@namespace);
			WebTarget path = namespacePath(ns, bundle, "unsubscribe", subscription);
			return asyncPostRequest(path, Entity.entity("", MediaType.APPLICATION_JSON));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setSubscriptionAuthMode(String namespace, org.apache.pulsar.common.policies.data.SubscriptionAuthMode subscriptionAuthMode) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setSubscriptionAuthMode(string @namespace, SubscriptionAuthMode subscriptionAuthMode)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "subscriptionAuthMode");
				request(path).post(Entity.entity(subscriptionAuthMode, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setEncryptionRequiredStatus(String namespace, boolean encryptionRequired) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setEncryptionRequiredStatus(string @namespace, bool encryptionRequired)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "encryptionRequired");
				request(path).post(Entity.entity(encryptionRequired, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public int getMaxProducersPerTopic(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual int getMaxProducersPerTopic(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "maxProducersPerTopic");
				return request(path).get(typeof(Integer));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setMaxProducersPerTopic(String namespace, int maxProducersPerTopic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setMaxProducersPerTopic(string @namespace, int maxProducersPerTopic)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "maxProducersPerTopic");
				request(path).post(Entity.entity(maxProducersPerTopic, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public int getMaxConsumersPerTopic(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual int getMaxConsumersPerTopic(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "maxConsumersPerTopic");
				return request(path).get(typeof(Integer));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setMaxConsumersPerTopic(String namespace, int maxConsumersPerTopic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setMaxConsumersPerTopic(string @namespace, int maxConsumersPerTopic)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "maxConsumersPerTopic");
				request(path).post(Entity.entity(maxConsumersPerTopic, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public int getMaxConsumersPerSubscription(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual int getMaxConsumersPerSubscription(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "maxConsumersPerSubscription");
				return request(path).get(typeof(Integer));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setMaxConsumersPerSubscription(String namespace, int maxConsumersPerSubscription) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setMaxConsumersPerSubscription(string @namespace, int maxConsumersPerSubscription)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "maxConsumersPerSubscription");
				request(path).post(Entity.entity(maxConsumersPerSubscription, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public long getCompactionThreshold(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual long getCompactionThreshold(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "compactionThreshold");
				return request(path).get(typeof(Long));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setCompactionThreshold(String namespace, long compactionThreshold) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setCompactionThreshold(string @namespace, long compactionThreshold)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "compactionThreshold");
				request(path).put(Entity.entity(compactionThreshold, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public long getOffloadThreshold(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual long getOffloadThreshold(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "offloadThreshold");
				return request(path).get(typeof(Long));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setOffloadThreshold(String namespace, long offloadThreshold) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setOffloadThreshold(string @namespace, long offloadThreshold)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "offloadThreshold");
				request(path).put(Entity.entity(offloadThreshold, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public System.Nullable<long> getOffloadDeleteLagMs(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual long? getOffloadDeleteLagMs(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "offloadDeletionLagMs");
				return request(path).get(typeof(Long));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setOffloadDeleteLag(String namespace, long lag, java.util.concurrent.TimeUnit unit) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setOffloadDeleteLag(string @namespace, long lag, TimeUnit unit)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "offloadDeletionLagMs");
				request(path).put(Entity.entity(TimeUnit.MILLISECONDS.convert(lag, unit), MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearOffloadDeleteLag(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void clearOffloadDeleteLag(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "offloadDeletionLagMs");
				request(path).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SchemaAutoUpdateCompatibilityStrategy getSchemaAutoUpdateCompatibilityStrategy(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual SchemaAutoUpdateCompatibilityStrategy getSchemaAutoUpdateCompatibilityStrategy(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "schemaAutoUpdateCompatibilityStrategy");
				return request(path).get(typeof(SchemaAutoUpdateCompatibilityStrategy));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setSchemaAutoUpdateCompatibilityStrategy(String namespace, org.apache.pulsar.common.policies.data.SchemaAutoUpdateCompatibilityStrategy strategy) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setSchemaAutoUpdateCompatibilityStrategy(string @namespace, SchemaAutoUpdateCompatibilityStrategy strategy)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "schemaAutoUpdateCompatibilityStrategy");
				request(path).put(Entity.entity(strategy, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public boolean getSchemaValidationEnforced(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual bool getSchemaValidationEnforced(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "schemaValidationEnforced");
				return request(path).get(typeof(Boolean));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setSchemaValidationEnforced(String namespace, boolean schemaValidationEnforced) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setSchemaValidationEnforced(string @namespace, bool schemaValidationEnforced)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "schemaValidationEnforced");
				request(path).post(Entity.entity(schemaValidationEnforced, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SchemaCompatibilityStrategy getSchemaCompatibilityStrategy(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual SchemaCompatibilityStrategy getSchemaCompatibilityStrategy(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "schemaCompatibilityStrategy");
				return request(path).get(typeof(SchemaCompatibilityStrategy));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setSchemaCompatibilityStrategy(String namespace, org.apache.pulsar.common.policies.data.SchemaCompatibilityStrategy strategy) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setSchemaCompatibilityStrategy(string @namespace, SchemaCompatibilityStrategy strategy)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "schemaCompatibilityStrategy");
				request(path).put(Entity.entity(strategy, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public boolean getIsAllowAutoUpdateSchema(String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual bool getIsAllowAutoUpdateSchema(string @namespace)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "isAllowAutoUpdateSchema");
				return request(path).get(typeof(Boolean));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void setIsAllowAutoUpdateSchema(String namespace, boolean isAllowAutoUpdateSchema) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void setIsAllowAutoUpdateSchema(string @namespace, bool isAllowAutoUpdateSchema)
		{
			try
			{
				NamespaceName ns = NamespaceName.get(@namespace);
				WebTarget path = namespacePath(ns, "isAllowAutoUpdateSchema");
				request(path).post(Entity.entity(isAllowAutoUpdateSchema, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private WebTarget namespacePath(NamespaceName @namespace, params string[] parts)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.client.WebTarget super = namespace.isV2() ? adminV2Namespaces : adminNamespaces;
			WebTarget @base = @namespace.V2 ? adminV2Namespaces : adminNamespaces;
			WebTarget namespacePath = @base.path(@namespace.ToString());
			namespacePath = WebTargets.addParts(namespacePath, parts);
			return namespacePath;
		}
	}

}