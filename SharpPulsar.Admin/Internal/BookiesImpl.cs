using System;

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
namespace Org.Apache.Pulsar.Client.Admin.@internal
{

	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using BookieInfo = Org.Apache.Pulsar.Common.Policies.Data.BookieInfo;
	using BookiesRackConfiguration = Org.Apache.Pulsar.Common.Policies.Data.BookiesRackConfiguration;
	using ErrorData = Org.Apache.Pulsar.Common.Policies.Data.ErrorData;

	public class BookiesImpl : BaseResource, Bookies
	{
		private readonly WebTarget adminBookies;

		public BookiesImpl(WebTarget Web, Authentication Auth, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
			adminBookies = Web.path("/admin/v2/bookies");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.BookiesRackConfiguration getBookiesRackInfo() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual BookiesRackConfiguration BookiesRackInfo
		{
			get
			{
				try
				{
					return Request(adminBookies.path("racks-info")).get(typeof(BookiesRackConfiguration));
				}
				catch (Exception E)
				{
					throw GetApiException(E);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.BookieInfo getBookieRackInfo(String bookieAddress) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override BookieInfo GetBookieRackInfo(string BookieAddress)
		{
			try
			{
				return Request(adminBookies.path("racks-info").path(BookieAddress)).get(typeof(BookieInfo));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteBookieRackInfo(String bookieAddress) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteBookieRackInfo(string BookieAddress)
		{
			try
			{
				Request(adminBookies.path("racks-info").path(BookieAddress)).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateBookieRackInfo(String bookieAddress, String group, org.apache.pulsar.common.policies.data.BookieInfo bookieInfo) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateBookieRackInfo(string BookieAddress, string Group, BookieInfo BookieInfo)
		{
			try
			{
				Request(adminBookies.path("racks-info").path(BookieAddress).queryParam("group", Group)).post(Entity.entity(BookieInfo, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}
	}

}