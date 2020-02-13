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
	using SinkStatus = Org.Apache.Pulsar.Common.Policies.Data.SinkStatus;
	using SinkConfig = Org.Apache.Pulsar.Common.Io.SinkConfig;
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
//ORIGINAL LINE: @Slf4j public class SinksImpl extends ComponentResource implements org.apache.pulsar.client.admin.Sinks, org.apache.pulsar.client.admin.Sink
	public class SinksImpl : ComponentResource, Sinks, Sink
	{

		private readonly WebTarget sink;
		private readonly AsyncHttpClient asyncHttpClient;

		public SinksImpl(WebTarget Web, Authentication Auth, AsyncHttpClient AsyncHttpClient, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
			this.sink = Web.path("/admin/v3/sink");
			this.asyncHttpClient = AsyncHttpClient;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<String> listSinks(String tenant, String namespace) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<string> ListSinks(string Tenant, string Namespace)
		{
			try
			{
				Response Response = Request(sink.path(Tenant).path(Namespace)).get();
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
			private readonly SinksImpl outerInstance;

			public GenericTypeAnonymousInnerClass(SinksImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.io.SinkConfig getSink(String tenant, String namespace, String sinkName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override SinkConfig GetSink(string Tenant, string Namespace, string SinkName)
		{
			try
			{
				 Response Response = Request(sink.path(Tenant).path(Namespace).path(SinkName)).get();
				if (!Response.StatusInfo.Equals(Response.Status.OK))
				{
					throw GetApiException(Response);
				}
				return Response.readEntity(typeof(SinkConfig));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SinkStatus getSinkStatus(String tenant, String namespace, String sinkName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override SinkStatus GetSinkStatus(string Tenant, string Namespace, string SinkName)
		{
			try
			{
				Response Response = Request(sink.path(Tenant).path(Namespace).path(SinkName).path("status")).get();
				if (!Response.StatusInfo.Equals(Response.Status.OK))
				{
					throw GetApiException(Response);
				}
				return Response.readEntity(typeof(SinkStatus));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.policies.data.SinkStatus.SinkInstanceStatus.SinkInstanceStatusData getSinkStatus(String tenant, String namespace, String sinkName, int id) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override SinkStatus.SinkInstanceStatus.SinkInstanceStatusData GetSinkStatus(string Tenant, string Namespace, string SinkName, int Id)
		{
			try
			{
				Response Response = Request(sink.path(Tenant).path(Namespace).path(SinkName).path(Convert.ToString(Id)).path("status")).get();
				if (!Response.StatusInfo.Equals(Response.Status.OK))
				{
					throw GetApiException(Response);
				}
				return Response.readEntity(typeof(SinkStatus.SinkInstanceStatus.SinkInstanceStatusData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createSink(org.apache.pulsar.common.io.SinkConfig sinkConfig, String fileName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateSink(SinkConfig SinkConfig, string FileName)
		{
			try
			{
				RequestBuilder Builder = post(sink.path(SinkConfig.Tenant).path(SinkConfig.Namespace).path(SinkConfig.Name).Uri.toASCIIString()).addBodyPart(new StringPart("sinkConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(SinkConfig), MediaType.APPLICATION_JSON));

				if (!string.ReferenceEquals(FileName, null) && !FileName.StartsWith("builtin://", StringComparison.Ordinal))
				{
					// If the function code is built in, we don't need to submit here
					Builder.addBodyPart(new FilePart("data", new File(FileName), MediaType.APPLICATION_OCTET_STREAM));
				}
				org.asynchttpclient.Response Response = asyncHttpClient.executeRequest(AddAuthHeaders(sink, Builder).build()).get();

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
//ORIGINAL LINE: @Override public void createSinkWithUrl(org.apache.pulsar.common.io.SinkConfig sinkConfig, String pkgUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateSinkWithUrl(SinkConfig SinkConfig, string PkgUrl)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart Mp = new FormDataMultiPart();

				Mp.bodyPart(new FormDataBodyPart("url", PkgUrl, MediaType.TEXT_PLAIN_TYPE));

				Mp.bodyPart(new FormDataBodyPart("sinkConfig", (new Gson()).toJson(SinkConfig), MediaType.APPLICATION_JSON_TYPE));
				Request(sink.path(SinkConfig.Tenant).path(SinkConfig.Namespace).path(SinkConfig.Name)).post(Entity.entity(Mp, MediaType.MULTIPART_FORM_DATA), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteSink(String cluster, String namespace, String function) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteSink(string Cluster, string Namespace, string Function)
		{
			try
			{
				Request(sink.path(Cluster).path(Namespace).path(Function)).delete(typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSink(org.apache.pulsar.common.io.SinkConfig sinkConfig, String fileName, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateSink(SinkConfig SinkConfig, string FileName, UpdateOptions UpdateOptions)
		{
			try
			{
				RequestBuilder Builder = put(sink.path(SinkConfig.Tenant).path(SinkConfig.Namespace).path(SinkConfig.Name).Uri.toASCIIString()).addBodyPart(new StringPart("sinkConfig", ObjectMapperFactory.ThreadLocal.writeValueAsString(SinkConfig), MediaType.APPLICATION_JSON));

				if (UpdateOptions != null)
				{
					Builder.addBodyPart(new StringPart("updateOptions", ObjectMapperFactory.ThreadLocal.writeValueAsString(UpdateOptions), MediaType.APPLICATION_JSON));
				}

				if (!string.ReferenceEquals(FileName, null) && !FileName.StartsWith("builtin://", StringComparison.Ordinal))
				{
					// If the function code is built in, we don't need to submit here
					Builder.addBodyPart(new FilePart("data", new File(FileName), MediaType.APPLICATION_OCTET_STREAM));
				}
				org.asynchttpclient.Response Response = asyncHttpClient.executeRequest(AddAuthHeaders(sink, Builder).build()).get();

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
//ORIGINAL LINE: @Override public void updateSink(org.apache.pulsar.common.io.SinkConfig sinkConfig, String fileName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateSink(SinkConfig SinkConfig, string FileName)
		{
		   UpdateSink(SinkConfig, FileName, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSinkWithUrl(org.apache.pulsar.common.io.SinkConfig sinkConfig, String pkgUrl, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateSinkWithUrl(SinkConfig SinkConfig, string PkgUrl, UpdateOptions UpdateOptions)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.glassfish.jersey.media.multipart.FormDataMultiPart mp = new org.glassfish.jersey.media.multipart.FormDataMultiPart();
				FormDataMultiPart Mp = new FormDataMultiPart();

				Mp.bodyPart(new FormDataBodyPart("url", PkgUrl, MediaType.TEXT_PLAIN_TYPE));

				Mp.bodyPart(new FormDataBodyPart("sinkConfig", (new Gson()).toJson(SinkConfig), MediaType.APPLICATION_JSON_TYPE));

				if (UpdateOptions != null)
				{
					Mp.bodyPart(new FormDataBodyPart("updateOptions", ObjectMapperFactory.ThreadLocal.writeValueAsString(UpdateOptions), MediaType.APPLICATION_JSON_TYPE));
				}

				Request(sink.path(SinkConfig.Tenant).path(SinkConfig.Namespace).path(SinkConfig.Name)).put(Entity.entity(Mp, MediaType.MULTIPART_FORM_DATA), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateSinkWithUrl(org.apache.pulsar.common.io.SinkConfig sinkConfig, String pkgUrl) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void UpdateSinkWithUrl(SinkConfig SinkConfig, string PkgUrl)
		{
			UpdateSinkWithUrl(SinkConfig, PkgUrl, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void restartSink(String tenant, String namespace, String functionName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void RestartSink(string Tenant, string Namespace, string FunctionName, int InstanceId)
		{
			try
			{
				Request(sink.path(Tenant).path(Namespace).path(FunctionName).path(Convert.ToString(InstanceId)).path("restart")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void restartSink(String tenant, String namespace, String functionName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void RestartSink(string Tenant, string Namespace, string FunctionName)
		{
			try
			{
				Request(sink.path(Tenant).path(Namespace).path(FunctionName).path("restart")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void stopSink(String tenant, String namespace, String sinkName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void StopSink(string Tenant, string Namespace, string SinkName, int InstanceId)
		{
			try
			{
				Request(sink.path(Tenant).path(Namespace).path(SinkName).path(Convert.ToString(InstanceId)).path("stop")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void stopSink(String tenant, String namespace, String sinkName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void StopSink(string Tenant, string Namespace, string SinkName)
		{
			try
			{
				Request(sink.path(Tenant).path(Namespace).path(SinkName).path("stop")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void startSink(String tenant, String namespace, String sinkName, int instanceId) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void StartSink(string Tenant, string Namespace, string SinkName, int InstanceId)
		{
			try
			{
				Request(sink.path(Tenant).path(Namespace).path(SinkName).path(Convert.ToString(InstanceId)).path("start")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void startSink(String tenant, String namespace, String sinkName) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void StartSink(string Tenant, string Namespace, string SinkName)
		{
			try
			{
				Request(sink.path(Tenant).path(Namespace).path(SinkName).path("start")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
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
					Response Response = Request(sink.path("builtinsinks")).get();
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
			private readonly SinksImpl outerInstance;

			public GenericTypeAnonymousInnerClass2(SinksImpl OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void reloadBuiltInSinks() throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void ReloadBuiltInSinks()
		{
			try
			{
				Request(sink.path("reloadBuiltInSinks")).post(Entity.entity("", MediaType.APPLICATION_JSON), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}
	}

}