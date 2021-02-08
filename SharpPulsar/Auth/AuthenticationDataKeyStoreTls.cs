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
using System;
using SharpPulsar.Api;
using SharpPulsar.Common;
using SharpPulsar.Interfaces;

namespace SharpPulsar.Auth
{
	[Serializable]
    public class AuthenticationDataKeyStoreTls : IAuthenticationDataProvider
    {
        [NonSerialized]
        private readonly KeyStoreParams _keyStoreParams;

        public AuthenticationDataKeyStoreTls(KeyStoreParams keyStoreParams)
        {
            _keyStoreParams = keyStoreParams;
        }

        /*
         * TLS
         */
        public virtual bool HasDataForTls()
        {
            return true;
        }

        public virtual KeyStoreParams TlsKeyStoreParams => _keyStoreParams;

        public virtual string CommandData => null;
    }
}
