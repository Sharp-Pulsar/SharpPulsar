using System.Text.Json;
using System.Text.Json.Serialization;
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
namespace SharpPulsar.Auth.OAuth2
{
	/// <summary>
	/// A JSON object representing a credentials file.
	/// </summary>
	public class KeyFile
	{
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }


        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; }

        [JsonPropertyName("client_email")]
        public string ClientEmail { get; set; }

        [JsonPropertyName("issuer_url")]
        public string IssuerUrl { get; set; }

        public virtual string ToJson()
		{
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
		}
		public static KeyFile FromJson(string value)
		{
            return JsonSerializer.Deserialize<KeyFile>(value);
		}
	}

}