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
namespace Pulsar.Api
{


	using AuthData = org.apache.pulsar.common.api.AuthData;

	/// <summary>
	/// Interface for accessing data which are used in variety of authentication schemes on client side.
	/// </summary>
	public interface AuthenticationDataProvider
	{
		/*
		 * TLS
		 */

		/// <summary>
		/// Check if data for TLS are available.
		/// </summary>
		/// <returns> true if this authentication data contain data for TLS </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default boolean hasDataForTls()
	//	{
	//		return false;
	//	}

		/// 
		/// <returns> a client certificate chain, or null if the data are not available </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default java.security.cert.Certificate[] getTlsCertificates()
	//	{
	//		return null;
	//	}

		/// 
		/// <returns> a private key for the client certificate, or null if the data are not available </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default java.security.PrivateKey getTlsPrivateKey()
	//	{
	//		return null;
	//	}

		/*
		 * HTTP
		 */

		/// <summary>
		/// Check if data for HTTP are available.
		/// </summary>
		/// <returns> true if this authentication data contain data for HTTP </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default boolean hasDataForHttp()
	//	{
	//		return false;
	//	}

		/// 
		/// <returns> a authentication scheme, or {@code null} if the request will not be authenticated. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default String getHttpAuthType()
	//	{
	//		return null;
	//	}

		/// 
		/// <returns> an enumeration of all the header names </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default java.util.Set<java.util.Map.Entry<String, String>> getHttpHeaders() throws Exception
	//	{
	//		return null;
	//	}

		/*
		 * Command
		 */

		/// <summary>
		/// Check if data from Pulsar protocol are available.
		/// </summary>
		/// <returns> true if this authentication data contain data from Pulsar protocol </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default boolean hasDataFromCommand()
	//	{
	//		return false;
	//	}

		/// 
		/// <returns> authentication data which will be stored in a command </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default String getCommandData()
	//	{
	//		return null;
	//	}

		/// <summary>
		/// For mutual authentication, This method use passed in `data` to evaluate and challenge,
		/// then returns null if authentication has completed;
		/// returns authenticated data back to server side, if authentication has not completed.
		/// 
		/// <para>Mainly used for mutual authentication like sasl.
		/// </para>
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default org.apache.pulsar.common.api.AuthData authenticate(org.apache.pulsar.common.api.AuthData data) throws javax.naming.AuthenticationException
	//	{
	//		byte[] bytes = (hasDataFromCommand() ? this.getCommandData() : "").getBytes(UTF_8);
	//		return AuthData.of(bytes);
	//	}
	}

}