
using System.Collections.Generic;
using SharpPulsar.API;
using SharpPulsar.Shared;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
namespace SharpPulsar.Common
{

    /// <summary>
    /// An empty implement. Doesn't provide any public key or private key, and just returns `null`.
    /// </summary>
    public class DummyCryptoKeyReaderImpl : ICryptoKeyReader
    {

        public static readonly DummyCryptoKeyReaderImpl INSTANCE = new DummyCryptoKeyReaderImpl();

        private DummyCryptoKeyReaderImpl()
        {
        }

        public EncryptionKeyInfo GetPublicKey(string keyName, IDictionary<string, string> metadata)
        {
            return null;
        }

        public EncryptionKeyInfo GetPrivateKey(string keyName, IDictionary<string, string> metadata)
        {
            return null;
        }
    }
}
