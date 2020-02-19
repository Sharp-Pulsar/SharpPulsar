using System;
using System.Collections.Generic;
using SharpPulsar.Api;

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
namespace SharpPulsar.Test.Impl.auth
{

    [Serializable]
	public class MockEncodedAuthenticationParameterSupport : IAuthentication, IEncodedAuthenticationParameterSupport
	{
		public IDictionary<string, string> AuthParamsMap = new Dictionary<string, string>();

		public void Configure(string authParams)
		{
			var @params = authParams.Split(";", true);
			foreach (var p in @params)
			{
				var kv = p.Split(":", true);
				if (kv.Length == 2)
				{
					AuthParamsMap[kv[0]] = kv[1];
				}
			}
		}

		public string AuthMethodName => null;

        public IAuthenticationDataProvider AuthData => null;

        public void Configure(IDictionary<string, string> authParams)
		{

		}

		public void Start()
		{

		}

		public void Close()
		{

		}

        public void Dispose()
        {
            Close();
        }
    }

}