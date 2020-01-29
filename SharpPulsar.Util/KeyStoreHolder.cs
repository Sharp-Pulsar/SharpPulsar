﻿using System;
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
namespace SharpPulsar.Util
{

	/// <summary>
	/// Holder for the secure key store.
	/// </summary>
	/// <seealso cref= java.security.KeyStore </seealso>
	public class KeyStoreHolder
	{

		private KeyStore keyStore = null;
		public KeyStoreHolder()
		{
			try
			{
				keyStore = KeyStore.getInstance(KeyStore.DefaultType);
				keyStore.load(null, null);
			}
			catch (Exception e) when (e is SecurityException || e is IOException)
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

		public virtual void SetCertificate(string alias, Certificate certificate)
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

		public virtual void SetPrivateKey(string alias, PrivateKey privateKey, Certificate[] certChain)
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