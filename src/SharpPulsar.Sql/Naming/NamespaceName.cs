using System;
using System.Collections.Concurrent;
using SharpPulsar.Sql.Precondition;

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
	/// Parser of a value from the namespace field provided in configuration.
	/// </summary>
	public class NamespaceName : ServiceUnitId
	{

		private readonly string _namespace;

		private readonly string _tenant;
		private readonly string _cluster;
		private readonly string _localName;

		private static readonly ConcurrentDictionary<string, NamespaceName> Cache = new ConcurrentDictionary<string, NamespaceName>();
		

		public static readonly NamespaceName SystemNamespace = NamespaceName.Get("pulsar/system");

		public static NamespaceName Get(string tenant, string @namespace)
		{
			ValidateNamespaceName(tenant, @namespace);
			return Get(tenant + '/' + @namespace);
		}

		public static NamespaceName Get(string tenant, string cluster, string @namespace)
		{
			ValidateNamespaceName(tenant, cluster, @namespace);
			return Get(tenant + '/' + cluster + '/' + @namespace);
		}

		public static NamespaceName Get(string @namespace)
		{
			try
			{
                Condition.CheckNotNull(@namespace);
                if (!Cache.ContainsKey(@namespace))
                {
					Cache[@namespace] = new NamespaceName(@namespace);
                }
			}
			catch (NullReferenceException)
			{
				throw new ArgumentException("Invalid null namespace: " + @namespace);
			}
			try
			{
				return Cache[@namespace];
			}
			catch (Exception e)
			{
				throw;
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
					ValidateNamespaceName(parts[0], parts[1]);

					_tenant = parts[0];
					_cluster = null;
					_localName = parts[1];
				}
				else if (parts.Length == 3)
				{
					// Old style namespace: <tenant>/<cluster>/<namespace>
					ValidateNamespaceName(parts[0], parts[1], parts[2]);

					_tenant = parts[0];
					_cluster = parts[1];
					_localName = parts[2];
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
			this._namespace = @namespace;
		}

		public string Tenant => _tenant;

        [Obsolete]
		public string Cluster => _cluster;

        public string LocalName => _localName;

        public bool Global => string.ReferenceEquals(_cluster, null) || Constants.GlobalCluster.Equals(_cluster, StringComparison.OrdinalIgnoreCase);

        public string GetPersistentTopicName(string localTopic)
		{
			return GetTopicName(TopicDomain.Persistent, localTopic);
		}

		/// <summary>
		/// Compose the topic name from namespace + topic.
		/// </summary>
		/// <param name="domain"> </param>
		/// <param name="topic">
		/// @return </param>
		internal string GetTopicName(TopicDomain domain, string topic)
		{
			try
			{
                Condition.CheckNotNull(domain);
				NamedEntity.CheckName(topic);
				return $"{domain.ToString()}://{_namespace}/{topic}";
			}
			catch (NullReferenceException e)
			{
				throw new ArgumentException("Null pointer is invalid as domain for topic.", e);
			}
		}

		public override string ToString()
		{
			return _namespace;
		}

		public override bool Equals(object obj)
		{
			if (obj is NamespaceName)
			{
				NamespaceName other = (NamespaceName) obj;
				return object.Equals(_namespace, other._namespace);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return _namespace.GetHashCode();
		}

		public static void ValidateNamespaceName(string tenant, string @namespace)
		{
			try
			{
                Condition.CheckNotNull(tenant);
                Condition.CheckNotNull(@namespace);
				if (tenant.Length == 0 || @namespace.Length == 0)
				{
					throw new ArgumentException($"Invalid namespace format. namespace: {tenant}/{@namespace}");
				}
				NamedEntity.CheckName(tenant);
				NamedEntity.CheckName(@namespace);
			}
			catch (NullReferenceException e)
			{
				throw new ArgumentException($"Invalid namespace format. namespace: {tenant}/{@namespace}", e);
			}
		}

		public static void ValidateNamespaceName(string tenant, string cluster, string @namespace)
		{
			try
			{
				Condition.CheckNotNull(tenant);
                Condition.CheckNotNull(cluster);
                Condition.CheckNotNull(@namespace);
				if (tenant.Length == 0 || cluster.Length == 0 || @namespace.Length == 0)
				{
					throw new ArgumentException($"Invalid namespace format. namespace: {tenant}/{cluster}/{@namespace}");
				}
				NamedEntity.CheckName(tenant);
				NamedEntity.CheckName(cluster);
				NamedEntity.CheckName(@namespace);
			}
			catch (NullReferenceException e)
			{
				throw new ArgumentException($"Invalid namespace format. namespace: {tenant}/{cluster}/{@namespace}", e);
			}
		}

		public NamespaceName NamespaceObject => this;

        public bool Includes(TopicName topicName)
		{
			return this.Equals(topicName.NamespaceObject);
		}

		/// <summary>
		/// Returns true if this is a V2 namespace prop/namespace-name. </summary>
		/// <returns> true if v2 </returns>
		public bool V2 => string.ReferenceEquals(_cluster, null);
    }

}