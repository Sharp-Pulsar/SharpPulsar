using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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
namespace SharpPulsar.Sql.Naming
{
	/// <summary>
	/// Encapsulate the parsing of the completeTopicName name.
	/// </summary>
	public class TopicName : ServiceUnitId
	{
		public const string PublicTenant = "public";
		public const string DefaultNamespace = "default";

		public const string PartitionedTopicSuffix = "-partition-";

		private readonly string _completeTopicName;

		private readonly TopicDomain _domain;
		private readonly string _tenant;
		private readonly string _cluster;
		private readonly string _namespacePortion;
		private readonly string _localName;

		private readonly NamespaceName _namespaceName;

		private readonly int _partitionIndex;

        private static readonly ConcurrentDictionary<string, TopicName> Cache = new ConcurrentDictionary<string, TopicName>();
		public TopicName()
		{

		}
		private class CacheLoaderAnonymousInnerClass //: CacheLoader<string, TopicName>
		{
			public TopicName Load(string name)
			{
				return new TopicName(name.ToLower());
			}
		}

		public static readonly TopicName TransactionCoordinatorAssign = Get(TopicDomain.Persistent.Value(), NamespaceName.SystemNamespace, "transaction_coordinator_assign");

		public static TopicName Get(string domain, NamespaceName namespaceName, string topic)
		{
			var name = domain + "://" + namespaceName.ToString() + '/' + topic;
			return Get(name.ToLower());
		}

		public static TopicName Get(string domain, string tenant, string @namespace, string topic)
		{
			string name = domain + "://" + tenant + '/' + @namespace + '/' + topic;
			return Get(name.ToLower());
		}

		public static TopicName Get(string domain, string tenant, string cluster, string @namespace, string topic)
		{
			string name = domain + "://" + tenant + '/' + cluster + '/' + @namespace + '/' + topic;
			return Get(name.ToLower());
		}

		public static TopicName Get(string topic)
		{
			try
			{
				if(!Cache.ContainsKey(topic))
					Cache[topic] = new TopicName(topic);
				return Cache[topic];
			}
			catch (Exception e)
			{
				throw;
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
						completeTopicName = TopicDomain.Persistent.Value() + "://" + completeTopicName;
					}
					else if (prts.Length == 1)
					{
						completeTopicName = TopicDomain.Persistent.Value() + "://" + PublicTenant + "/" + DefaultNamespace + "/" + prts[0];
					}
					else
					{
						throw new ArgumentException("Invalid short topic name '" + completeTopicName + "', it should be in the format of " + "<tenant>/<namespace>/<topic> or <topic>");
					}
				}

				// The fully qualified topic name can be in two different forms:
				// new:    persistent://tenant/namespace/topic
				// legacy: persistent://tenant/cluster/namespace/topic

                IList<string> parts = completeTopicName.Split("://").Take(2).ToList();  
				_domain = TopicDomain.GetEnum(parts[0]);

				string rest = parts[1];

				// The rest of the name can be in different forms:
				// new:    tenant/namespace/<localName>
				// legacy: tenant/cluster/namespace/<localName>
				// Examples of localName:
				// 1. some/name/xyz//
				// 2. /xyz-123/feeder-2


				parts = rest.Split("/").Take(4).ToList();
				if (parts.Count == 3)
				{
					// New topic name without cluster name
					_tenant = parts[0];
					_cluster = null;
					_namespacePortion = parts[1];
					_localName = parts[2];
					_partitionIndex = GetPartitionIndex(completeTopicName);
					_namespaceName = NamespaceName.Get(_tenant, _namespacePortion);
				}
				else if (parts.Count == 4)
				{
					// Legacy topic name that includes cluster name
					_tenant = parts[0];
					_cluster = parts[1];
					_namespacePortion = parts[2];
					_localName = parts[3];
					_partitionIndex = GetPartitionIndex(completeTopicName);
					_namespaceName = NamespaceName.Get(_tenant, _cluster, _namespacePortion);
				}
				else
				{
					throw new ArgumentException("Invalid topic name: " + completeTopicName);
				}


				if (ReferenceEquals(_localName, null) || _localName.Length == 0)
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
				_completeTopicName = $"{_domain}://{_tenant}/{_namespacePortion}/{_localName}";
			}
			else
			{
				_completeTopicName = $"{_domain}://{_tenant}/{_cluster}/{_namespacePortion}/{_localName}";
			}
		}

		public virtual bool Persistent => TopicDomain.Persistent == _domain;

        /// <summary>
		/// Extract the namespace portion out of a completeTopicName name.
		/// 
		/// <para>Works both with old & new convention.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the namespace </returns>
		public virtual string Namespace => _namespaceName.ToString();

        /// <summary>
		/// Get the namespace object that this completeTopicName belongs to.
		/// </summary>
		/// <returns> namespace object </returns>
		public virtual NamespaceName NamespaceObject => _namespaceName;

        public virtual TopicDomain Domain => _domain;

        public virtual string Tenant => _tenant;

        [Obsolete]
		public virtual string Cluster => _cluster;

        public virtual string NamespacePortion => _namespacePortion;

        public virtual string LocalName => _localName;

        public virtual string EncodedLocalName => Codec.Encode(_localName);

        public TopicName GetPartition(int index)
		{
			if (index == -1 || ToString().Contains(PartitionedTopicSuffix))
			{
				return this;
			}
			string partitionName = ToString() + PartitionedTopicSuffix + index;
			return Get(partitionName);
		}

		/// <returns> partition index of the completeTopicName.
		/// It returns -1 if the completeTopicName (topic) is not partitioned. </returns>
		public virtual int PartitionIndex => _partitionIndex;

        public virtual bool Partitioned => _partitionIndex != -1;

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
					return _completeTopicName.Substring(0, _completeTopicName.LastIndexOf("-partition-", StringComparison.Ordinal));
				}
				else
				{
					return _completeTopicName;
				}
			}
		}

		/// <returns> partition index of the completeTopicName.
		/// It returns -1 if the completeTopicName (topic) is not partitioned. </returns>
		public static int GetPartitionIndex(string topic)
		{
			int partitionIndex = -1;
			if (topic.Contains(PartitionedTopicSuffix))
			{
				try
				{
					partitionIndex = int.Parse(topic.Substring(topic.LastIndexOf('-') + 1));
				}
				catch (FormatException)
				{
					Log.LogWarning("Could not get the partition index from the topic {}", topic);
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
					return string.Format("{0}/{1}/{2}/{3}", _domain, _tenant, _namespacePortion, EncodedLocalName);
				}
				else
				{
					return string.Format("{0}/{1}/{2}/{3}/{4}", _domain, _tenant, _cluster, _namespacePortion, EncodedLocalName);
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
					return string.Format("{0}/{1}/{2}/{3}", _tenant, _namespacePortion, _domain, EncodedLocalName);
				}
				else
				{
					return string.Format("{0}/{1}/{2}/{3}/{4}", _tenant, _cluster, _namespacePortion, _domain, EncodedLocalName);
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
					return string.Format("{0}/{1}/{2}/{3}", _domain, _tenant, _namespacePortion, EncodedLocalName);
				}
				else
				{
					return string.Format("{0}/{1}/{2}/{3}/{4}", _domain, _tenant, _cluster, _namespacePortion, EncodedLocalName);
				}
			}
		}

		public virtual bool Global => ReferenceEquals(_cluster, null) || Constants.GlobalCluster.Equals(_cluster, StringComparison.OrdinalIgnoreCase);

        public virtual string SchemaName => Tenant + "/" + NamespacePortion + "/" + EncodedLocalName;

        public override string ToString()
		{
			return _completeTopicName;
		}

		public override bool Equals(object obj)
		{
			if (obj is TopicName)
			{
				TopicName other = (TopicName) obj;
				return Equals(_completeTopicName, other._completeTopicName);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return _completeTopicName.GetHashCode();
		}

		public virtual bool Includes(TopicName otherTopicName)
		{
			return Equals(otherTopicName);
		}

		/// <summary>
		/// Returns true if this a V2 topic name prop/ns/topic-name. </summary>
		/// <returns> true if V2 </returns>
		public virtual bool V2 => ReferenceEquals(_cluster, null);
    }

}