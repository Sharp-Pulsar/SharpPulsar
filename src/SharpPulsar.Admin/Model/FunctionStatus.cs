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
namespace SharpPulsar.Admin.Model
{
	
	public class FunctionStatus
	{

		public int NumInstances;
		public int NumRunning;
		public IList<FunctionInstanceStatus> Instances = new LinkedList<FunctionInstanceStatus>();

		/// <summary>
		/// Function instance status.
		/// </summary>
		public class FunctionInstanceStatus
		{
			public int InstanceId;
			public FunctionInstanceStatusData Status;

			/// <summary>
			/// Function instance status data.
			/// </summary>
			public class FunctionInstanceStatusData
			{

				public bool Running;

				public string Error;

				public long NumRestarts;

				public long NumReceived;

				public long NumSuccessfullyProcessed;

				public long NumUserExceptions;

				public IList<ExceptionInformation> LatestUserExceptions;

				public long NumSystemExceptions;

				public IList<ExceptionInformation> LatestSystemExceptions;

				public double AverageLatency;

				public long LastInvocationTime;

				public string WorkerId;
			}

		}

		public virtual void AddInstance(FunctionInstanceStatus FunctionInstanceStatus)
		{
			Instances.Add(FunctionInstanceStatus);
		}

	}

}