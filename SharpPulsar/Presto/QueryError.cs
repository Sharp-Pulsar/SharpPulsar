/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Text.Json.Serialization;
using SharpPulsar.Presto.Facebook.Type;

namespace SharpPulsar.Presto
{
	public class QueryError
	{
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("sqlState")]
        public string SqlState { get; set;}
        [JsonPropertyName("errorCode")]
        public int ErrorCode { get; set; }
        [JsonPropertyName("errorName")]
        public string ErrorName { get; set; }
        [JsonPropertyName("errorType")]
        public string ErrorType { get; set; }
        [JsonPropertyName("errorLocation")]
        public ErrorLocation ErrorLocation { get; set; }
        [JsonPropertyName("failureInfo")]
        public FailureInfo FailureInfo { get; set; }
        
		public override string ToString()
		{
			return StringHelper.Build(this).Add("message", Message).Add("sqlState", SqlState).Add("errorCode", ErrorCode).Add("errorName", ErrorName).Add("errorType", ErrorType).Add("errorLocation", ErrorLocation).Add("failureInfo", FailureInfo).ToString();
		}
	}

}