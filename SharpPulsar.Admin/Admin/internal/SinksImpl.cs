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
	using SinkStatus = org.apache.pulsar.common.policies.data.SinkStatus;
	using SinkConfig = org.apache.pulsar.common.io.SinkConfig;
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
//ORIGINAL LINE: @Slf4j public class SinksImpl extends ComponentResource implements org.apache.pulsar.client.admin.Sinks, org.apache.pulsar.client.admin.Sink
	public class SinksImpl : ComponentResource, Sinks, Sink
	{

		private readonly WebTarget sink;
		private readonly AsyncHttpClient asyncHttpClient;

		public SinksImpl(WebTarget web, Authentication auth, AsyncHttpClient asyncHttpClient, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			this.sink = web.path("/admin/v3/sink");
			this.asyncHttpClient = asyncHttpClient;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> listSinks(String tenant, String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> listSinks(string tenant, string @namespace)
		{
			try
			{
				Response response = request(sink.path(tenant).path(@namespace)).get();
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
			private readonly SinksImpl outerInstance;

			public GenericTypeAnonymousInnerClass(SinksImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.io.SinkConfig getSink(String tenant, String namespace, String sinkName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual SinkConfig getSink(string tenant, string @namespace, string sinkName)
		{
			try
			{
				 Response response = request(sink.path(tenant).path(@namespace).path(sinkName)).get();
				if (!response.StatusInfo.Equals(Response.Status.OK))
				{
					throw getApiException(response);
				}
				return response.readEntity(typeof(SinkConfig));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SinkStatus getSinkStatus(String tenant, String namespace, String sinkName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual SinkStatus getSinkStatus(string tenant, string @namespace, string sinkName)
		{
			try
			{
				Response response = request(sink.path(tenant).path(@namespace).path(sinkName).path("status")).get();
				if (!response.StatusInfo.Equals(Response.Status.OK))
				{
					throw getApiException(response);
				}
				return response.readEntity(typeof(SinkStatus));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SinkStatus.SinkInstanceStatus.SinkInstanceStatusData getSinkStatus(String tenant, String namespace, String sinkName, int id) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual SinkStatus.SinkInstanceStatus.SinkInstanceStatusData getSinkStatus(string tenant, string @namespace, string sinkName, int id)
		{
			try
			{
				Response response = request(sink.path(tenant).path(@namespace).path(sinkName).path(Convert.ToString(id)).path("status")).get();
				if (!response.StatusInfo.Equals(Response.Status.OK))
				{
					throw getApiException(response);
				}
				return response.readEntity(typeof(SinkStatus.SinkInstanceStatus.SinkInstanceStatusData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createSink(org.apache.pulsar.common.io.SinkConfig sinkConfig, String fileName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createSink(SinkConfig sinkConfig, string fileName)
		{
			try
			{
				RequestBuilder builder = post(sink.path(sinkConfig.Tenant).path(sinkConfig.Namespace).path(sinkConfig.Name).Uri.toASCIIString()).addBodyPart(new StringPart("sinkConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(sinkConfig), MediaType.APPLICATION_JSON));

				if (!string.ReferenceEquals(fileName, null) && !fileName.StartsWith("builtin://", StringComparison.Ordinal))
				{
					// If the function code is built in, we don't need to submit here
					builder.addBodyPart(new FilePart("data", new File(fileName), MediaType.APPLICATION_OCTET_STREAM));
				}
				org.asynchttpclient.Response response = asyncHttpClient.executeRequest(addAuthHeaders(sink, builder).build()).get();

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
//ORIGINAL LINE: @Override public void createSinkWithUrl(org.apache.pulsar.common.io.SinkConfig sinkConfig, String pkgUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createSinkWithUrl(SinkConfig sinkConfig, string pkgUrl)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart mp = new FormDataMultiPart();

				mp.bodyPart(new FormDataBodyPart("url", pkgUrl, MediaType.TEXT_PLAIN_TYPE));

				mp.bodyPart(new FormDataBodyPart("sinkConfig", (new Gson()).toJson(sinkConfig), MediaType.APPLICATION_JSON_TYPE));
				request(sink.path(sinkConfig.Tenant).path(sinkConfig.Namespace).path(sinkConfig.Name)).post(Entity.entity(mp, MediaType.MULTIPART_FORM_DATA), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteSink(String cluster, String namespace, String function) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteSink(string cluster, string @namespace, string function)
		{
			try
			{
				request(sink.path(cluster).path(@namespace).path(function)).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSink(org.apache.pulsar.common.io.SinkConfig sinkConfig, String fileName, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateSink(SinkConfig sinkConfig, string fileName, UpdateOptions updateOptions)
		{
			try
			{
				RequestBuilder builder = put(sink.path(sinkConfig.Tenant).path(sinkConfig.Namespace).path(sinkConfig.Name).Uri.toASCIIString()).addBodyPart(new StringPart("sinkConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(sinkConfig), MediaType.APPLICATION_JSON));

				if (updateOptions != null)
				{
					builder.addBodyPart(new StringPart("updateOptions", ObjectMapperFactory.ThreadLocal.writeValueAsString(updateOptions), MediaType.APPLICATION_JSON));
				}

				if (!string.ReferenceEquals(fileName, null) && !fileName.StartsWith("builtin://", StringComparison.Ordinal))
				{
					// If the function code is built in, we don't need to submit here
					builder.addBodyPart(new FilePart("data", new File(fileName), MediaType.APPLICATION_OCTET_STREAM));
				}
				org.asynchttpclient.Response response = asyncHttpClient.executeRequest(addAuthHeaders(sink, builder).build()).get();

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
//ORIGINAL LINE: @Override public void updateSink(org.apache.pulsar.common.io.SinkConfig sinkConfig, String fileName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateSink(SinkConfig sinkConfig, string fileName)
		{
		   updateSink(sinkConfig, fileName, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSinkWithUrl(org.apache.pulsar.common.io.SinkConfig sinkConfig, String pkgUrl, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateSinkWithUrl(SinkConfig sinkConfig, string pkgUrl, UpdateOptions updateOptions)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart mp = new FormDataMultiPart();

				mp.bodyPart(new FormDataBodyPart("url", pkgUrl, MediaType.TEXT_PLAIN_TYPE));

				mp.bodyPart(new FormDataBodyPart("sinkConfig", (new Gson()).toJson(sinkConfig), MediaType.APPLICATION_JSON_TYPE));

				if (updateOptions != null)
				{
					mp.bodyPart(new FormDataBodyPart("updateOptions", ObjectMapperFactory.ThreadLocal.writeValueAsString(updateOptions), MediaType.APPLICATION_JSON_TYPE));
				}

				request(sink.path(sinkConfig.Tenant).path(sinkConfig.Namespace).path(sinkConfig.Name)).put(Entity.entity(mp, MediaType.MULTIPART_FORM_DATA), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSinkWithUrl(org.apache.pulsar.common.io.SinkConfig sinkConfig, String pkgUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateSinkWithUrl(SinkConfig sinkConfig, string pkgUrl)
		{
			updateSinkWithUrl(sinkConfig, pkgUrl, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void restartSink(String tenant, String namespace, String functionName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void restartSink(string tenant, string @namespace, string functionName, int instanceId)
		{
			try
			{
				request(sink.path(tenant).path(@namespace).path(functionName).path(Convert.ToString(instanceId)).path("restart")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void restartSink(String tenant, String namespace, String functionName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void restartSink(string tenant, string @namespace, string functionName)
		{
			try
			{
				request(sink.path(tenant).path(@namespace).path(functionName).path("restart")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void stopSink(String tenant, String namespace, String sinkName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void stopSink(string tenant, string @namespace, string sinkName, int instanceId)
		{
			try
			{
				request(sink.path(tenant).path(@namespace).path(sinkName).path(Convert.ToString(instanceId)).path("stop")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void stopSink(String tenant, String namespace, String sinkName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void stopSink(string tenant, string @namespace, string sinkName)
		{
			try
			{
				request(sink.path(tenant).path(@namespace).path(sinkName).path("stop")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void startSink(String tenant, String namespace, String sinkName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void startSink(string tenant, string @namespace, string sinkName, int instanceId)
		{
			try
			{
				request(sink.path(tenant).path(@namespace).path(sinkName).path(Convert.ToString(instanceId)).path("start")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void startSink(String tenant, String namespace, String sinkName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void startSink(string tenant, string @namespace, string sinkName)
		{
			try
			{
				request(sink.path(tenant).path(@namespace).path(sinkName).path("start")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<org.apache.pulsar.common.io.ConnectorDefinition> getBuiltInSinks() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<ConnectorDefinition> BuiltInSinks
		{
			get
			{
				try
				{
					Response response = request(sink.path("builtinsinks")).get();
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
			private readonly SinksImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(SinksImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void reloadBuiltInSinks() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void reloadBuiltInSinks()
		{
			try
			{
				request(sink.path("reloadBuiltInSinks")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}
	}

}