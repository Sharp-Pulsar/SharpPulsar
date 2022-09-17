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
namespace Org.Apache.Pulsar.Common.Policies.Data
{
	using ProducerAccessMode = org.apache.pulsar.client.api.ProducerAccessMode;

	/// <summary>
	/// Statistics about a publisher.
	/// </summary>
	public interface PublisherStats
	{

		ProducerAccessMode AccessMode {get;}

		/// <summary>
		/// Total rate of messages published by this publisher (msg/s). </summary>
		double MsgRateIn {get;}

		/// <summary>
		/// Total throughput of messages published by this publisher (byte/s). </summary>
		double MsgThroughputIn {get;}

		/// <summary>
		/// Average message size published by this publisher. </summary>
		double AverageMsgSize {get;}

		/// <summary>
		/// The total rate of chunked messages published by this publisher. * </summary>
		double ChunkedMessageRate {get;}

		/// <summary>
		/// Id of this publisher. </summary>
		long ProducerId {get;}

		/// <summary>
		/// Whether partial producer is supported at client. </summary>
		bool SupportsPartialProducer {get;}

		/// <summary>
		/// Producer name. </summary>
		string ProducerName {get;}

		/// <summary>
		/// Address of this publisher. </summary>
		string Address {get;}

		/// <summary>
		/// Timestamp of connection. </summary>
		string ConnectedSince {get;}

		/// <summary>
		/// Client library version. </summary>
		string ClientVersion {get;}

		/// <summary>
		/// Metadata (key/value strings) associated with this publisher. </summary>
		IDictionary<string, string> Metadata {get;}
	}

}