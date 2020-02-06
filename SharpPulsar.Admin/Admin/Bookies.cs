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
namespace org.apache.pulsar.client.admin
{
	using BookieInfo = pulsar.common.policies.data.BookieInfo;
	using BookiesRackConfiguration = pulsar.common.policies.data.BookiesRackConfiguration;

	/// <summary>
	/// Admin interface for bookies rack placement management.
	/// </summary>
	public interface Bookies
	{

		/// <summary>
		/// Gets the rack placement information for all the bookies in the cluster
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.BookiesRackConfiguration getBookiesRackInfo() throws PulsarAdminException;
		BookiesRackConfiguration BookiesRackInfo {get;}

		/// <summary>
		/// Gets the rack placement information for a specific bookie in the cluster
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.BookieInfo getBookieRackInfo(String bookieAddress) throws PulsarAdminException;
		BookieInfo getBookieRackInfo(string bookieAddress);

		/// <summary>
		/// Remove rack placement information for a specific bookie in the cluster
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteBookieRackInfo(String bookieAddress) throws PulsarAdminException;
		void deleteBookieRackInfo(string bookieAddress);

		/// <summary>
		/// Updates the rack placement information for a specific bookie in the cluster
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateBookieRackInfo(String bookieAddress, String group, org.apache.pulsar.common.policies.data.BookieInfo bookieInfo) throws PulsarAdminException;
		void updateBookieRackInfo(string bookieAddress, string group, BookieInfo bookieInfo);
	}

}