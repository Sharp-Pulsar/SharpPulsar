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
namespace org.apache.pulsar.common.util
{

	/// <summary>
	/// Holder for the secure key store.
	/// </summary>
	/// <seealso cref= java.security.KeyStore </seealso>
	public class KeyStoreHolder
	{

		private KeyStore keyStore = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public KeyStoreHolder() throws java.security.KeyStoreException
		public KeyStoreHolder()
		{
			try
			{
				keyStore = KeyStore.getInstance(KeyStore.DefaultType);
				keyStore.load(null, null);
			}
			catch (Exception e) when (e is GeneralSecurityException || e is IOException)
			{
				throw new KeyStoreException("KeyStore creation error", e);
			}
		}

		public virtual KeyStore KeyStore
		{
			get
			{
				return keyStore;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setCertificate(String alias, java.security.cert.Certificate certificate) throws java.security.KeyStoreException
		public virtual void setCertificate(string alias, Certificate certificate)
		{
			try
			{
				keyStore.setCertificateEntry(alias, certificate);
			}
			catch (GeneralSecurityException e)
			{
				throw new KeyStoreException("Failed to set the certificate", e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setPrivateKey(String alias, java.security.PrivateKey privateKey, java.security.cert.Certificate[] certChain) throws java.security.KeyStoreException
		public virtual void setPrivateKey(string alias, PrivateKey privateKey, Certificate[] certChain)
		{
			try
			{
				keyStore.setKeyEntry(alias, privateKey, "".ToCharArray(), certChain);
			}
			catch (GeneralSecurityException e)
			{
				throw new KeyStoreException("Failed to set the private key", e);
			}
		}

	}

}