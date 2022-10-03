using System.Threading.Tasks;
using SharpPulsar.Admin.Model;
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
	/// Admin interface for bookies rack placement management.
	/// </summary>
	public interface IBookies
	{

		/// <summary>
		/// Gets the rack placement information for all the bookies in the cluster.
		/// </summary>
        BookiesRackConfiguration BookiesRackInfo {get;}

		/// <summary>
		/// Gets the rack placement information for all the bookies in the cluster asynchronously.
		/// </summary>
		ValueTask<BookiesRackConfiguration> BookiesRackInfoAsync {get;}

		/// <summary>
		/// Gets discovery information for all the bookies in the cluster.
		/// </summary>
		BookiesClusterInfo Bookies {get;}

		/// <summary>
		/// Gets discovery information for all the bookies in the cluster asynchronously.
		/// </summary>
		ValueTask<BookiesClusterInfo> BookiesAsync {get;}

		/// <summary>
		/// Gets the rack placement information for a specific bookie in the cluster.
		/// </summary>
		BookieInfo GetBookieRackInfo(string bookieAddress);

		/// <summary>
		/// Gets the rack placement information for a specific bookie in the cluster asynchronously.
		/// </summary>
		ValueTask<BookieInfo> GetBookieRackInfoAsync(string bookieAddress);

		/// <summary>
		/// Remove rack placement information for a specific bookie in the cluster.
		/// </summary>
		void DeleteBookieRackInfo(string bookieAddress);

		/// <summary>
		/// Remove rack placement information for a specific bookie in the cluster asynchronously.
		/// </summary>
		ValueTask DeleteBookieRackInfoAsync(string bookieAddress);

		/// <summary>
		/// Updates the rack placement information for a specific bookie in the cluster.
		/// </summary>
		void UpdateBookieRackInfo(string bookieAddress, string group, BookieInfo bookieInfo);

		/// <summary>
		/// Updates the rack placement information for a specific bookie in the cluster asynchronously.
		/// </summary>
		ValueTask UpdateBookieRackInfoAsync(string bookieAddress, string group, BookieInfo bookieInfo);
	}

}