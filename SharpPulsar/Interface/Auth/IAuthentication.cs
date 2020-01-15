using SharpPulsar.Exception;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SharpPulsar.Exception.PulsarClientException;

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
namespace SharpPulsar.Interface.Auth
{

	/// <summary>
	/// Interface of authentication providers.
	/// </summary>
	public interface IAuthentication : IAsyncDisposable
	{

		/// <returns> the identifier for this authentication method </returns>
		string AuthMethodName {get;}

		/// 
		/// <returns> The authentication data identifying this client that will be sent to the broker </returns>
		/// <exception cref="PulsarClientException.GettingAuthenticationDataException">
		///             if there was error getting the authentication data to use </exception>
		/// <exception cref="PulsarClientException">
		///             any other error </exception>
		///             
		IAuthenticationDataProvider GetAuthData()
		{
			throw new UnsupportedAuthenticationException("Method not implemented!");
		}

		/// <summary>
		/// Get/Create an authentication data provider which provides the data that this client will be sent to the broker.
		/// Some authentication method need to auth between each client channel. So it need the broker, who it will talk to.
		/// </summary>
		/// <param name="brokerHostName">
		///          target broker host name
		/// </param>
		/// <returns> The authentication data provider </returns>
		IAuthenticationDataProvider GetAuthData(string brokerHostName)
		{
			if (string.IsNullOrWhiteSpace(brokerHostName))
				throw new PulsarClientException("Broker Host Name Cannot be Empty");
			return this.GetAuthData();
	    }

		/// <summary>
		/// Configure the authentication plugins with the supplied parameters.
		/// </summary>
		/// <param name="authParams"> </param>
		/// @deprecated This method will be deleted on version 2.0, instead please use configure(String
		///             encodedAuthParamString) which is in EncodedAuthenticationParameterSupport for now and will be
		///             integrated into this interface. 
		
		void Configure(string authParams);

		/// <summary>
		/// Initialize the authentication provider.
		/// </summary>
		void Start();

		/// <summary>
		/// An authentication Stage.
		/// when authentication complete, passed-in authFuture will contains authentication related http request headers.
		/// </summary>
		void AuthenticationStage(string requestUrl, IAuthenticationDataProvider authData, IDictionary<string, string> previousResHeaders, Task<IDictionary<string, string>> authFuture)
		{
			authFuture.complete(null);
		}

		/// <summary>
		/// Add an authenticationStage that will complete along with authFuture.
		/// </summary>
		ISet<IDictionary<string, string>> NewRequestHeader(string hostName, IAuthenticationDataProvider authData, IDictionary<string, string> previousResHeaders)
		{
			return authData.GetHttpHeaders();
		}

	}

}