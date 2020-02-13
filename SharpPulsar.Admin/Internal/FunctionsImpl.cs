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
namespace Org.Apache.Pulsar.Client.Admin.@internal
{
	using Gson = com.google.gson.Gson;
	using TypeToken = com.google.gson.reflect.TypeToken;
	using HttpHeaders = io.netty.handler.codec.http.HttpHeaders;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using FunctionConfig = Org.Apache.Pulsar.Common.Functions.FunctionConfig;
	using FunctionState = Org.Apache.Pulsar.Common.Functions.FunctionState;
	using UpdateOptions = Org.Apache.Pulsar.Common.Functions.UpdateOptions;
	using WorkerInfo = Org.Apache.Pulsar.Common.Functions.WorkerInfo;
	using ConnectorDefinition = Org.Apache.Pulsar.Common.Io.ConnectorDefinition;
	using ErrorData = Org.Apache.Pulsar.Common.Policies.Data.ErrorData;
	using FunctionStats = Org.Apache.Pulsar.Common.Policies.Data.FunctionStats;
	using FunctionStatus = Org.Apache.Pulsar.Common.Policies.Data.FunctionStatus;
	using ObjectMapperFactory = Org.Apache.Pulsar.Common.Util.ObjectMapperFactory;
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

		public FunctionsImpl(WebTarget Web, Authentication Auth, AsyncHttpClient AsyncHttpClient, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
			this.functions = Web.path("/admin/v3/functions");
			this.asyncHttpClient = AsyncHttpClient;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> getFunctions(String tenant, String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> GetFunctions(string Tenant, string Namespace)
		{
			try
			{
				Response Response = Request(functions.path(Tenant).path(Namespace)).get();
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
			private readonly FunctionsImpl outerInstance;

			public GenericTypeAnonymousInnerClass(FunctionsImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.functions.FunctionConfig getFunction(String tenant, String namespace, String function) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override FunctionConfig GetFunction(string Tenant, string Namespace, string Function)
		{
			try
			{
				 Response Response = Request(functions.path(Tenant).path(Namespace).path(Function)).get();
				if (!Response.StatusInfo.Equals(Response.Status.OK))
				{
					throw GetApiException(Response);
				}
				return Response.readEntity(typeof(FunctionConfig));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.FunctionStatus getFunctionStatus(String tenant, String namespace, String function) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override FunctionStatus GetFunctionStatus(string Tenant, string Namespace, string Function)
		{
			try
			{
				Response Response = Request(functions.path(Tenant).path(Namespace).path(Function).path("status")).get();
				if (!Response.StatusInfo.Equals(Response.Status.OK))
				{
					throw GetApiException(Response);
				}
				return Response.readEntity(typeof(FunctionStatus));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.pulsar.common.policies.data.FunctionStatus.FunctionInstanceStatus.FunctionInstanceStatusData getFunctionStatus(String tenant, String namespace, String function, int id) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual FunctionStatus.FunctionInstanceStatus.FunctionInstanceStatusData GetFunctionStatus(string Tenant, string Namespace, string Function, int Id)
		{
			try
			{
				Response Response = Request(functions.path(Tenant).path(Namespace).path(Function).path(Convert.ToString(Id)).path("status")).get();
				if (!Response.StatusInfo.Equals(Response.Status.OK))
				{
					throw GetApiException(Response);
				}
				return Response.readEntity(typeof(FunctionStatus.FunctionInstanceStatus.FunctionInstanceStatusData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.FunctionStats.FunctionInstanceStats.FunctionInstanceStatsData getFunctionStats(String tenant, String namespace, String function, int id) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override FunctionStats.FunctionInstanceStats.FunctionInstanceStatsData GetFunctionStats(string Tenant, string Namespace, string Function, int Id)
		{
			try
			{
				Response Response = Request(functions.path(Tenant).path(Namespace).path(Function).path(Convert.ToString(Id)).path("stats")).get();
				if (!Response.StatusInfo.Equals(Response.Status.OK))
				{
					throw GetApiException(Response);
				}
				return Response.readEntity(typeof(FunctionStats.FunctionInstanceStats.FunctionInstanceStatsData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.FunctionStats getFunctionStats(String tenant, String namespace, String function) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override FunctionStats GetFunctionStats(string Tenant, string Namespace, string Function)
		{
			try
			{
				Response Response = Request(functions.path(Tenant).path(Namespace).path(Function).path("stats")).get();
				if (!Response.StatusInfo.Equals(Response.Status.OK))
				{
					throw GetApiException(Response);
				}
				return Response.readEntity(typeof(FunctionStats));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createFunction(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String fileName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateFunction(FunctionConfig FunctionConfig, string FileName)
		{
			try
			{
				RequestBuilder Builder = post(functions.path(FunctionConfig.Tenant).path(FunctionConfig.Namespace).path(FunctionConfig.Name).Uri.toASCIIString()).addBodyPart(new StringPart("functionConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(FunctionConfig), MediaType.APPLICATION_JSON));

				if (!string.ReferenceEquals(FileName, null) && !FileName.StartsWith("builtin://", StringComparison.Ordinal))
				{
					// If the function code is built in, we don't need to submit here
				   Builder.addBodyPart(new FilePart("data", new File(FileName), MediaType.APPLICATION_OCTET_STREAM));
				}
				org.asynchttpclient.Response Response = asyncHttpClient.executeRequest(AddAuthHeaders(functions, Builder).build()).get();

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
//ORIGINAL LINE: @Override public void createFunctionWithUrl(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String pkgUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateFunctionWithUrl(FunctionConfig FunctionConfig, string PkgUrl)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart Mp = new FormDataMultiPart();

				Mp.bodyPart(new FormDataBodyPart("url", PkgUrl, MediaType.TEXT_PLAIN_TYPE));

				Mp.bodyPart(new FormDataBodyPart("functionConfig", (new Gson()).toJson(FunctionConfig), MediaType.APPLICATION_JSON_TYPE));
				Request(functions.path(FunctionConfig.Tenant).path(FunctionConfig.Namespace).path(FunctionConfig.Name)).post(Entity.entity(Mp, MediaType.MULTIPART_FORM_DATA), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteFunction(String cluster, String namespace, String function) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteFunction(string Cluster, string Namespace, string Function)
		{
			try
			{
				Request(functions.path(Cluster).path(Namespace).path(Function)).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateFunction(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String fileName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateFunction(FunctionConfig FunctionConfig, string FileName)
		{
			UpdateFunction(FunctionConfig, FileName, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateFunction(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String fileName, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws org.apache.pulsar.client.admin.PulsarAdminException
			public override void UpdateFunction(FunctionConfig FunctionConfig, string FileName, UpdateOptions UpdateOptions)
			{
			try
			{
				RequestBuilder Builder = put(functions.path(FunctionConfig.Tenant).path(FunctionConfig.Namespace).path(FunctionConfig.Name).Uri.toASCIIString()).addBodyPart(new StringPart("functionConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(FunctionConfig), MediaType.APPLICATION_JSON));

				if (UpdateOptions != null)
				{
					   Builder.addBodyPart(new StringPart("updateOptions", ObjectMapperFactory.ThreadLocal.writeValueAsString(UpdateOptions), MediaType.APPLICATION_JSON));
				}

				if (!string.ReferenceEquals(FileName, null) && !FileName.StartsWith("builtin://", StringComparison.Ordinal))
				{
					// If the function code is built in, we don't need to submit here
					Builder.addBodyPart(new FilePart("data", new File(FileName), MediaType.APPLICATION_OCTET_STREAM));
				}
				org.asynchttpclient.Response Response = asyncHttpClient.executeRequest(AddAuthHeaders(functions, Builder).build()).get();

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
//ORIGINAL LINE: @Override public void updateFunctionWithUrl(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String pkgUrl, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateFunctionWithUrl(FunctionConfig FunctionConfig, string PkgUrl, UpdateOptions UpdateOptions)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart Mp = new FormDataMultiPart();

				Mp.bodyPart(new FormDataBodyPart("url", PkgUrl, MediaType.TEXT_PLAIN_TYPE));

				Mp.bodyPart(new FormDataBodyPart("functionConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(FunctionConfig), MediaType.APPLICATION_JSON_TYPE));

				if (UpdateOptions != null)
				{
					Mp.bodyPart(new FormDataBodyPart("updateOptions", ObjectMapperFactory.ThreadLocal.writeValueAsString(UpdateOptions), MediaType.APPLICATION_JSON_TYPE));
				}

				Request(functions.path(FunctionConfig.Tenant).path(FunctionConfig.Namespace).path(FunctionConfig.Name)).put(Entity.entity(Mp, MediaType.MULTIPART_FORM_DATA), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateFunctionWithUrl(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String pkgUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateFunctionWithUrl(FunctionConfig FunctionConfig, string PkgUrl)
		{
			UpdateFunctionWithUrl(FunctionConfig, PkgUrl, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public String triggerFunction(String tenant, String namespace, String functionName, String topic, String triggerValue, String triggerFile) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override string TriggerFunction(string Tenant, string Namespace, string FunctionName, string Topic, string TriggerValue, string TriggerFile)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart Mp = new FormDataMultiPart();
				if (!string.ReferenceEquals(TriggerFile, null))
				{
					Mp.bodyPart(new FileDataBodyPart("dataStream", new File(TriggerFile), MediaType.APPLICATION_OCTET_STREAM_TYPE));
				}
				if (!string.ReferenceEquals(TriggerValue, null))
				{
					Mp.bodyPart(new FormDataBodyPart("data", TriggerValue, MediaType.TEXT_PLAIN_TYPE));
				}
				if (!string.ReferenceEquals(Topic, null) && Topic.Length > 0)
				{
					Mp.bodyPart(new FormDataBodyPart("topic", Topic, MediaType.TEXT_PLAIN_TYPE));
				}
				return Request(functions.path(Tenant).path(Namespace).path(FunctionName).path("trigger")).post(Entity.entity(Mp, MediaType.MULTIPART_FORM_DATA), typeof(string));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void restartFunction(String tenant, String namespace, String functionName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void RestartFunction(string Tenant, string Namespace, string FunctionName, int InstanceId)
		{
			try
			{
				Request(functions.path(Tenant).path(Namespace).path(FunctionName).path(Convert.ToString(InstanceId)).path("restart")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void restartFunction(String tenant, String namespace, String functionName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void RestartFunction(string Tenant, string Namespace, string FunctionName)
		{
			try
			{
				Request(functions.path(Tenant).path(Namespace).path(FunctionName).path("restart")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void stopFunction(String tenant, String namespace, String functionName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void StopFunction(string Tenant, string Namespace, string FunctionName, int InstanceId)
		{
			try
			{
				Request(functions.path(Tenant).path(Namespace).path(FunctionName).path(Convert.ToString(InstanceId)).path("stop")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void stopFunction(String tenant, String namespace, String functionName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void StopFunction(string Tenant, string Namespace, string FunctionName)
		{
			try
			{
				Request(functions.path(Tenant).path(Namespace).path(FunctionName).path("stop")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void startFunction(String tenant, String namespace, String functionName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void StartFunction(string Tenant, string Namespace, string FunctionName, int InstanceId)
		{
			try
			{
				Request(functions.path(Tenant).path(Namespace).path(FunctionName).path(Convert.ToString(InstanceId)).path("start")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void startFunction(String tenant, String namespace, String functionName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void StartFunction(string Tenant, string Namespace, string FunctionName)
		{
			try
			{
				Request(functions.path(Tenant).path(Namespace).path(FunctionName).path("start")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void uploadFunction(String sourceFile, String path) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UploadFunction(string SourceFile, string Path)
		{
			try
			{
				RequestBuilder Builder = post(functions.path("upload").Uri.toASCIIString()).addBodyPart(new FilePart("data", new File(SourceFile), MediaType.APPLICATION_OCTET_STREAM)).addBodyPart(new StringPart("path", Path, MediaType.TEXT_PLAIN));

				org.asynchttpclient.Response Response = asyncHttpClient.executeRequest(AddAuthHeaders(functions, Builder).build()).get();
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
//ORIGINAL LINE: @Override public void downloadFunction(String destinationPath, String tenant, String namespace, String functionName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DownloadFunction(string DestinationPath, string Tenant, string Namespace, string FunctionName)
		{
			DownloadFile(DestinationPath, functions.path(Tenant).path(Namespace).path(FunctionName).path("download"));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void downloadFunction(String destinationPath, String path) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DownloadFunction(string DestinationPath, string Path)
		{
			DownloadFile(DestinationPath, functions.path("download").queryParam("path", Path));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void downloadFile(String destinationPath, javax.ws.rs.client.WebTarget target) throws org.apache.pulsar.client.admin.PulsarAdminException
		private void DownloadFile(string DestinationPath, WebTarget Target)
		{
			HttpResponseStatus Status;
			try
			{
				File File = new File(DestinationPath);
				if (!File.exists())
				{
					File.createNewFile();
				}
				FileChannel Os = (new FileStream(DestinationPath, FileMode.Create, FileAccess.Write)).Channel;

				RequestBuilder Builder = get(Target.Uri.toASCIIString());

				Future<HttpResponseStatus> WhenStatusCode = asyncHttpClient.executeRequest(AddAuthHeaders(functions, Builder).build(), new AsyncHandlerAnonymousInnerClass(this, Os));

				Status = WhenStatusCode.get();
				Os.close();

				if (Status.StatusCode < 200 || Status.StatusCode >= 300)
				{
					throw GetApiException(Response.status(Status.StatusCode).entity(Status.StatusText).build());
				}
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		public class AsyncHandlerAnonymousInnerClass : AsyncHandler<HttpResponseStatus>
		{
			private readonly FunctionsImpl outerInstance;

			private FileChannel os;

			public AsyncHandlerAnonymousInnerClass(FunctionsImpl OuterInstance, FileChannel Os)
			{
				this.outerInstance = OuterInstance;
				this.os = Os;
			}

			private HttpResponseStatus status;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public State onStatusReceived(org.asynchttpclient.HttpResponseStatus responseStatus) throws Exception
			public override State onStatusReceived(HttpResponseStatus ResponseStatus)
			{
				status = ResponseStatus;
				if (status.StatusCode != Response.Status.OK.StatusCode)
				{
					return State.ABORT;
				}
				return State.CONTINUE;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public State onHeadersReceived(io.netty.handler.codec.http.HttpHeaders headers) throws Exception
			public override State onHeadersReceived(HttpHeaders Headers)
			{
				return State.CONTINUE;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public State onBodyPartReceived(org.asynchttpclient.HttpResponseBodyPart bodyPart) throws Exception
			public override State onBodyPartReceived(HttpResponseBodyPart BodyPart)
			{

				os.write(BodyPart.BodyByteBuffer);
				return State.CONTINUE;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.asynchttpclient.HttpResponseStatus onCompleted() throws Exception
			public override HttpResponseStatus onCompleted()
			{
				return status;
			}

			public override void onThrowable(Exception T)
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
					Response Response = Request(functions.path("connectors")).get();
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
			private readonly FunctionsImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(FunctionsImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
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
					return Request(functions.path("cluster")).get(new GenericTypeAnonymousInnerClass3(this));
				}
				catch (Exception E)
				{
					throw GetApiException(E);
				}
			}
		}

		public class GenericTypeAnonymousInnerClass3 : GenericType<IList<WorkerInfo>>
		{
			private readonly FunctionsImpl outerInstance;

			public GenericTypeAnonymousInnerClass3(FunctionsImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.functions.FunctionState getFunctionState(String tenant, String namespace, String function, String key) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override FunctionState GetFunctionState(string Tenant, string Namespace, string Function, string Key)
		{
			try
			{
				Response Response = Request(functions.path(Tenant).path(Namespace).path(Function).path("state").path(Key)).get();
				if (!Response.StatusInfo.Equals(Response.Status.OK))
				{
					throw GetApiException(Response);
				}
				return Response.readEntity(typeof(FunctionState));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void putFunctionState(String tenant, String namespace, String function, org.apache.pulsar.common.functions.FunctionState state) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void PutFunctionState(string Tenant, string Namespace, string Function, FunctionState State)
		{
			try
			{
				 RequestBuilder Builder = post(functions.path(Tenant).path(Namespace).path(Function).path("state").path(State.Key).Uri.toASCIIString());
				 Builder.addBodyPart(new StringPart("state", ObjectMapperFactory.ThreadLocal.writeValueAsString(State), MediaType.APPLICATION_JSON));
				 org.asynchttpclient.Response Response = asyncHttpClient.executeRequest(AddAuthHeaders(functions, Builder).build()).get();

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
	}

}