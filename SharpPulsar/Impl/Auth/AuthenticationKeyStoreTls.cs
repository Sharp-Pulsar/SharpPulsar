
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
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Api;
using SharpPulsar.Exceptions;

namespace SharpPulsar.Impl.Auth
{
    /// <summary>
    /// This plugin requires these parameters: keyStoreType, keyStorePath, and  keyStorePassword.
    /// This parameter will construct a AuthenticationDataProvider
    /// </summary>
    [Serializable]
    public class AuthenticationKeyStoreTls : IAuthentication, IEncodedAuthenticationParameterSupport
    {
        private const long SerialVersionUid = 1L;

        public string AuthMethodName => "tls";

        // parameter name
        public const string KeystoreType = "keyStoreType";
        public const string KeystorePath = "keyStorePath";
        public const string KeystorePw = "keyStorePassword";
        private const string DefaultKeystoreType = "JKS";
        [NonSerialized]
        private KeyStoreParams _keyStoreParams;

        public AuthenticationKeyStoreTls()
        {
        }

        public AuthenticationKeyStoreTls(string keyStoreType, string keyStorePath, string keyStorePassword)
        {
            _keyStoreParams = new KeyStoreParams
            {
                KeyStoreType = keyStoreType,
                KeyStorePath = keyStorePath,
                KeyStorePassword = keyStorePassword
            };
        }

        public virtual void Dispose()
        {
            // noop
        }

        public virtual IAuthenticationDataProvider AuthData
        {
            get
            {
                try
                {
                    return new AuthenticationDataKeyStoreTls(this._keyStoreParams);
                }
                catch (Exception e)
                {
                    throw new PulsarClientException(e.ToString());
                }
            }
        }



        // passed in KEYSTORE_TYPE/KEYSTORE_PATH/KEYSTORE_PW to construct parameters.
        // e.g. {"keyStoreType":"JKS","keyStorePath":"/path/to/keystorefile","keyStorePassword":"keystorepw"}
        //  or: "keyStoreType":"JKS","keyStorePath":"/path/to/keystorefile","keyStorePassword":"keystorepw"
        public virtual void Configure(string paramsString)
        {
            IDictionary<string, string> @params = null;
            try
            {
                @params = AuthenticationUtil.ConfigureFromJsonString(paramsString);
            }
            catch (Exception)
            {
                // auth-param is not in json format
                //log.info("parameter not in Json format: {}", paramsString);
            }

            // in ":" "," format.
            @params = (@params == null || @params.Count == 0)
                ? AuthenticationUtil.ConfigureFromPulsar1AuthParamString(paramsString)
                : @params;

            Configure(@params);
        }

        public virtual void Configure(IDictionary<string, string> @params)
        {
            var keyStoreType = @params[KeystoreType];
            var keyStorePath = @params[KeystorePath];
            var keyStorePassword = @params[KeystorePw];

            if (string.IsNullOrWhiteSpace(keyStorePath) || string.IsNullOrWhiteSpace(keyStorePassword))
            {
                throw new ArgumentException("Passed in parameter empty. " + KeystorePath + ": " + keyStorePath + " " +
                                            KeystorePw + ": " + keyStorePassword);
            }

            if (string.IsNullOrWhiteSpace(keyStoreType))
            {
                keyStoreType = DefaultKeystoreType;
            }

            _keyStoreParams = new KeyStoreParams
            {
                KeyStoreType = keyStoreType,
                KeyStorePath = keyStorePath,
                KeyStorePassword = keyStorePassword
            };
        }

        public virtual void Start()
        {
            // noop
        }

        // return strings like : "key1":"value1", "key2":"value2", ...
        public static string MapToString(IDictionary<string, string> map)
        {
            return string.Join(",", map.ToList());
        }
    }
}
