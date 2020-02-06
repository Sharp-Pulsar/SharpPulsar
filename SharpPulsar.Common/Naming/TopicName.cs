using org.apache.pulsar.common.util;
using System;
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
namespace SharpPulsar.Common.Naming
{
	/// <summary>
	/// Encapsulate the parsing of the completeTopicName name.
	/// </summary>
	public class TopicName : ServiceUnitId
	{

		private static readonly Logger log = LoggerFactory.getLogger(typeof(TopicName));

		public const string PUBLIC_TENANT = "public";
		public const string DEFAULT_NAMESPACE = "default";

		public const string PARTITIONED_TOPIC_SUFFIX = "-partition-";

		private readonly string completeTopicName;

		private readonly TopicDomain domain;
		private readonly string tenant;
		private readonly string cluster;
		private readonly string namespacePortion;
		private readonly string localName;

		private readonly NamespaceName namespaceName;

		private readonly int partitionIndex;

		//private static readonly LoadingCache<string, TopicName> cache = CacheBuilder.newBuilder().maximumSize(100000).expireAfterAccess(30, TimeUnit.MINUTES).build(new CacheLoaderAnonymousInnerClass());
		public TopicName()
		{

		}
		private class CacheLoaderAnonymousInnerClass //: CacheLoader<string, TopicName>
		{
			public TopicName Load(string name)
			{
				return new TopicName(name);
			}
		}

		public static readonly TopicName TRANSACTION_COORDINATOR_ASSIGN = TopicName.Get(TopicDomain.persistent.value(), NamespaceName.SYSTEM_NAMESPACE, "transaction_coordinator_assign");

		public static TopicName Get(string domain, NamespaceName namespaceName, string topic)
		{
			string name = domain + "://" + namespaceName.ToString() + '/' + topic;
			return TopicName.Get(name);
		}

		public static TopicName get(string domain, string tenant, string @namespace, string topic)
		{
			string name = domain + "://" + tenant + '/' + @namespace + '/' + topic;
			return TopicName.Get(name);
		}

		public static TopicName Get(string domain, string tenant, string cluster, string @namespace, string topic)
		{
			string name = domain + "://" + tenant + '/' + cluster + '/' + @namespace + '/' + topic;
			return TopicName.Get(name);
		}

		public static TopicName Get(string topic)
		{
			try
			{
				return cache.get(topic);
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

		public static bool IsValid(string topic)
		{
			try
			{
				Get(topic);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private TopicName(string completeTopicName)
		{
			try
			{
				// The topic name can be in two different forms, one is fully qualified topic name,
				// the other one is short topic name
				if (!completeTopicName.Contains("://"))
				{
					// The short topic name can be:
					// - <topic>
					// - <property>/<namespace>/<topic>
					var prts = completeTopicName.Split('/');
					if (prts.Length == 3)
					{
						completeTopicName = TopicDomain.persistent.value() + "://" + completeTopicName;
					}
					else if (prts.Length == 1)
					{
						completeTopicName = TopicDomain.persistent.value() + "://" + PUBLIC_TENANT + "/" + DEFAULT_NAMESPACE + "/" + prts[0];
					}
					else
					{
						throw new ArgumentException("Invalid short topic name '" + completeTopicName + "', it should be in the format of " + "<tenant>/<namespace>/<topic> or <topic>");
					}
				}

				// The fully qualified topic name can be in two different forms:
				// new:    persistent://tenant/namespace/topic
				// legacy: persistent://tenant/cluster/namespace/topic

				IList<string> parts = Splitter.on("://").limit(2).splitToList(completeTopicName);
				this.domain = TopicDomain.getEnum(parts[0]);

				string rest = parts[1];

				// The rest of the name can be in different forms:
				// new:    tenant/namespace/<localName>
				// legacy: tenant/cluster/namespace/<localName>
				// Examples of localName:
				// 1. some/name/xyz//
				// 2. /xyz-123/feeder-2


				parts = Splitter.on("/").limit(4).splitToList(rest);
				if (parts.Count == 3)
				{
					// New topic name without cluster name
					this.tenant = parts[0];
					this.cluster = null;
					this.namespacePortion = parts[1];
					this.localName = parts[2];
					this.partitionIndex = getPartitionIndex(completeTopicName);
					this.namespaceName = NamespaceName.get(tenant, namespacePortion);
				}
				else if (parts.Count == 4)
				{
					// Legacy topic name that includes cluster name
					this.tenant = parts[0];
					this.cluster = parts[1];
					this.namespacePortion = parts[2];
					this.localName = parts[3];
					this.partitionIndex = getPartitionIndex(completeTopicName);
					this.namespaceName = NamespaceName.get(tenant, cluster, namespacePortion);
				}
				else
				{
					throw new ArgumentException("Invalid topic name: " + completeTopicName);
				}


				if (string.ReferenceEquals(localName, null) || localName.Length == 0)
				{
					throw new ArgumentException("Invalid topic name: " + completeTopicName);
				}

			}
			catch (NullReferenceException e)
			{
				throw new ArgumentException("Invalid topic name: " + completeTopicName, e);
			}
			if (V2)
			{
				this.completeTopicName = string.Format("{0}://{1}/{2}/{3}", domain, tenant, namespacePortion, localName);
			}
			else
			{
				this.completeTopicName = string.Format("{0}://{1}/{2}/{3}/{4}", domain, tenant, cluster, namespacePortion, localName);
			}
		}

		public virtual bool Persistent
		{
			get
			{
				return TopicDomain.persistent == domain;
			}
		}

		/// <summary>
		/// Extract the namespace portion out of a completeTopicName name.
		/// 
		/// <para>Works both with old & new convention.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the namespace </returns>
		public virtual string Namespace
		{
			get
			{
				return namespaceName.ToString();
			}
		}

		/// <summary>
		/// Get the namespace object that this completeTopicName belongs to.
		/// </summary>
		/// <returns> namespace object </returns>
		public virtual NamespaceName NamespaceObject
		{
			get
			{
				return namespaceName;
			}
		}

		public virtual TopicDomain Domain
		{
			get
			{
				return domain;
			}
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

		public virtual string NamespacePortion
		{
			get
			{
				return namespacePortion;
			}
		}

		public virtual string LocalName
		{
			get
			{
				return localName;
			}
		}

		public virtual string EncodedLocalName
		{
			get
			{
				return Codec.encode(localName);
			}
		}

		public TopicName GetPartition(int index)
		{
			if (index == -1 || this.ToString().Contains(PARTITIONED_TOPIC_SUFFIX))
			{
				return this;
			}
			string partitionName = this.ToString() + PARTITIONED_TOPIC_SUFFIX + index;
			return Get(partitionName);
		}

		/// <returns> partition index of the completeTopicName.
		/// It returns -1 if the completeTopicName (topic) is not partitioned. </returns>
		public virtual int PartitionIndex
		{
			get
			{
				return partitionIndex;
			}
		}

		public virtual bool Partitioned
		{
			get
			{
				return partitionIndex != -1;
			}
		}

		/// <summary>
		/// For partitions in a topic, return the base partitioned topic name.
		/// Eg:
		/// <ul>
		///  <li><code>persistent://prop/cluster/ns/my-topic-partition-1</code> -->
		///  <code>persistent://prop/cluster/ns/my-topic</code>
		///  <li><code>persistent://prop/cluster/ns/my-topic</code> --> <code>persistent://prop/cluster/ns/my-topic</code>
		/// </ul>
		/// </summary>
		public virtual string PartitionedTopicName
		{
			get
			{
				if (Partitioned)
				{
					return completeTopicName.Substring(0, completeTopicName.LastIndexOf("-partition-", StringComparison.Ordinal));
				}
				else
				{
					return completeTopicName;
				}
			}
		}

		/// <returns> partition index of the completeTopicName.
		/// It returns -1 if the completeTopicName (topic) is not partitioned. </returns>
		public static int GetPartitionIndex(string topic)
		{
			int partitionIndex = -1;
			if (topic.Contains(PARTITIONED_TOPIC_SUFFIX))
			{
				try
				{
					partitionIndex = int.Parse(topic.Substring(topic.LastIndexOf('-') + 1));
				}
				catch (FormatException)
				{
					log.warn("Could not get the partition index from the topic {}", topic);
				}
			}

			return partitionIndex;
		}

		/// <summary>
		/// Returns the http rest path for use in the admin web service.
		/// Eg:
		///   * "persistent/my-tenant/my-namespace/my-topic"
		///   * "non-persistent/my-tenant/my-namespace/my-topic"
		/// </summary>
		/// <returns> topic rest path </returns>
		public virtual string RestPath
		{
			get
			{
				if (V2)
				{
					return string.Format("{0}/{1}/{2}/{3}", domain, tenant, namespacePortion, EncodedLocalName);
				}
				else
				{
					return string.Format("{0}/{1}/{2}/{3}/{4}", domain, tenant, cluster, namespacePortion, EncodedLocalName);
				}
			}
		}

		/// <summary>
		/// Returns the name of the persistence resource associated with the completeTopicName.
		/// </summary>
		/// <returns> the relative path to be used in persistence </returns>
		public virtual string PersistenceNamingEncoding
		{
			get
			{
				// The convention is: domain://tenant/namespace/topic
				// We want to persist in the order: tenant/namespace/domain/topic
    
				// For legacy naming scheme, the convention is: domain://tenant/cluster/namespace/topic
				// We want to persist in the order: tenant/cluster/namespace/domain/topic
				if (V2)
				{
					return string.Format("{0}/{1}/{2}/{3}", tenant, namespacePortion, domain, EncodedLocalName);
				}
				else
				{
					return string.Format("{0}/{1}/{2}/{3}/{4}", tenant, cluster, namespacePortion, domain, EncodedLocalName);
				}
			}
		}

		/// <summary>
		/// Get a string suitable for completeTopicName lookup.
		/// 
		/// <para>Example:
		/// 
		/// </para>
		/// <para>persistent://tenant/cluster/namespace/completeTopicName ->
		///   persistent/tenant/cluster/namespace/completeTopicName
		/// 
		/// @return
		/// </para>
		/// </summary>
		public virtual string LookupName
		{
			get
			{
				if (V2)
				{
					return string.Format("{0}/{1}/{2}/{3}", domain, tenant, namespacePortion, EncodedLocalName);
				}
				else
				{
					return string.Format("{0}/{1}/{2}/{3}/{4}", domain, tenant, cluster, namespacePortion, EncodedLocalName);
				}
			}
		}

		public virtual bool Global
		{
			get
			{
				return string.ReferenceEquals(cluster, null) || Constants.GLOBAL_CLUSTER.Equals(cluster, StringComparison.OrdinalIgnoreCase);
			}
		}

		public virtual string SchemaName
		{
			get
			{
				return Tenant + "/" + NamespacePortion + "/" + EncodedLocalName;
			}
		}

		public override string ToString()
		{
			return completeTopicName;
		}

		public override bool Equals(object obj)
		{
			if (obj is TopicName)
			{
				TopicName other = (TopicName) obj;
				return object.Equals(completeTopicName, other.completeTopicName);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return completeTopicName.GetHashCode();
		}

		public virtual bool includes(TopicName otherTopicName)
		{
			return this.Equals(otherTopicName);
		}

		/// <summary>
		/// Returns true if this a V2 topic name prop/ns/topic-name. </summary>
		/// <returns> true if V2 </returns>
		public virtual bool V2
		{
			get
			{
				return string.ReferenceEquals(cluster, null);
			}
		}
	}

}