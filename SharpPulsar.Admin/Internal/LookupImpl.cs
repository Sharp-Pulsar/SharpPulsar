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
	using LookupData = Org.Apache.Pulsar.Common.Lookup.Data.LookupData;
	using TopicName = Org.Apache.Pulsar.Common.Naming.TopicName;

	public class LookupImpl : BaseResource, Lookup
	{

		private readonly WebTarget v2lookup;
		private readonly bool useTls;

		public LookupImpl(WebTarget Web, Authentication Auth, bool UseTls, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
			this.useTls = UseTls;
			v2lookup = Web.path("/lookup/v2");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public String lookupTopic(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override string LookupTopic(string Topic)
		{
			TopicName TopicName = TopicName.get(Topic);
			string Prefix = TopicName.V2 ? "/topic" : "/destination";
			WebTarget Target = v2lookup.path(Prefix).path(TopicName.LookupName);

			try
			{
				return DoTopicLookup(Target);
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public String getBundleRange(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override string GetBundleRange(string Topic)
		{
			TopicName TopicName = TopicName.get(Topic);
			string Prefix = TopicName.V2 ? "/topic" : "/destination";
			WebTarget Target = v2lookup.path(Prefix).path(TopicName.LookupName).path("bundle");

			try
			{
				return Request(Target).get(typeof(string));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String doTopicLookup(javax.ws.rs.client.WebTarget lookupResource) throws org.apache.pulsar.client.admin.PulsarAdminException
		private string DoTopicLookup(WebTarget LookupResource)
		{
			LookupData LookupData = Request(LookupResource).get(typeof(LookupData));
			if (useTls)
			{
				return LookupData.BrokerUrlTls;
			}
			else
			{
				return LookupData.BrokerUrl;
			}
		}

	}

}