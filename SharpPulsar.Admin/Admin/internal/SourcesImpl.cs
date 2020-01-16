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
	using Gson = com.google.gson.Gson;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Authentication = org.apache.pulsar.client.api.Authentication;
	using UpdateOptions = org.apache.pulsar.common.functions.UpdateOptions;
	using ConnectorDefinition = org.apache.pulsar.common.io.ConnectorDefinition;
	using ErrorData = org.apache.pulsar.common.policies.data.ErrorData;
	using SourceConfig = org.apache.pulsar.common.io.SourceConfig;
	using SourceStatus = org.apache.pulsar.common.policies.data.SourceStatus;
	using ObjectMapperFactory = org.apache.pulsar.common.util.ObjectMapperFactory;
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

		public SourcesImpl(WebTarget web, Authentication auth, AsyncHttpClient asyncHttpClient, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			this.source = web.path("/admin/v3/source");
			this.asyncHttpClient = asyncHttpClient;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> listSources(String tenant, String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> listSources(string tenant, string @namespace)
		{
			try
			{
				Response response = request(source.path(tenant).path(@namespace)).get();
				if (!response.StatusInfo.Equals(Response.Status.OK))
				{
					throw getApiException(response);
				}
				return response.readEntity(new GenericTypeAnonymousInnerClass(this));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class GenericTypeAnonymousInnerClass : GenericType<IList<string>>
		{
			private readonly SourcesImpl outerInstance;

			public GenericTypeAnonymousInnerClass(SourcesImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.io.SourceConfig getSource(String tenant, String namespace, String sourceName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual SourceConfig getSource(string tenant, string @namespace, string sourceName)
		{
			try
			{
				 Response response = request(source.path(tenant).path(@namespace).path(sourceName)).get();
				if (!response.StatusInfo.Equals(Response.Status.OK))
				{
					throw getApiException(response);
				}
				return response.readEntity(typeof(SourceConfig));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SourceStatus getSourceStatus(String tenant, String namespace, String sourceName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual SourceStatus getSourceStatus(string tenant, string @namespace, string sourceName)
		{
			try
			{
				Response response = request(source.path(tenant).path(@namespace).path(sourceName).path("status")).get();
				if (!response.StatusInfo.Equals(Response.Status.OK))
				{
					throw getApiException(response);
				}
				return response.readEntity(typeof(SourceStatus));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SourceStatus.SourceInstanceStatus.SourceInstanceStatusData getSourceStatus(String tenant, String namespace, String sourceName, int id) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual SourceStatus.SourceInstanceStatus.SourceInstanceStatusData getSourceStatus(string tenant, string @namespace, string sourceName, int id)
		{
			try
			{
				Response response = request(source.path(tenant).path(@namespace).path(sourceName).path(Convert.ToString(id)).path("status")).get();
				if (!response.StatusInfo.Equals(Response.Status.OK))
				{
					throw getApiException(response);
				}
				return response.readEntity(typeof(SourceStatus.SourceInstanceStatus.SourceInstanceStatusData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createSource(org.apache.pulsar.common.io.SourceConfig sourceConfig, String fileName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createSource(SourceConfig sourceConfig, string fileName)
		{
			try
			{
				RequestBuilder builder = post(source.path(sourceConfig.Tenant).path(sourceConfig.Namespace).path(sourceConfig.Name).Uri.toASCIIString()).addBodyPart(new StringPart("sourceConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(sourceConfig), MediaType.APPLICATION_JSON));

				if (!string.ReferenceEquals(fileName, null) && !fileName.StartsWith("builtin://", StringComparison.Ordinal))
				{
					// If the function code is built in, we don't need to submit here
					builder.addBodyPart(new FilePart("data", new File(fileName), MediaType.APPLICATION_OCTET_STREAM));
				}
				org.asynchttpclient.Response response = asyncHttpClient.executeRequest(addAuthHeaders(source, builder).build()).get();
				if (response.StatusCode < 200 || response.StatusCode >= 300)
				{
					throw getApiException(Response.status(response.StatusCode).entity(response.ResponseBody).build());
				}
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createSourceWithUrl(org.apache.pulsar.common.io.SourceConfig sourceConfig, String pkgUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createSourceWithUrl(SourceConfig sourceConfig, string pkgUrl)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart mp = new FormDataMultiPart();

				mp.bodyPart(new FormDataBodyPart("url", pkgUrl, MediaType.TEXT_PLAIN_TYPE));

				mp.bodyPart(new FormDataBodyPart("sourceConfig", (new Gson()).toJson(sourceConfig), MediaType.APPLICATION_JSON_TYPE));
				request(source.path(sourceConfig.Tenant).path(sourceConfig.Namespace).path(sourceConfig.Name)).post(Entity.entity(mp, MediaType.MULTIPART_FORM_DATA), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteSource(String cluster, String namespace, String function) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteSource(string cluster, string @namespace, string function)
		{
			try
			{
				request(source.path(cluster).path(@namespace).path(function)).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSource(org.apache.pulsar.common.io.SourceConfig sourceConfig, String fileName, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateSource(SourceConfig sourceConfig, string fileName, UpdateOptions updateOptions)
		{
			try
			{
				RequestBuilder builder = put(source.path(sourceConfig.Tenant).path(sourceConfig.Namespace).path(sourceConfig.Name).Uri.toASCIIString()).addBodyPart(new StringPart("sourceConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(sourceConfig), MediaType.APPLICATION_JSON));

				if (updateOptions != null)
				{
					builder.addBodyPart(new StringPart("updateOptions", ObjectMapperFactory.ThreadLocal.writeValueAsString(updateOptions), MediaType.APPLICATION_JSON));
				}

				if (!string.ReferenceEquals(fileName, null) && !fileName.StartsWith("builtin://", StringComparison.Ordinal))
				{
					// If the function code is built in, we don't need to submit here
					builder.addBodyPart(new FilePart("data", new File(fileName), MediaType.APPLICATION_OCTET_STREAM));
				}
				org.asynchttpclient.Response response = asyncHttpClient.executeRequest(addAuthHeaders(source, builder).build()).get();

				if (response.StatusCode < 200 || response.StatusCode >= 300)
				{
					throw getApiException(Response.status(response.StatusCode).entity(response.ResponseBody).build());
				}
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSource(org.apache.pulsar.common.io.SourceConfig sourceConfig, String fileName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateSource(SourceConfig sourceConfig, string fileName)
		{
			updateSource(sourceConfig, fileName, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSourceWithUrl(org.apache.pulsar.common.io.SourceConfig sourceConfig, String pkgUrl, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateSourceWithUrl(SourceConfig sourceConfig, string pkgUrl, UpdateOptions updateOptions)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart mp = new FormDataMultiPart();

				mp.bodyPart(new FormDataBodyPart("url", pkgUrl, MediaType.TEXT_PLAIN_TYPE));

				mp.bodyPart(new FormDataBodyPart("sourceConfig", (new Gson()).toJson(sourceConfig), MediaType.APPLICATION_JSON_TYPE));

				if (updateOptions != null)
				{
					mp.bodyPart(new FormDataBodyPart("updateOptions", ObjectMapperFactory.ThreadLocal.writeValueAsString(updateOptions), MediaType.APPLICATION_JSON_TYPE));
				}

				request(source.path(sourceConfig.Tenant).path(sourceConfig.Namespace).path(sourceConfig.Name)).put(Entity.entity(mp, MediaType.MULTIPART_FORM_DATA), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSourceWithUrl(org.apache.pulsar.common.io.SourceConfig sourceConfig, String pkgUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateSourceWithUrl(SourceConfig sourceConfig, string pkgUrl)
		{
			updateSourceWithUrl(sourceConfig, pkgUrl, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void restartSource(String tenant, String namespace, String functionName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void restartSource(string tenant, string @namespace, string functionName, int instanceId)
		{
			try
			{
				request(source.path(tenant).path(@namespace).path(functionName).path(Convert.ToString(instanceId)).path("restart")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void restartSource(String tenant, String namespace, String functionName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void restartSource(string tenant, string @namespace, string functionName)
		{
			try
			{
				request(source.path(tenant).path(@namespace).path(functionName).path("restart")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void stopSource(String tenant, String namespace, String sourceName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void stopSource(string tenant, string @namespace, string sourceName, int instanceId)
		{
			try
			{
				request(source.path(tenant).path(@namespace).path(sourceName).path(Convert.ToString(instanceId)).path("stop")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void stopSource(String tenant, String namespace, String sourceName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void stopSource(string tenant, string @namespace, string sourceName)
		{
			try
			{
				request(source.path(tenant).path(@namespace).path(sourceName).path("stop")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void startSource(String tenant, String namespace, String sourceName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void startSource(string tenant, string @namespace, string sourceName, int instanceId)
		{
			try
			{
				request(source.path(tenant).path(@namespace).path(sourceName).path(Convert.ToString(instanceId)).path("start")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void startSource(String tenant, String namespace, String sourceName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void startSource(string tenant, string @namespace, string sourceName)
		{
			try
			{
				request(source.path(tenant).path(@namespace).path(sourceName).path("start")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
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
					Response response = request(source.path("builtinsources")).get();
					if (!response.StatusInfo.Equals(Response.Status.OK))
					{
						throw getApiException(response);
					}
					return response.readEntity(new GenericTypeAnonymousInnerClass2(this));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

		private class GenericTypeAnonymousInnerClass2 : GenericType<IList<ConnectorDefinition>>
		{
			private readonly SourcesImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(SourcesImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void reloadBuiltInSources() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void reloadBuiltInSources()
		{
			try
			{
				request(source.path("reloadBuiltInSources")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}
	}

}