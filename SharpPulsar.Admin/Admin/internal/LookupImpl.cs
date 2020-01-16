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
	using LookupData = org.apache.pulsar.common.lookup.data.LookupData;
	using TopicName = org.apache.pulsar.common.naming.TopicName;

	public class LookupImpl : BaseResource, Lookup
	{

		private readonly WebTarget v2lookup;
		private readonly bool useTls;

		public LookupImpl(WebTarget web, Authentication auth, bool useTls, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			this.useTls = useTls;
			v2lookup = web.path("/lookup/v2");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public String lookupTopic(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual string lookupTopic(string topic)
		{
			TopicName topicName = TopicName.get(topic);
			string prefix = topicName.V2 ? "/topic" : "/destination";
			WebTarget target = v2lookup.path(prefix).path(topicName.LookupName);

			try
			{
				return doTopicLookup(target);
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public String getBundleRange(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual string getBundleRange(string topic)
		{
			TopicName topicName = TopicName.get(topic);
			string prefix = topicName.V2 ? "/topic" : "/destination";
			WebTarget target = v2lookup.path(prefix).path(topicName.LookupName).path("bundle");

			try
			{
				return request(target).get(typeof(string));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String doTopicLookup(javax.ws.rs.client.WebTarget lookupResource) throws org.apache.pulsar.client.admin.PulsarAdminException
		private string doTopicLookup(WebTarget lookupResource)
		{
			LookupData lookupData = request(lookupResource).get(typeof(LookupData));
			if (useTls)
			{
				return lookupData.BrokerUrlTls;
			}
			else
			{
				return lookupData.BrokerUrl;
			}
		}

	}

}