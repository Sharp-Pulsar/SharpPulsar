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
	using ToString = lombok.ToString;

	/// <summary>
	/// Configuration of bookkeeper persistence policies.
	/// </summary>
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @ToString public class PersistencePolicies
	public class PersistencePolicies
	{
		private int bookkeeperEnsemble;
		private int bookkeeperWriteQuorum;
		private int bookkeeperAckQuorum;
		private double managedLedgerMaxMarkDeleteRate;

		public PersistencePolicies() : this(2, 2, 2, 0.0)
		{
		}

		public PersistencePolicies(int BookkeeperEnsemble, int BookkeeperWriteQuorum, int BookkeeperAckQuorum, double ManagedLedgerMaxMarkDeleteRate)
		{
			this.bookkeeperEnsemble = BookkeeperEnsemble;
			this.bookkeeperWriteQuorum = BookkeeperWriteQuorum;
			this.bookkeeperAckQuorum = BookkeeperAckQuorum;
			this.managedLedgerMaxMarkDeleteRate = ManagedLedgerMaxMarkDeleteRate;
		}

		public virtual int BookkeeperEnsemble
		{
			get
			{
				return bookkeeperEnsemble;
			}
		}

		public virtual int BookkeeperWriteQuorum
		{
			get
			{
				return bookkeeperWriteQuorum;
			}
		}

		public virtual int BookkeeperAckQuorum
		{
			get
			{
				return bookkeeperAckQuorum;
			}
		}

		public virtual double ManagedLedgerMaxMarkDeleteRate
		{
			get
			{
				return managedLedgerMaxMarkDeleteRate;
			}
		}

		public override int GetHashCode()
		{
			return Objects.hash(bookkeeperEnsemble, bookkeeperWriteQuorum, bookkeeperAckQuorum, managedLedgerMaxMarkDeleteRate);
		}
		public override bool Equals(object Obj)
		{
			if (Obj is PersistencePolicies)
			{
				PersistencePolicies Other = (PersistencePolicies) Obj;
				return bookkeeperEnsemble == Other.bookkeeperEnsemble && bookkeeperWriteQuorum == Other.bookkeeperWriteQuorum && bookkeeperAckQuorum == Other.bookkeeperAckQuorum && managedLedgerMaxMarkDeleteRate == Other.managedLedgerMaxMarkDeleteRate;
			}

			return false;
		}
	}

}