using System.Text;

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
    /// <summary>
    /// Authentication data.
    /// </summary>
    public class AuthData
	{
        public static byte[] InitAuthDataBytes = Encoding.UTF8.GetBytes("PulsarAuthInit");
        public static byte[] RefreshAuthDataBytes = Encoding.UTF8.GetBytes("PulsarAuthRefresh");


        public static AuthData InitAuthData = Of(InitAuthDataBytes);

        public static AuthData RefreshAuthData = Of(RefreshAuthDataBytes);

        public byte[] Bytes;

		public bool Complete => Bytes == null;
        public static AuthData Of(byte[] authData)
        {
            return new AuthData(authData);
        }
        public AuthData(byte[] authData)
        {
            Bytes = authData;
        }
	}

}