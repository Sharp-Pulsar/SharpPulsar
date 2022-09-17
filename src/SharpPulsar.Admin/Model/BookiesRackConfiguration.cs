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

	/// <summary>
	/// The rack configuration map for bookies.
	/// </summary>
	public class BookiesRackConfiguration : SortedDictionary<string, IDictionary<string, BookieInfo>>
	{

		private const long SerialVersionUID = 0L;

		public virtual bool RemoveBookie(string Address)
		{
			lock (this)
			{
				foreach (KeyValuePair<string, IDictionary<string, BookieInfo>> Entry in this.SetOfKeyValuePairs())
				{
					if (Entry.Value.Remove(Address) != null)
					{
						if (Entry.Value.IsEmpty())
						{
							this.Remove(Entry.Key);
						}
						return true;
					}
				}
				return false;
			}
		}

		public virtual Optional<BookieInfo> GetBookie(string Address)
		{
			lock (this)
			{
				foreach (IDictionary<string, BookieInfo> M in this.Values)
				{
					BookieInfo Bi = M[Address];
					if (Bi != null)
					{
						return Bi;
					}
				}
				return null;
			}
		}

		public virtual void UpdateBookie(string Group, string Address, BookieInfo BookieInfo)
		{
			lock (this)
			{
				Objects.requireNonNull(Group);
				Objects.requireNonNull(Address);
				Objects.requireNonNull(BookieInfo);
        
				// Remove from any group first
				RemoveBookie(Address);
				computeIfAbsent(Group, key => new SortedDictionary<>()).put(Address, BookieInfo);
			}
		}
	}

}