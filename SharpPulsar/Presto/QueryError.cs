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
namespace SharpPulsar.Presto
{
	public class QueryError
	{
        public string Message { get;}
        public string SqlState { get;}
        public int ErrorCode { get; }
        public string ErrorName { get;}
        public string ErrorType { get;}
        public ErrorLocation ErrorLocation { get;}
        public FailureInfo FailureInfo { get;}

        public QueryError(string message, string sqlState, int errorCode, string errorName, string errorType, ErrorLocation errorLocation, FailureInfo failureInfo)
		{
			Message = message;
			SqlState = sqlState;
			ErrorCode = errorCode;
			ErrorName = errorName;
			ErrorType = errorType;
			ErrorLocation = errorLocation;
			FailureInfo = failureInfo;
		}


		public override string ToString()
		{
			return toStringHelper(this).add("message", Message).add("sqlState", SqlState).add("errorCode", ErrorCode).add("errorName", ErrorName).add("errorType", ErrorType).add("errorLocation", ErrorLocation).add("failureInfo", FailureInfo).ToString();
		}
	}

}