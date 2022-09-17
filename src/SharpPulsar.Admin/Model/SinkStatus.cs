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
	using Data = lombok.Data;

	/// <summary>
	/// Status of Pulsar Sink.
	/// </summary>
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @Data public class SinkStatus
	public class SinkStatus
	{
		// The total number of sink instances that ought to be running
		public int NumInstances;
		// The number of source instances that are actually running
		public int NumRunning;
		public IList<SinkInstanceStatus> Instances = new LinkedList<SinkInstanceStatus>();

		/// <summary>
		/// Status of a Sink instance.
		/// </summary>
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @Data public static class SinkInstanceStatus
		public class SinkInstanceStatus
		{
			public int InstanceId;
			public SinkInstanceStatusData Status;

			/// <summary>
			/// Status data of a Sink instance.
			/// </summary>
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @Data public static class SinkInstanceStatusData
			public class SinkInstanceStatusData
			{
				// Is this instance running?
				public bool Running;

				// Do we have any error while running this instance
				public string Error;

				// Number of times this instance has restarted
				public long NumRestarts;

				// Number of messages read from Pulsar
				public long NumReadFromPulsar;

				// Number of times there was a system exception handling messages
				public long NumSystemExceptions;

				// A list of the most recent system exceptions
				public IList<ExceptionInformation> LatestSystemExceptions;

				// Number of times there was a sink exception
				public long NumSinkExceptions;

				// A list of the most recent sink exceptions
				public IList<ExceptionInformation> LatestSinkExceptions;

				// Number of messages written to sink
				public long NumWrittenToSink;

				// When was the last time we received a message from Pulsar
				public long LastReceivedTime;

				public string WorkerId;
			}
		}

		public virtual void AddInstance(SinkInstanceStatus SinkInstanceStatus)
		{
			Instances.Add(SinkInstanceStatus);
		}

	}

}