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
namespace Org.Apache.Pulsar.Common.Policies.Data
{
	using Data = lombok.Data;

	/// <summary>
	/// Source status.
	/// </summary>
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @Data public class SourceStatus
	public class SourceStatus
	{
		// The total number of source instances that ought to be running
		public int NumInstances;
		// The number of source instances that are actually running
		public int NumRunning;
		public IList<SourceInstanceStatus> Instances = new LinkedList<SourceInstanceStatus>();

		/// <summary>
		/// Source instance status.
		/// </summary>
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @Data public static class SourceInstanceStatus
		public class SourceInstanceStatus
		{
			public int InstanceId;
			public SourceInstanceStatusData Status;

			/// <summary>
			/// Source instance status data.
			/// </summary>
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @Data public static class SourceInstanceStatusData
			public class SourceInstanceStatusData
			{
				// Is this instance running?
				public bool Running;

				// Do we have any error while running this instance
				public string Error;

				// Number of times this instance has restarted
				public long NumRestarts;

				// Number of messages received from source
				public long NumReceivedFromSource;

				// Number of times there was a system exception handling messages
				public long NumSystemExceptions;

				// A list of the most recent system exceptions
				public IList<ExceptionInformation> LatestSystemExceptions;

				// Number of times there was a exception from source while reading messages
				public long NumSourceExceptions;

				// A list of the most recent source exceptions
				public IList<ExceptionInformation> LatestSourceExceptions;

				// Number of messages written into pulsar
				public long NumWritten;

				// When was the last time we received a message from the source
				public long LastReceivedTime;

				// The worker id on which the source is running
				public string WorkerId;
			}
		}

		public virtual void AddInstance(SourceInstanceStatus SourceInstanceStatus)
		{
			Instances.Add(SourceInstanceStatus);
		}

	}

}