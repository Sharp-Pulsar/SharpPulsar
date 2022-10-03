using System.Collections.Generic;
using System.Threading.Tasks;

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
namespace SharpPulsar.Admin.Interfaces
{

    /// <summary>
    /// This is an interface class to allow using command line tool to quickly lookup the broker serving the topic.
    /// </summary>
    public interface Lookup
	{

		/// <summary>
		/// Lookup a topic.
		/// </summary>
		/// <param name="topic"> </param>
		/// <returns> the broker URL that serves the topic </returns>
		string LookupTopic(string topic);

		/// <summary>
		/// Lookup a topic asynchronously.
		/// </summary>
		/// <param name="topic"> </param>
		/// <returns> the broker URL that serves the topic </returns>
		ValueTask<string> LookupTopicAsync(string topic);

		/// <summary>
		/// Lookup a partitioned topic.
		/// </summary>
		/// <param name="topic"> </param>
		/// <returns> the broker URLs that serves the topic </returns>
		IDictionary<string, string> LookupPartitionedTopic(string topic);


		/// <summary>
		/// Lookup a partitioned topic.
		/// </summary>
		/// <param name="topic"> </param>
		/// <returns> the broker URLs that serves the topic </returns>
		ValueTask<IDictionary<string, string>> LookupPartitionedTopicAsync(string topic);

		/// <summary>
		/// Get a bundle range of a topic.
		/// </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		string GetBundleRange(string topic);

		/// <summary>
		/// Get a bundle range of a topic asynchronously.
		/// </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask<string> GetBundleRangeAsync(string topic);
	}

}