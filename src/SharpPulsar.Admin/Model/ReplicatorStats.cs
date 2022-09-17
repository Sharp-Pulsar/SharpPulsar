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
namespace Org.Apache.Pulsar.Common.Policies.Data
{
	/// <summary>
	/// Statistics about a replicator.
	/// </summary>
	public interface ReplicatorStats
	{

		/// <summary>
		/// Total rate of messages received from the remote cluster (msg/s). </summary>
		double MsgRateIn {get;}

		/// <summary>
		/// Total throughput received from the remote cluster (bytes/s). </summary>
		double MsgThroughputIn {get;}

		/// <summary>
		/// Total rate of messages delivered to the replication-subscriber (msg/s). </summary>
		double MsgRateOut {get;}

		/// <summary>
		/// Total throughput delivered to the replication-subscriber (bytes/s). </summary>
		double MsgThroughputOut {get;}

		/// <summary>
		/// Total rate of messages expired (msg/s). </summary>
		double MsgRateExpired {get;}

		/// <summary>
		/// Number of messages pending to be replicated to remote cluster. </summary>
		long ReplicationBacklog {get;}

		/// <summary>
		/// is the replication-subscriber up and running to replicate to remote cluster. </summary>
		bool Connected {get;}

		/// <summary>
		/// Time in seconds from the time a message was produced to the time when it is about to be replicated. </summary>
		long ReplicationDelayInSeconds {get;}

		/// <summary>
		/// Address of incoming replication connection. </summary>
		string InboundConnection {get;}

		/// <summary>
		/// Timestamp of incoming connection establishment time. </summary>
		string InboundConnectedSince {get;}

		/// <summary>
		/// Address of outbound replication connection. </summary>
		string OutboundConnection {get;}

		/// <summary>
		/// Timestamp of outbound connection establishment time. </summary>
		string OutboundConnectedSince {get;}
	}

}