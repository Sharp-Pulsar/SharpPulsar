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
namespace Org.Apache.Pulsar.Client.Admin.@internal
{


	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using InternalConfigurationData = Org.Apache.Pulsar.Common.Conf.InternalConfigurationData;
	using ErrorData = Org.Apache.Pulsar.Common.Policies.Data.ErrorData;
	using NamespaceOwnershipStatus = Org.Apache.Pulsar.Common.Policies.Data.NamespaceOwnershipStatus;
	using Codec = Org.Apache.Pulsar.Common.Util.Codec;

	public class BrokersImpl : BaseResource, Brokers
	{
		private readonly WebTarget adminBrokers;

		public BrokersImpl(WebTarget Web, Authentication Auth, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
			adminBrokers = Web.path("/admin/v2/brokers");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getActiveBrokers(String cluster) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> GetActiveBrokers(string Cluster)
		{
			try
			{
				return Request(adminBrokers.path(Cluster)).get(new GenericTypeAnonymousInnerClass(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass : GenericType<IList<string>>
		{
			private readonly BrokersImpl outerInstance;

			public GenericTypeAnonymousInnerClass(BrokersImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, org.apache.pulsar.common.policies.data.NamespaceOwnershipStatus> getOwnedNamespaces(String cluster, String brokerUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IDictionary<string, NamespaceOwnershipStatus> GetOwnedNamespaces(string Cluster, string BrokerUrl)
		{
			try
			{
				return Request(adminBrokers.path(Cluster).path(BrokerUrl).path("ownedNamespaces")).get(new GenericTypeAnonymousInnerClass2(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass2 : GenericType<IDictionary<string, NamespaceOwnershipStatus>>
		{
			private readonly BrokersImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(BrokersImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateDynamicConfiguration(String configName, String configValue) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateDynamicConfiguration(string ConfigName, string ConfigValue)
		{
			try
			{
				string Value = Codec.encode(ConfigValue);
				Request(adminBrokers.path("/configuration/").path(ConfigName).path(Value)).post(Entity.json(""), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteDynamicConfiguration(String configName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteDynamicConfiguration(string ConfigName)
		{
			try
			{
				Request(adminBrokers.path("/configuration/").path(ConfigName)).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
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
					return Request(adminBrokers.path("/configuration/").path("values")).get(new GenericTypeAnonymousInnerClass3(this));
				}
				catch (Exception E)
				{
					throw GetApiException(E);
				}
			}
		}

		public class GenericTypeAnonymousInnerClass3 : GenericType<IDictionary<string, string>>
		{
			private readonly BrokersImpl outerInstance;

			public GenericTypeAnonymousInnerClass3(BrokersImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
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
					return Request(adminBrokers.path("/configuration")).get(new GenericTypeAnonymousInnerClass4(this));
				}
				catch (Exception E)
				{
					throw GetApiException(E);
				}
			}
		}

		public class GenericTypeAnonymousInnerClass4 : GenericType<IList<string>>
		{
			private readonly BrokersImpl outerInstance;

			public GenericTypeAnonymousInnerClass4(BrokersImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
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
					return Request(adminBrokers.path("/configuration").path("runtime")).get(new GenericTypeAnonymousInnerClass5(this));
				}
				catch (Exception E)
				{
					throw GetApiException(E);
				}
			}
		}

		public class GenericTypeAnonymousInnerClass5 : GenericType<IDictionary<string, string>>
		{
			private readonly BrokersImpl outerInstance;

			public GenericTypeAnonymousInnerClass5(BrokersImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
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
					return Request(adminBrokers.path("/internal-configuration")).get(typeof(InternalConfigurationData));
				}
				catch (Exception E)
				{
					throw GetApiException(E);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void healthcheck() throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void Healthcheck()
		{
			try
			{
				string Result = Request(adminBrokers.path("/health")).get(typeof(string));
				if (!Result.Trim().ToLower().Equals("ok"))
				{
					throw new PulsarAdminException("Healthcheck returned unexpected result: " + Result);
				}
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}
	}

}