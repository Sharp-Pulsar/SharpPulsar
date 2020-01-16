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
namespace org.apache.pulsar.client.admin
{

	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using ErrorData = org.apache.pulsar.common.policies.data.ErrorData;
	using ObjectMapperFactory = org.apache.pulsar.common.util.ObjectMapperFactory;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") @Slf4j public class PulsarAdminException extends Exception
	public class PulsarAdminException : Exception
	{
		private const int DEFAULT_STATUS_CODE = 500;

		private readonly string httpError;
		private readonly int statusCode;

		private static string getReasonFromServer(WebApplicationException e)
		{
			try
			{
				return e.Response.readEntity(typeof(ErrorData)).reason;
			}
			catch (Exception)
			{
				try
				{
					return ObjectMapperFactory.ThreadLocal.readValue(e.Response.Entity.ToString(), typeof(ErrorData)).reason;
				}
				catch (Exception)
				{
					try
					{
						return ObjectMapperFactory.ThreadLocal.readValue(e.Message, typeof(ErrorData)).reason;
					}
					catch (Exception)
					{
						// could not parse output to ErrorData class
						return e.Message;
					}
				}
			}
		}

		public PulsarAdminException(ClientErrorException e) : base(getReasonFromServer(e), e)
		{
			this.httpError = getReasonFromServer(e);
			this.statusCode = e.Response.Status;
		}

		public PulsarAdminException(ClientErrorException e, string message) : base(message, e)
		{
			this.httpError = getReasonFromServer(e);
			this.statusCode = e.Response.Status;
		}

		public PulsarAdminException(ServerErrorException e) : base(getReasonFromServer(e), e)
		{
			this.httpError = getReasonFromServer(e);
			this.statusCode = e.Response.Status;
		}

		public PulsarAdminException(ServerErrorException e, string message) : base(message, e)
		{
			this.httpError = getReasonFromServer(e);
			this.statusCode = e.Response.Status;
		}

		public PulsarAdminException(Exception t) : base(t)
		{
			httpError = null;
			statusCode = DEFAULT_STATUS_CODE;
		}

		public PulsarAdminException(WebApplicationException e) : base(getReasonFromServer(e), e)
		{
			this.httpError = getReasonFromServer(e);
			this.statusCode = e.Response.Status;
		}

		public PulsarAdminException(string message, Exception t) : base(message, t)
		{
			httpError = null;
			statusCode = DEFAULT_STATUS_CODE;
		}

		public PulsarAdminException(string message) : base(message)
		{
			httpError = null;
			statusCode = DEFAULT_STATUS_CODE;
		}

		public virtual string HttpError
		{
			get
			{
				return httpError;
			}
		}

		public virtual int StatusCode
		{
			get
			{
				return statusCode;
			}
		}

		public class NotAuthorizedException : PulsarAdminException
		{
			public NotAuthorizedException(ClientErrorException e) : base(e)
			{
			}
		}

		public class NotFoundException : PulsarAdminException
		{
			public NotFoundException(ClientErrorException e) : base(e)
			{
			}
		}

		public class NotAllowedException : PulsarAdminException
		{
			public NotAllowedException(ClientErrorException e) : base(e)
			{
			}
		}

		public class ConflictException : PulsarAdminException
		{
			public ConflictException(ClientErrorException e) : base(e)
			{
			}
		}

		public class PreconditionFailedException : PulsarAdminException
		{
			public PreconditionFailedException(ClientErrorException e) : base(e)
			{
			}
		}
		public class TimeoutException : PulsarAdminException
		{
			public TimeoutException(Exception t) : base(t)
			{
			}
		}

		public class ServerSideErrorException : PulsarAdminException
		{
			public ServerSideErrorException(ServerErrorException e, string msg) : base(e, msg)
			{
			}

			public ServerSideErrorException(ServerErrorException e) : base(e, "Some error occourred on the server")
			{
			}
		}

		public class HttpErrorException : PulsarAdminException
		{
			public HttpErrorException(Exception e) : base(e)
			{
			}

			public HttpErrorException(Exception t) : base(t)
			{
			}
		}

		public class ConnectException : PulsarAdminException
		{
			public ConnectException(Exception t) : base(t)
			{
			}

			public ConnectException(string message, Exception t) : base(message, t)
			{
			}
		}

		public class GettingAuthenticationDataException : PulsarAdminException
		{
			public GettingAuthenticationDataException(Exception t) : base(t)
			{
			}

			public GettingAuthenticationDataException(string msg) : base(msg)
			{
			}
		}
	}

}