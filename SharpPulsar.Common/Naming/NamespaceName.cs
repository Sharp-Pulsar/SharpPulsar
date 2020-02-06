using System;

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
namespace SharpPulsar.Common.Naming
{

	/// <summary>
	/// Parser of a value from the namespace field provided in configuration.
	/// </summary>
	public class NamespaceName : ServiceUnitId
	{

		private readonly string @namespace;

		private readonly string tenant;
		private readonly string cluster;
		private readonly string localName;

		private static readonly LoadingCache<string, NamespaceName> cache = CacheBuilder.newBuilder().maximumSize(100000).expireAfterAccess(30, TimeUnit.MINUTES).build(new CacheLoaderAnonymousInnerClass());

		private class CacheLoaderAnonymousInnerClass : CacheLoader<string, NamespaceName>
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public NamespaceName load(String name) throws Exception
			public override NamespaceName load(string name)
			{
				return new NamespaceName(name);
			}
		}

		public static readonly NamespaceName SYSTEM_NAMESPACE = NamespaceName.get("pulsar/system");

		public static NamespaceName get(string tenant, string @namespace)
		{
			validateNamespaceName(tenant, @namespace);
			return get(tenant + '/' + @namespace);
		}

		public static NamespaceName get(string tenant, string cluster, string @namespace)
		{
			validateNamespaceName(tenant, cluster, @namespace);
			return get(tenant + '/' + cluster + '/' + @namespace);
		}

		public static NamespaceName get(string @namespace)
		{
			try
			{
				checkNotNull(@namespace);
			}
			catch (NullReferenceException)
			{
				throw new ArgumentException("Invalid null namespace: " + @namespace);
			}
			try
			{
				return cache.get(@namespace);
			}
			catch (ExecutionException e)
			{
				throw (Exception) e.InnerException;
			}
			catch (UncheckedExecutionException e)
			{
				throw (Exception) e.InnerException;
			}
		}

		private NamespaceName(string @namespace)
		{
			if (string.ReferenceEquals(@namespace, null) || @namespace.Length == 0)
			{
				throw new ArgumentException("Invalid null namespace: " + @namespace);
			}

			// Verify it's a proper namespace
			// The namespace name is composed of <tenant>/<namespace>
			// or in the legacy format with the cluster name:
			// <tenant>/<cluster>/<namespace>
			try
			{

				string[] parts = @namespace.Split("/", true);
				if (parts.Length == 2)
				{
					// New style namespace : <tenant>/<namespace>
					validateNamespaceName(parts[0], parts[1]);

					tenant = parts[0];
					cluster = null;
					localName = parts[1];
				}
				else if (parts.Length == 3)
				{
					// Old style namespace: <tenant>/<cluster>/<namespace>
					validateNamespaceName(parts[0], parts[1], parts[2]);

					tenant = parts[0];
					cluster = parts[1];
					localName = parts[2];
				}
				else
				{
					throw new ArgumentException("Invalid namespace format. namespace: " + @namespace);
				}
			}
			catch (NullReferenceException e)
			{
				throw new ArgumentException("Invalid namespace format. namespace: " + @namespace, e);
			}
			this.@namespace = @namespace;
		}

		public virtual string Tenant
		{
			get
			{
				return tenant;
			}
		}

		[Obsolete]
		public virtual string Cluster
		{
			get
			{
				return cluster;
			}
		}

		public virtual string LocalName
		{
			get
			{
				return localName;
			}
		}

		public virtual bool Global
		{
			get
			{
				return string.ReferenceEquals(cluster, null) || Constants.GLOBAL_CLUSTER.Equals(cluster, StringComparison.OrdinalIgnoreCase);
			}
		}

		public virtual string getPersistentTopicName(string localTopic)
		{
			return getTopicName(TopicDomain.persistent, localTopic);
		}

		/// <summary>
		/// Compose the topic name from namespace + topic.
		/// </summary>
		/// <param name="domain"> </param>
		/// <param name="topic">
		/// @return </param>
		internal virtual string getTopicName(TopicDomain domain, string topic)
		{
			try
			{
				checkNotNull(domain);
				NamedEntity.checkName(topic);
				return string.Format("{0}://{1}/{2}", domain.ToString(), @namespace, topic);
			}
			catch (NullReferenceException e)
			{
				throw new ArgumentException("Null pointer is invalid as domain for topic.", e);
			}
		}

		public override string ToString()
		{
			return @namespace;
		}

		public override bool Equals(object obj)
		{
			if (obj is NamespaceName)
			{
				NamespaceName other = (NamespaceName) obj;
				return Objects.equal(@namespace, other.@namespace);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return @namespace.GetHashCode();
		}

		public static void validateNamespaceName(string tenant, string @namespace)
		{
			try
			{
				checkNotNull(tenant);
				checkNotNull(@namespace);
				if (tenant.Length == 0 || @namespace.Length == 0)
				{
					throw new ArgumentException(string.Format("Invalid namespace format. namespace: {0}/{1}", tenant, @namespace));
				}
				NamedEntity.checkName(tenant);
				NamedEntity.checkName(@namespace);
			}
			catch (NullReferenceException e)
			{
				throw new ArgumentException(string.Format("Invalid namespace format. namespace: {0}/{1}", tenant, @namespace), e);
			}
		}

		public static void validateNamespaceName(string tenant, string cluster, string @namespace)
		{
			try
			{
				checkNotNull(tenant);
				checkNotNull(cluster);
				checkNotNull(@namespace);
				if (tenant.Length == 0 || cluster.Length == 0 || @namespace.Length == 0)
				{
					throw new ArgumentException(string.Format("Invalid namespace format. namespace: {0}/{1}/{2}", tenant, cluster, @namespace));
				}
				NamedEntity.checkName(tenant);
				NamedEntity.checkName(cluster);
				NamedEntity.checkName(@namespace);
			}
			catch (NullReferenceException e)
			{
				throw new ArgumentException(string.Format("Invalid namespace format. namespace: {0}/{1}/{2}", tenant, cluster, @namespace), e);
			}
		}

		public virtual NamespaceName NamespaceObject
		{
			get
			{
				return this;
			}
		}

		public virtual bool includes(TopicName topicName)
		{
			return this.Equals(topicName.NamespaceObject);
		}

		/// <summary>
		/// Returns true if this is a V2 namespace prop/namespace-name. </summary>
		/// <returns> true if v2 </returns>
		public virtual bool V2
		{
			get
			{
				return string.ReferenceEquals(cluster, null);
			}
		}
	}

}