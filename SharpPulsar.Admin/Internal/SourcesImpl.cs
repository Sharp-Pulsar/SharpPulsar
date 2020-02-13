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
	using Gson = com.google.gson.Gson;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using UpdateOptions = Org.Apache.Pulsar.Common.Functions.UpdateOptions;
	using ConnectorDefinition = Org.Apache.Pulsar.Common.Io.ConnectorDefinition;
	using ErrorData = Org.Apache.Pulsar.Common.Policies.Data.ErrorData;
	using SourceConfig = Org.Apache.Pulsar.Common.Io.SourceConfig;
	using SourceStatus = Org.Apache.Pulsar.Common.Policies.Data.SourceStatus;
	using ObjectMapperFactory = Org.Apache.Pulsar.Common.Util.ObjectMapperFactory;
	using AsyncHttpClient = org.asynchttpclient.AsyncHttpClient;
	using RequestBuilder = org.asynchttpclient.RequestBuilder;
	using FilePart = org.asynchttpclient.request.body.multipart.FilePart;
	using StringPart = org.asynchttpclient.request.body.multipart.StringPart;
	using FormDataBodyPart = org.glassfish.jersey.media.multipart.FormDataBodyPart;
	using FormDataMultiPart = org.glassfish.jersey.media.multipart.FormDataMultiPart;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.asynchttpclient.Dsl.post;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.asynchttpclient.Dsl.put;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class SourcesImpl extends ComponentResource implements org.apache.pulsar.client.admin.Sources, org.apache.pulsar.client.admin.Source
	public class SourcesImpl : ComponentResource, Sources, Source
	{

		private readonly WebTarget source;
		private readonly AsyncHttpClient asyncHttpClient;

		public SourcesImpl(WebTarget Web, Authentication Auth, AsyncHttpClient AsyncHttpClient, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
			this.source = Web.path("/admin/v3/source");
			this.asyncHttpClient = AsyncHttpClient;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> listSources(String tenant, String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> ListSources(string Tenant, string Namespace)
		{
			try
			{
				Response Response = Request(source.path(Tenant).path(Namespace)).get();
				if (!Response.StatusInfo.Equals(Response.Status.OK))
				{
					throw GetApiException(Response);
				}
				return Response.readEntity(new GenericTypeAnonymousInnerClass(this));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class GenericTypeAnonymousInnerClass : GenericType<IList<string>>
		{
			private readonly SourcesImpl outerInstance;

			public GenericTypeAnonymousInnerClass(SourcesImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.io.SourceConfig getSource(String tenant, String namespace, String sourceName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override SourceConfig GetSource(string Tenant, string Namespace, string SourceName)
		{
			try
			{
				 Response Response = Request(source.path(Tenant).path(Namespace).path(SourceName)).get();
				if (!Response.StatusInfo.Equals(Response.Status.OK))
				{
					throw GetApiException(Response);
				}
				return Response.readEntity(typeof(SourceConfig));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SourceStatus getSourceStatus(String tenant, String namespace, String sourceName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override SourceStatus GetSourceStatus(string Tenant, string Namespace, string SourceName)
		{
			try
			{
				Response Response = Request(source.path(Tenant).path(Namespace).path(SourceName).path("status")).get();
				if (!Response.StatusInfo.Equals(Response.Status.OK))
				{
					throw GetApiException(Response);
				}
				return Response.readEntity(typeof(SourceStatus));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SourceStatus.SourceInstanceStatus.SourceInstanceStatusData getSourceStatus(String tenant, String namespace, String sourceName, int id) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override SourceStatus.SourceInstanceStatus.SourceInstanceStatusData GetSourceStatus(string Tenant, string Namespace, string SourceName, int Id)
		{
			try
			{
				Response Response = Request(source.path(Tenant).path(Namespace).path(SourceName).path(Convert.ToString(Id)).path("status")).get();
				if (!Response.StatusInfo.Equals(Response.Status.OK))
				{
					throw GetApiException(Response);
				}
				return Response.readEntity(typeof(SourceStatus.SourceInstanceStatus.SourceInstanceStatusData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createSource(org.apache.pulsar.common.io.SourceConfig sourceConfig, String fileName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateSource(SourceConfig SourceConfig, string FileName)
		{
			try
			{
				RequestBuilder Builder = post(source.path(SourceConfig.Tenant).path(SourceConfig.Namespace).path(SourceConfig.Name).Uri.toASCIIString()).addBodyPart(new StringPart("sourceConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(SourceConfig), MediaType.APPLICATION_JSON));

				if (!string.ReferenceEquals(FileName, null) && !FileName.StartsWith("builtin://", StringComparison.Ordinal))
				{
					// If the function code is built in, we don't need to submit here
					Builder.addBodyPart(new FilePart("data", new File(FileName), MediaType.APPLICATION_OCTET_STREAM));
				}
				org.asynchttpclient.Response Response = asyncHttpClient.executeRequest(AddAuthHeaders(source, Builder).build()).get();
				if (Response.StatusCode < 200 || Response.StatusCode >= 300)
				{
					throw GetApiException(Response.status(Response.StatusCode).entity(Response.ResponseBody).build());
				}
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createSourceWithUrl(org.apache.pulsar.common.io.SourceConfig sourceConfig, String pkgUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateSourceWithUrl(SourceConfig SourceConfig, string PkgUrl)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart Mp = new FormDataMultiPart();

				Mp.bodyPart(new FormDataBodyPart("url", PkgUrl, MediaType.TEXT_PLAIN_TYPE));

				Mp.bodyPart(new FormDataBodyPart("sourceConfig", (new Gson()).toJson(SourceConfig), MediaType.APPLICATION_JSON_TYPE));
				Request(source.path(SourceConfig.Tenant).path(SourceConfig.Namespace).path(SourceConfig.Name)).post(Entity.entity(Mp, MediaType.MULTIPART_FORM_DATA), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteSource(String cluster, String namespace, String function) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteSource(string Cluster, string Namespace, string Function)
		{
			try
			{
				Request(source.path(Cluster).path(Namespace).path(Function)).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSource(org.apache.pulsar.common.io.SourceConfig sourceConfig, String fileName, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateSource(SourceConfig SourceConfig, string FileName, UpdateOptions UpdateOptions)
		{
			try
			{
				RequestBuilder Builder = put(source.path(SourceConfig.Tenant).path(SourceConfig.Namespace).path(SourceConfig.Name).Uri.toASCIIString()).addBodyPart(new StringPart("sourceConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(SourceConfig), MediaType.APPLICATION_JSON));

				if (UpdateOptions != null)
				{
					Builder.addBodyPart(new StringPart("updateOptions", ObjectMapperFactory.ThreadLocal.writeValueAsString(UpdateOptions), MediaType.APPLICATION_JSON));
				}

				if (!string.ReferenceEquals(FileName, null) && !FileName.StartsWith("builtin://", StringComparison.Ordinal))
				{
					// If the function code is built in, we don't need to submit here
					Builder.addBodyPart(new FilePart("data", new File(FileName), MediaType.APPLICATION_OCTET_STREAM));
				}
				org.asynchttpclient.Response Response = asyncHttpClient.executeRequest(AddAuthHeaders(source, Builder).build()).get();

				if (Response.StatusCode < 200 || Response.StatusCode >= 300)
				{
					throw GetApiException(Response.status(Response.StatusCode).entity(Response.ResponseBody).build());
				}
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSource(org.apache.pulsar.common.io.SourceConfig sourceConfig, String fileName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateSource(SourceConfig SourceConfig, string FileName)
		{
			UpdateSource(SourceConfig, FileName, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSourceWithUrl(org.apache.pulsar.common.io.SourceConfig sourceConfig, String pkgUrl, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateSourceWithUrl(SourceConfig SourceConfig, string PkgUrl, UpdateOptions UpdateOptions)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart Mp = new FormDataMultiPart();

				Mp.bodyPart(new FormDataBodyPart("url", PkgUrl, MediaType.TEXT_PLAIN_TYPE));

				Mp.bodyPart(new FormDataBodyPart("sourceConfig", (new Gson()).toJson(SourceConfig), MediaType.APPLICATION_JSON_TYPE));

				if (UpdateOptions != null)
				{
					Mp.bodyPart(new FormDataBodyPart("updateOptions", ObjectMapperFactory.ThreadLocal.writeValueAsString(UpdateOptions), MediaType.APPLICATION_JSON_TYPE));
				}

				Request(source.path(SourceConfig.Tenant).path(SourceConfig.Namespace).path(SourceConfig.Name)).put(Entity.entity(Mp, MediaType.MULTIPART_FORM_DATA), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSourceWithUrl(org.apache.pulsar.common.io.SourceConfig sourceConfig, String pkgUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateSourceWithUrl(SourceConfig SourceConfig, string PkgUrl)
		{
			UpdateSourceWithUrl(SourceConfig, PkgUrl, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void restartSource(String tenant, String namespace, String functionName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void RestartSource(string Tenant, string Namespace, string FunctionName, int InstanceId)
		{
			try
			{
				Request(source.path(Tenant).path(Namespace).path(FunctionName).path(Convert.ToString(InstanceId)).path("restart")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void restartSource(String tenant, String namespace, String functionName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void RestartSource(string Tenant, string Namespace, string FunctionName)
		{
			try
			{
				Request(source.path(Tenant).path(Namespace).path(FunctionName).path("restart")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void stopSource(String tenant, String namespace, String sourceName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void StopSource(string Tenant, string Namespace, string SourceName, int InstanceId)
		{
			try
			{
				Request(source.path(Tenant).path(Namespace).path(SourceName).path(Convert.ToString(InstanceId)).path("stop")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void stopSource(String tenant, String namespace, String sourceName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void StopSource(string Tenant, string Namespace, string SourceName)
		{
			try
			{
				Request(source.path(Tenant).path(Namespace).path(SourceName).path("stop")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void startSource(String tenant, String namespace, String sourceName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void StartSource(string Tenant, string Namespace, string SourceName, int InstanceId)
		{
			try
			{
				Request(source.path(Tenant).path(Namespace).path(SourceName).path(Convert.ToString(InstanceId)).path("start")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void startSource(String tenant, String namespace, String sourceName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void StartSource(string Tenant, string Namespace, string SourceName)
		{
			try
			{
				Request(source.path(Tenant).path(Namespace).path(SourceName).path("start")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<org.apache.pulsar.common.io.ConnectorDefinition> getBuiltInSources() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<ConnectorDefinition> BuiltInSources
		{
			get
			{
				try
				{
					Response Response = Request(source.path("builtinsources")).get();
					if (!Response.StatusInfo.Equals(Response.Status.OK))
					{
						throw GetApiException(Response);
					}
					return Response.readEntity(new GenericTypeAnonymousInnerClass2(this));
				}
				catch (Exception E)
				{
					throw GetApiException(E);
				}
			}
		}

		public class GenericTypeAnonymousInnerClass2 : GenericType<IList<ConnectorDefinition>>
		{
			private readonly SourcesImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(SourcesImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void reloadBuiltInSources() throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void ReloadBuiltInSources()
		{
			try
			{
				Request(source.path("reloadBuiltInSources")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}
	}

}