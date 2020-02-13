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
namespace Org.Apache.Pulsar.Client.Admin
{

	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using ErrorData = Org.Apache.Pulsar.Common.Policies.Data.ErrorData;
	using ObjectMapperFactory = Org.Apache.Pulsar.Common.Util.ObjectMapperFactory;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") @Slf4j public class PulsarAdminException extends Exception
	public class PulsarAdminException : Exception
	{
		private const int DefaultStatusCode = 500;

		public virtual HttpError {get;}
		public virtual StatusCode {get;}

		private static string GetReasonFromServer(WebApplicationException E)
		{
			try
			{
				return E.Response.readEntity(typeof(ErrorData)).reason;
			}
			catch (Exception)
			{
				try
				{
					return ObjectMapperFactory.ThreadLocal.readValue(E.Response.Entity.ToString(), typeof(ErrorData)).reason;
				}
				catch (Exception)
				{
					try
					{
						return ObjectMapperFactory.ThreadLocal.readValue(E.Message, typeof(ErrorData)).reason;
					}
					catch (Exception)
					{
						// could not parse output to ErrorData class
						return E.Message;
					}
				}
			}
		}

		public PulsarAdminException(ClientErrorException E) : base(GetReasonFromServer(E), E)
		{
			this.HttpError = GetReasonFromServer(E);
			this.StatusCode = E.Response.Status;
		}

		public PulsarAdminException(ClientErrorException E, string Message) : base(Message, E)
		{
			this.HttpError = GetReasonFromServer(E);
			this.StatusCode = E.Response.Status;
		}

		public PulsarAdminException(ServerErrorException E) : base(GetReasonFromServer(E), E)
		{
			this.HttpError = GetReasonFromServer(E);
			this.StatusCode = E.Response.Status;
		}

		public PulsarAdminException(ServerErrorException E, string Message) : base(Message, E)
		{
			this.HttpError = GetReasonFromServer(E);
			this.StatusCode = E.Response.Status;
		}

		public PulsarAdminException(Exception T) : base(T)
		{
			HttpError = null;
			StatusCode = DefaultStatusCode;
		}

		public PulsarAdminException(WebApplicationException E) : base(GetReasonFromServer(E), E)
		{
			this.HttpError = GetReasonFromServer(E);
			this.StatusCode = E.Response.Status;
		}

		public PulsarAdminException(string Message, Exception T) : base(Message, T)
		{
			HttpError = null;
			StatusCode = DefaultStatusCode;
		}

		public PulsarAdminException(string Message) : base(Message)
		{
			HttpError = null;
			StatusCode = DefaultStatusCode;
		}



		public class NotAuthorizedException : PulsarAdminException
		{
			public NotAuthorizedException(ClientErrorException E) : base(E)
			{
			}
		}

		public class NotFoundException : PulsarAdminException
		{
			public NotFoundException(ClientErrorException E) : base(E)
			{
			}
		}

		public class NotAllowedException : PulsarAdminException
		{
			public NotAllowedException(ClientErrorException E) : base(E)
			{
			}
		}

		public class ConflictException : PulsarAdminException
		{
			public ConflictException(ClientErrorException E) : base(E)
			{
			}
		}

		public class PreconditionFailedException : PulsarAdminException
		{
			public PreconditionFailedException(ClientErrorException E) : base(E)
			{
			}
		}
		public class TimeoutException : PulsarAdminException
		{
			public TimeoutException(Exception T) : base(T)
			{
			}
		}

		public class ServerSideErrorException : PulsarAdminException
		{
			public ServerSideErrorException(ServerErrorException E, string Msg) : base(E, Msg)
			{
			}

			public ServerSideErrorException(ServerErrorException E) : base(E, "Some error occourred on the server")
			{
			}
		}

		public class HttpErrorException : PulsarAdminException
		{
			public HttpErrorException(Exception E) : base(E)
			{
			}

			public HttpErrorException(Exception T) : base(T)
			{
			}
		}

		public class ConnectException : PulsarAdminException
		{
			public ConnectException(Exception T) : base(T)
			{
			}

			public ConnectException(string Message, Exception T) : base(Message, T)
			{
			}
		}

		public class GettingAuthenticationDataException : PulsarAdminException
		{
			public GettingAuthenticationDataException(Exception T) : base(T)
			{
			}

			public GettingAuthenticationDataException(string Msg) : base(Msg)
			{
			}
		}
	}

}