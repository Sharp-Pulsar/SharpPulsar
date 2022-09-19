﻿using System.Collections.Generic;

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
namespace SharpPulsar.Admin.Model
{
	using PartitionedTopicMetadata = org.apache.pulsar.common.partition.PartitionedTopicMetadata;

	/// <summary>
	/// Statistics for a partitioned topic.
	/// </summary>
	public interface PartitionedTopicStats : TopicStats
	{

		PartitionedTopicMetadata Metadata {get;}

// JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
// ORIGINAL LINE: java.util.Map<String, ? extends TopicStats> getPartitions();
		IDictionary<string, TopicStats> Partitions {get;}

		TopicStats Add(TopicStats ts);
	}
}