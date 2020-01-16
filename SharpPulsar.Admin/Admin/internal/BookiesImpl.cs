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
namespace org.apache.pulsar.client.admin.@internal
{

	using Authentication = org.apache.pulsar.client.api.Authentication;
	using BookieInfo = org.apache.pulsar.common.policies.data.BookieInfo;
	using BookiesRackConfiguration = org.apache.pulsar.common.policies.data.BookiesRackConfiguration;
	using ErrorData = org.apache.pulsar.common.policies.data.ErrorData;

	public class BookiesImpl : BaseResource, Bookies
	{
		private readonly WebTarget adminBookies;

		public BookiesImpl(WebTarget web, Authentication auth, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			adminBookies = web.path("/admin/v2/bookies");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.BookiesRackConfiguration getBookiesRackInfo() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual BookiesRackConfiguration BookiesRackInfo
		{
			get
			{
				try
				{
					return request(adminBookies.path("racks-info")).get(typeof(BookiesRackConfiguration));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.BookieInfo getBookieRackInfo(String bookieAddress) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual BookieInfo getBookieRackInfo(string bookieAddress)
		{
			try
			{
				return request(adminBookies.path("racks-info").path(bookieAddress)).get(typeof(BookieInfo));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteBookieRackInfo(String bookieAddress) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteBookieRackInfo(string bookieAddress)
		{
			try
			{
				request(adminBookies.path("racks-info").path(bookieAddress)).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateBookieRackInfo(String bookieAddress, String group, org.apache.pulsar.common.policies.data.BookieInfo bookieInfo) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateBookieRackInfo(string bookieAddress, string group, BookieInfo bookieInfo)
		{
			try
			{
				request(adminBookies.path("racks-info").path(bookieAddress).queryParam("group", group)).post(Entity.entity(bookieInfo, MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}
	}

}