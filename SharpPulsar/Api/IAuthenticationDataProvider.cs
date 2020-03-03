using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SharpPulsar.Shared.Auth;

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
namespace SharpPulsar.Api
{
	
	/// <summary>
	/// Interface for accessing data which are used in variety of authentication schemes on client side.
	/// </summary>
	public interface IAuthenticationDataProvider
	{
		/*
		 * TLS
		 */

		/// <summary>
		/// Check if data for TLS are available.
		/// </summary>
		/// <returns> true if this authentication data contain data for TLS </returns>
		virtual bool HasDataForTls()
		{
			return false;
		}

		/// 
		/// <returns> a client certificate chain, or null if the data are not available </returns>
		virtual X509Certificate2[] TlsCertificates => null;

        /// 
		/// <returns> a private key for the client certificate, or null if the data are not available </returns>
		virtual AsymmetricAlgorithm TlsPrivateKey => null;

        /*
		 * HTTP
		 */

		/// <summary>
		/// Check if data for HTTP are available.
		/// </summary>
		/// <returns> true if this authentication data contain data for HTTP </returns>
		virtual bool HasDataForHttp()
		{
			return false;
		}

		/// 
		/// <returns> a authentication scheme, or {@code null} if the request will not be authenticated. </returns>
		virtual string HttpAuthType => null;

        /// 
		/// <returns> an enumeration of all the header names </returns>
		virtual ISet<KeyValuePair<string, string>> HttpHeaders => null;

        /*
		 * Command
		 */

		/// <summary>
		/// Check if data from Pulsar protocol are available.
		/// </summary>
		/// <returns> true if this authentication data contain data from Pulsar protocol </returns>
		virtual bool HasDataFromCommand()
		{
			return false;
		}

		/// 
		/// <returns> authentication data which will be stored in a command </returns>
		virtual string CommandData => null;

        /// <summary>
		/// For mutual authentication, This method use passed in `data` to evaluate and challenge,
		/// then returns null if authentication has completed;
		/// returns authenticated data back to server side, if authentication has not completed.
		/// 
		/// <para>Mainly used for mutual authentication like sasl.
		/// </para>
		/// </summary>
		virtual AuthDataShared Authenticate(AuthDataShared auth)
		{
            var bytes = (sbyte[])(object)Encoding.UTF8.GetBytes((HasDataFromCommand() ? CommandData : ""));
			return new AuthDataShared(bytes);
        }
	}

}