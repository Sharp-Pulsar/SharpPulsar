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
namespace org.apache.pulsar.common.util
{

	using NamespaceBundleStats = org.apache.pulsar.policies.data.loadbalancer.NamespaceBundleStats;
	using ResourceType = org.apache.pulsar.policies.data.loadbalancer.SystemResourceUsage.ResourceType;

	public class NamespaceBundleStatsComparator : IComparer<string>
	{
		internal IDictionary<string, NamespaceBundleStats> map;
		internal ResourceType resType;

		public NamespaceBundleStatsComparator(IDictionary<string, NamespaceBundleStats> map, ResourceType resType)
		{
			this.map = map;
			this.resType = resType;
		}

		// sort in reverse order, maximum loaded should be on top
		public virtual int Compare(string a, string b)
		{
			int result = 0;
			if (this.resType == ResourceType.CPU)
			{
				result = map[a].compareByMsgRate(map[b]);
			}
			else if (this.resType == ResourceType.Memory)
			{
				result = map[a].compareByTopicConnections(map[b]);
			}
			else if (this.resType == ResourceType.BandwidthIn)
			{
				result = map[a].compareByBandwidthIn(map[b]);
			}
			else if (this.resType == ResourceType.BandwidthOut)
			{
				result = map[a].compareByBandwidthOut(map[b]);
			}
			else
			{
				result = map[a].CompareTo(map[b]);
			}

			if (result > 0)
			{
				return -1;
			}
			else
			{
				return 1;
			}
		}
	}

}