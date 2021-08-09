using System;
using System.Security.Cryptography.X509Certificates;
using SharpPulsar.Interfaces;

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
namespace SharpPulsar.Auth
{


    public class AuthenticationDataTls : IAuthenticationDataProvider
	{
        public AuthenticationDataTls(string certPfxFilePath, string password)
        {
            if (string.IsNullOrWhiteSpace(certPfxFilePath))
            {
                throw new ArgumentException("certFilePath must not be null");
            }
            var collection = new X509Certificate2Collection();

            collection.Import(certPfxFilePath, password, X509KeyStorageFlags.PersistKeySet);

            TlsCertificates = collection;
            //_tlsPrivateKey = cert.GetRSAPrivateKey();
        }

		public bool HasDataForTls()
		{
			return true;
		}

		public X509Certificate2Collection TlsCertificates { get; }
    }

}