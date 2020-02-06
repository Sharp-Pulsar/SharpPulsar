using System;
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
namespace org.apache.pulsar.client.admin.@internal
{


	using Authentication = client.api.Authentication;
	using InternalConfigurationData = pulsar.common.conf.InternalConfigurationData;
	using ErrorData = pulsar.common.policies.data.ErrorData;
	using NamespaceOwnershipStatus = pulsar.common.policies.data.NamespaceOwnershipStatus;
	using Codec = pulsar.common.util.Codec;

	public class BrokersImpl : BaseResource, Brokers
	{
		private readonly WebTarget adminBrokers;

		public BrokersImpl(WebTarget web, Authentication auth, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			adminBrokers = web.path("/admin/v2/brokers");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getActiveBrokers(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> getActiveBrokers(string cluster)
		{
			try
			{
				return request(adminBrokers.path(cluster)).get(new GenericTypeAnonymousInnerClass(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass : GenericType<IList<string>>
		{
			private readonly BrokersImpl outerInstance;

			public GenericTypeAnonymousInnerClass(BrokersImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, org.apache.pulsar.common.policies.data.NamespaceOwnershipStatus> getOwnedNamespaces(String cluster, String brokerUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IDictionary<string, NamespaceOwnershipStatus> getOwnedNamespaces(string cluster, string brokerUrl)
		{
			try
			{
				return request(adminBrokers.path(cluster).path(brokerUrl).path("ownedNamespaces")).get(new GenericTypeAnonymousInnerClass2(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass2 : GenericType<IDictionary<string, NamespaceOwnershipStatus>>
		{
			private readonly BrokersImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(BrokersImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateDynamicConfiguration(String configName, String configValue) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateDynamicConfiguration(string configName, string configValue)
		{
			try
			{
				string value = Codec.encode(configValue);
				request(adminBrokers.path("/configuration/").path(configName).path(value)).post(Entity.json(""), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteDynamicConfiguration(String configName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteDynamicConfiguration(string configName)
		{
			try
			{
				request(adminBrokers.path("/configuration/").path(configName)).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, String> getAllDynamicConfigurations() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IDictionary<string, string> AllDynamicConfigurations
		{
			get
			{
				try
				{
					return request(adminBrokers.path("/configuration/").path("values")).get(new GenericTypeAnonymousInnerClass3(this));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

		private class GenericTypeAnonymousInnerClass3 : GenericType<IDictionary<string, string>>
		{
			private readonly BrokersImpl outerInstance;

			public GenericTypeAnonymousInnerClass3(BrokersImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getDynamicConfigurationNames() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> DynamicConfigurationNames
		{
			get
			{
				try
				{
					return request(adminBrokers.path("/configuration")).get(new GenericTypeAnonymousInnerClass4(this));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

		private class GenericTypeAnonymousInnerClass4 : GenericType<IList<string>>
		{
			private readonly BrokersImpl outerInstance;

			public GenericTypeAnonymousInnerClass4(BrokersImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, String> getRuntimeConfigurations() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IDictionary<string, string> RuntimeConfigurations
		{
			get
			{
				try
				{
					return request(adminBrokers.path("/configuration").path("runtime")).get(new GenericTypeAnonymousInnerClass5(this));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

		private class GenericTypeAnonymousInnerClass5 : GenericType<IDictionary<string, string>>
		{
			private readonly BrokersImpl outerInstance;

			public GenericTypeAnonymousInnerClass5(BrokersImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.conf.InternalConfigurationData getInternalConfigurationData() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual InternalConfigurationData InternalConfigurationData
		{
			get
			{
				try
				{
					return request(adminBrokers.path("/internal-configuration")).get(typeof(InternalConfigurationData));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void healthcheck() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void healthcheck()
		{
			try
			{
				string result = request(adminBrokers.path("/health")).get(typeof(string));
				if (!result.Trim().ToLower().Equals("ok"))
				{
					throw new PulsarAdminException("Healthcheck returned unexpected result: " + result);
				}
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}
	}

}