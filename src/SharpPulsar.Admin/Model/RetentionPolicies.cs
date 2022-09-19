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
	/// <summary>
	/// Definition of the retention policy.
	/// 
	/// <para>When you set a retention policy you must set **both** a *size limit* and a *time limit*.
	/// In the case where you don't want to limit by either time or size, the value must be set to `-1`.
	/// Retention policy will be effectively disabled and it won't prevent the deletion of acknowledged
	/// messages when either size or time limit is set to `0`.
	/// Infinite retention can be achieved by setting both time and size limits to `-1`.
	/// </para>
	/// </summary>
	public class RetentionPolicies
	{
		private int retentionTimeInMinutes;
		private long retentionSizeInMB;

		public RetentionPolicies() : this(0, 0)
		{
		}

		public RetentionPolicies(int RetentionTimeInMinutes, int RetentionSizeInMB)
		{
			this.retentionSizeInMB = RetentionSizeInMB;
			this.retentionTimeInMinutes = RetentionTimeInMinutes;
		}

		public virtual int RetentionTimeInMinutes
		{
			get
			{
				return retentionTimeInMinutes;
			}
		}

		public virtual long RetentionSizeInMB
		{
			get
			{
				return retentionSizeInMB;
			}
		}

		public override bool Equals(object O)
		{
			if (this == O)
			{
				return true;
			}
			if (O == null || this.GetType() != O.GetType())
			{
				return false;
			}
			RetentionPolicies That = (RetentionPolicies) O;

			if (retentionTimeInMinutes != That.retentionTimeInMinutes)
			{
				return false;
			}

			return retentionSizeInMB == That.retentionSizeInMB;
		}

		public override int GetHashCode()
		{
			long Result = retentionTimeInMinutes;
			Result = 31 * Result + retentionSizeInMB;
			return Long.hashCode(Result);
		}

		public override string ToString()
		{
			return "RetentionPolicies{" + "retentionTimeInMinutes=" + retentionTimeInMinutes + ", retentionSizeInMB=" + retentionSizeInMB + '}';
		}
	}

}