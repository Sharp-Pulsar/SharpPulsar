using System;
using System.Collections.Generic;
using System.IO;

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
	using TypeToken = com.google.gson.reflect.TypeToken;
	using HttpHeaders = io.netty.handler.codec.http.HttpHeaders;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using StringUtils = apache.commons.lang3.StringUtils;
	using Authentication = client.api.Authentication;
	using FunctionConfig = pulsar.common.functions.FunctionConfig;
	using FunctionState = pulsar.common.functions.FunctionState;
	using UpdateOptions = pulsar.common.functions.UpdateOptions;
	using WorkerInfo = pulsar.common.functions.WorkerInfo;
	using ConnectorDefinition = pulsar.common.io.ConnectorDefinition;
	using ErrorData = pulsar.common.policies.data.ErrorData;
	using FunctionStats = pulsar.common.policies.data.FunctionStats;
	using FunctionStatus = pulsar.common.policies.data.FunctionStatus;
	using ObjectMapperFactory = pulsar.common.util.ObjectMapperFactory;
	using AsyncHandler = org.asynchttpclient.AsyncHandler;
	using AsyncHttpClient = org.asynchttpclient.AsyncHttpClient;
	using HttpResponseBodyPart = org.asynchttpclient.HttpResponseBodyPart;
	using HttpResponseStatus = org.asynchttpclient.HttpResponseStatus;
	using RequestBuilder = org.asynchttpclient.RequestBuilder;
	using FilePart = org.asynchttpclient.request.body.multipart.FilePart;
	using StringPart = org.asynchttpclient.request.body.multipart.StringPart;
	using FormDataBodyPart = org.glassfish.jersey.media.multipart.FormDataBodyPart;
	using FormDataMultiPart = org.glassfish.jersey.media.multipart.FormDataMultiPart;
	using FileDataBodyPart = org.glassfish.jersey.media.multipart.file.FileDataBodyPart;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.asynchttpclient.Dsl.get;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.asynchttpclient.Dsl.post;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.asynchttpclient.Dsl.put;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class FunctionsImpl extends ComponentResource implements org.apache.pulsar.client.admin.Functions
	public class FunctionsImpl : ComponentResource, Functions
	{

		private readonly WebTarget functions;
		private readonly AsyncHttpClient asyncHttpClient;

		public FunctionsImpl(WebTarget web, Authentication auth, AsyncHttpClient asyncHttpClient, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			this.functions = web.path("/admin/v3/functions");
			this.asyncHttpClient = asyncHttpClient;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getFunctions(String tenant, String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<string> getFunctions(string tenant, string @namespace)
		{
			try
			{
				Response response = request(functions.path(tenant).path(@namespace)).get();
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
			private readonly FunctionsImpl outerInstance;

			public GenericTypeAnonymousInnerClass(FunctionsImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.functions.FunctionConfig getFunction(String tenant, String namespace, String function) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual FunctionConfig getFunction(string tenant, string @namespace, string function)
		{
			try
			{
				 Response response = request(functions.path(tenant).path(@namespace).path(function)).get();
				if (!response.StatusInfo.Equals(Response.Status.OK))
				{
					throw getApiException(response);
				}
				return response.readEntity(typeof(FunctionConfig));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.FunctionStatus getFunctionStatus(String tenant, String namespace, String function) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual FunctionStatus getFunctionStatus(string tenant, string @namespace, string function)
		{
			try
			{
				Response response = request(functions.path(tenant).path(@namespace).path(function).path("status")).get();
				if (!response.StatusInfo.Equals(Response.Status.OK))
				{
					throw getApiException(response);
				}
				return response.readEntity(typeof(FunctionStatus));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.pulsar.common.policies.data.FunctionStatus.FunctionInstanceStatus.FunctionInstanceStatusData getFunctionStatus(String tenant, String namespace, String function, int id) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual FunctionStatus.FunctionInstanceStatus.FunctionInstanceStatusData getFunctionStatus(string tenant, string @namespace, string function, int id)
		{
			try
			{
				Response response = request(functions.path(tenant).path(@namespace).path(function).path(Convert.ToString(id)).path("status")).get();
				if (!response.StatusInfo.Equals(Response.Status.OK))
				{
					throw getApiException(response);
				}
				return response.readEntity(typeof(FunctionStatus.FunctionInstanceStatus.FunctionInstanceStatusData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.FunctionStats.FunctionInstanceStats.FunctionInstanceStatsData getFunctionStats(String tenant, String namespace, String function, int id) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual FunctionStats.FunctionInstanceStats.FunctionInstanceStatsData getFunctionStats(string tenant, string @namespace, string function, int id)
		{
			try
			{
				Response response = request(functions.path(tenant).path(@namespace).path(function).path(Convert.ToString(id)).path("stats")).get();
				if (!response.StatusInfo.Equals(Response.Status.OK))
				{
					throw getApiException(response);
				}
				return response.readEntity(typeof(FunctionStats.FunctionInstanceStats.FunctionInstanceStatsData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.FunctionStats getFunctionStats(String tenant, String namespace, String function) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual FunctionStats getFunctionStats(string tenant, string @namespace, string function)
		{
			try
			{
				Response response = request(functions.path(tenant).path(@namespace).path(function).path("stats")).get();
				if (!response.StatusInfo.Equals(Response.Status.OK))
				{
					throw getApiException(response);
				}
				return response.readEntity(typeof(FunctionStats));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createFunction(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String fileName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createFunction(FunctionConfig functionConfig, string fileName)
		{
			try
			{
				RequestBuilder builder = post(functions.path(functionConfig.Tenant).path(functionConfig.Namespace).path(functionConfig.Name).Uri.toASCIIString()).addBodyPart(new StringPart("functionConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(functionConfig), MediaType.APPLICATION_JSON));

				if (!string.ReferenceEquals(fileName, null) && !fileName.StartsWith("builtin://", StringComparison.Ordinal))
				{
					// If the function code is built in, we don't need to submit here
				   builder.addBodyPart(new FilePart("data", new File(fileName), MediaType.APPLICATION_OCTET_STREAM));
				}
				org.asynchttpclient.Response response = asyncHttpClient.executeRequest(addAuthHeaders(functions, builder).build()).get();

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
//ORIGINAL LINE: @Override public void createFunctionWithUrl(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String pkgUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createFunctionWithUrl(FunctionConfig functionConfig, string pkgUrl)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart mp = new FormDataMultiPart();

				mp.bodyPart(new FormDataBodyPart("url", pkgUrl, MediaType.TEXT_PLAIN_TYPE));

				mp.bodyPart(new FormDataBodyPart("functionConfig", (new Gson()).toJson(functionConfig), MediaType.APPLICATION_JSON_TYPE));
				request(functions.path(functionConfig.Tenant).path(functionConfig.Namespace).path(functionConfig.Name)).post(Entity.entity(mp, MediaType.MULTIPART_FORM_DATA), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteFunction(String cluster, String namespace, String function) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteFunction(string cluster, string @namespace, string function)
		{
			try
			{
				request(functions.path(cluster).path(@namespace).path(function)).delete(typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateFunction(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String fileName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateFunction(FunctionConfig functionConfig, string fileName)
		{
			updateFunction(functionConfig, fileName, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateFunction(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String fileName, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws org.apache.pulsar.client.admin.PulsarAdminException
			public virtual void updateFunction(FunctionConfig functionConfig, string fileName, UpdateOptions updateOptions)
			{
			try
			{
				RequestBuilder builder = put(functions.path(functionConfig.Tenant).path(functionConfig.Namespace).path(functionConfig.Name).Uri.toASCIIString()).addBodyPart(new StringPart("functionConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(functionConfig), MediaType.APPLICATION_JSON));

				if (updateOptions != null)
				{
					   builder.addBodyPart(new StringPart("updateOptions", ObjectMapperFactory.ThreadLocal.writeValueAsString(updateOptions), MediaType.APPLICATION_JSON));
				}

				if (!string.ReferenceEquals(fileName, null) && !fileName.StartsWith("builtin://", StringComparison.Ordinal))
				{
					// If the function code is built in, we don't need to submit here
					builder.addBodyPart(new FilePart("data", new File(fileName), MediaType.APPLICATION_OCTET_STREAM));
				}
				org.asynchttpclient.Response response = asyncHttpClient.executeRequest(addAuthHeaders(functions, builder).build()).get();

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
//ORIGINAL LINE: @Override public void updateFunctionWithUrl(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String pkgUrl, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateFunctionWithUrl(FunctionConfig functionConfig, string pkgUrl, UpdateOptions updateOptions)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart mp = new FormDataMultiPart();

				mp.bodyPart(new FormDataBodyPart("url", pkgUrl, MediaType.TEXT_PLAIN_TYPE));

				mp.bodyPart(new FormDataBodyPart("functionConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(functionConfig), MediaType.APPLICATION_JSON_TYPE));

				if (updateOptions != null)
				{
					mp.bodyPart(new FormDataBodyPart("updateOptions", ObjectMapperFactory.ThreadLocal.writeValueAsString(updateOptions), MediaType.APPLICATION_JSON_TYPE));
				}

				request(functions.path(functionConfig.Tenant).path(functionConfig.Namespace).path(functionConfig.Name)).put(Entity.entity(mp, MediaType.MULTIPART_FORM_DATA), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateFunctionWithUrl(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String pkgUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void updateFunctionWithUrl(FunctionConfig functionConfig, string pkgUrl)
		{
			updateFunctionWithUrl(functionConfig, pkgUrl, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public String triggerFunction(String tenant, String namespace, String functionName, String topic, String triggerValue, String triggerFile) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual string triggerFunction(string tenant, string @namespace, string functionName, string topic, string triggerValue, string triggerFile)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart mp = new FormDataMultiPart();
				if (!string.ReferenceEquals(triggerFile, null))
				{
					mp.bodyPart(new FileDataBodyPart("dataStream", new File(triggerFile), MediaType.APPLICATION_OCTET_STREAM_TYPE));
				}
				if (!string.ReferenceEquals(triggerValue, null))
				{
					mp.bodyPart(new FormDataBodyPart("data", triggerValue, MediaType.TEXT_PLAIN_TYPE));
				}
				if (!string.ReferenceEquals(topic, null) && topic.Length > 0)
				{
					mp.bodyPart(new FormDataBodyPart("topic", topic, MediaType.TEXT_PLAIN_TYPE));
				}
				return request(functions.path(tenant).path(@namespace).path(functionName).path("trigger")).post(Entity.entity(mp, MediaType.MULTIPART_FORM_DATA), typeof(string));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void restartFunction(String tenant, String namespace, String functionName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void restartFunction(string tenant, string @namespace, string functionName, int instanceId)
		{
			try
			{
				request(functions.path(tenant).path(@namespace).path(functionName).path(Convert.ToString(instanceId)).path("restart")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void restartFunction(String tenant, String namespace, String functionName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void restartFunction(string tenant, string @namespace, string functionName)
		{
			try
			{
				request(functions.path(tenant).path(@namespace).path(functionName).path("restart")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void stopFunction(String tenant, String namespace, String functionName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void stopFunction(string tenant, string @namespace, string functionName, int instanceId)
		{
			try
			{
				request(functions.path(tenant).path(@namespace).path(functionName).path(Convert.ToString(instanceId)).path("stop")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void stopFunction(String tenant, String namespace, String functionName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void stopFunction(string tenant, string @namespace, string functionName)
		{
			try
			{
				request(functions.path(tenant).path(@namespace).path(functionName).path("stop")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void startFunction(String tenant, String namespace, String functionName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void startFunction(string tenant, string @namespace, string functionName, int instanceId)
		{
			try
			{
				request(functions.path(tenant).path(@namespace).path(functionName).path(Convert.ToString(instanceId)).path("start")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void startFunction(String tenant, String namespace, String functionName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void startFunction(string tenant, string @namespace, string functionName)
		{
			try
			{
				request(functions.path(tenant).path(@namespace).path(functionName).path("start")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void uploadFunction(String sourceFile, String path) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void uploadFunction(string sourceFile, string path)
		{
			try
			{
				RequestBuilder builder = post(functions.path("upload").Uri.toASCIIString()).addBodyPart(new FilePart("data", new File(sourceFile), MediaType.APPLICATION_OCTET_STREAM)).addBodyPart(new StringPart("path", path, MediaType.TEXT_PLAIN));

				org.asynchttpclient.Response response = asyncHttpClient.executeRequest(addAuthHeaders(functions, builder).build()).get();
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
//ORIGINAL LINE: @Override public void downloadFunction(String destinationPath, String tenant, String namespace, String functionName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void downloadFunction(string destinationPath, string tenant, string @namespace, string functionName)
		{
			downloadFile(destinationPath, functions.path(tenant).path(@namespace).path(functionName).path("download"));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void downloadFunction(String destinationPath, String path) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void downloadFunction(string destinationPath, string path)
		{
			downloadFile(destinationPath, functions.path("download").queryParam("path", path));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void downloadFile(String destinationPath, javax.ws.rs.client.WebTarget target) throws org.apache.pulsar.client.admin.PulsarAdminException
		private void downloadFile(string destinationPath, WebTarget target)
		{
			HttpResponseStatus status;
			try
			{
				File file = new File(destinationPath);
				if (!file.exists())
				{
					file.createNewFile();
				}
				FileChannel os = (new FileStream(destinationPath, FileMode.Create, FileAccess.Write)).Channel;

				RequestBuilder builder = get(target.Uri.toASCIIString());

				Future<HttpResponseStatus> whenStatusCode = asyncHttpClient.executeRequest(addAuthHeaders(functions, builder).build(), new AsyncHandlerAnonymousInnerClass(this, os));

				status = whenStatusCode.get();
				os.close();

				if (status.StatusCode < 200 || status.StatusCode >= 300)
				{
					throw getApiException(Response.status(status.StatusCode).entity(status.StatusText).build());
				}
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private class AsyncHandlerAnonymousInnerClass : AsyncHandler<HttpResponseStatus>
		{
			private readonly FunctionsImpl outerInstance;

			private FileChannel os;

			public AsyncHandlerAnonymousInnerClass(FunctionsImpl outerInstance, FileChannel os)
			{
				this.outerInstance = outerInstance;
				this.os = os;
			}

			private HttpResponseStatus status;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public State onStatusReceived(org.asynchttpclient.HttpResponseStatus responseStatus) throws Exception
			public override State onStatusReceived(HttpResponseStatus responseStatus)
			{
				status = responseStatus;
				if (status.StatusCode != Response.Status.OK.StatusCode)
				{
					return State.ABORT;
				}
				return State.CONTINUE;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public State onHeadersReceived(io.netty.handler.codec.http.HttpHeaders headers) throws Exception
			public override State onHeadersReceived(HttpHeaders headers)
			{
				return State.CONTINUE;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public State onBodyPartReceived(org.asynchttpclient.HttpResponseBodyPart bodyPart) throws Exception
			public override State onBodyPartReceived(HttpResponseBodyPart bodyPart)
			{

				os.write(bodyPart.BodyByteBuffer);
				return State.CONTINUE;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.asynchttpclient.HttpResponseStatus onCompleted() throws Exception
			public override HttpResponseStatus onCompleted()
			{
				return status;
			}

			public override void onThrowable(Exception t)
			{
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<org.apache.pulsar.common.io.ConnectorDefinition> getConnectorsList() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<ConnectorDefinition> ConnectorsList
		{
			get
			{
				try
				{
					Response response = request(functions.path("connectors")).get();
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
			private readonly FunctionsImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(FunctionsImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Set<String> getSources() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual ISet<string> Sources
		{
			get
			{
	//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
				return ConnectorsList.Where(c => !StringUtils.isEmpty(c.SourceClass)).Select(ConnectorDefinition.getName).collect(Collectors.toSet());
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Set<String> getSinks() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual ISet<string> Sinks
		{
			get
			{
	//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
				return ConnectorsList.Where(c => !StringUtils.isEmpty(c.SinkClass)).Select(ConnectorDefinition.getName).collect(Collectors.toSet());
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<org.apache.pulsar.common.functions.WorkerInfo> getCluster() throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<WorkerInfo> Cluster
		{
			get
			{
				try
				{
					return request(functions.path("cluster")).get(new GenericTypeAnonymousInnerClass3(this));
				}
				catch (Exception e)
				{
					throw getApiException(e);
				}
			}
		}

		private class GenericTypeAnonymousInnerClass3 : GenericType<IList<WorkerInfo>>
		{
			private readonly FunctionsImpl outerInstance;

			public GenericTypeAnonymousInnerClass3(FunctionsImpl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.functions.FunctionState getFunctionState(String tenant, String namespace, String function, String key) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual FunctionState getFunctionState(string tenant, string @namespace, string function, string key)
		{
			try
			{
				Response response = request(functions.path(tenant).path(@namespace).path(function).path("state").path(key)).get();
				if (!response.StatusInfo.Equals(Response.Status.OK))
				{
					throw getApiException(response);
				}
				return response.readEntity(typeof(FunctionState));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void putFunctionState(String tenant, String namespace, String function, org.apache.pulsar.common.functions.FunctionState state) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void putFunctionState(string tenant, string @namespace, string function, FunctionState state)
		{
			try
			{
				 RequestBuilder builder = post(functions.path(tenant).path(@namespace).path(function).path("state").path(state.Key).Uri.toASCIIString());
				 builder.addBodyPart(new StringPart("state", ObjectMapperFactory.ThreadLocal.writeValueAsString(state), MediaType.APPLICATION_JSON));
				 org.asynchttpclient.Response response = asyncHttpClient.executeRequest(addAuthHeaders(functions, builder).build()).get();

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
	}

}