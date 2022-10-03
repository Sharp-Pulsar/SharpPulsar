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
    /// Status code.
    /// </summary>
    public enum Status
    {
        NotRun,
        RUNNING,
        SUCCESS,
        ERROR
    }
    /// <summary>
    /// Status of long running process.
    /// </summary>
    public class LongRunningProcessStatus
	{

		public Status Status;
		public string LastError;

		public LongRunningProcessStatus()
		{
			this.Status = Status.NotRun;
			this.LastError = "";
		}

		internal LongRunningProcessStatus(Status status, string lastError)
		{
			this.Status = status;
			this.LastError = lastError;
		}

		public static LongRunningProcessStatus ForStatus(Status status)
		{
			return new LongRunningProcessStatus(status, "");
		}

		public static LongRunningProcessStatus ForError(string lastError)
		{
			return new LongRunningProcessStatus(Status.ERROR, lastError);
		}
	}

}