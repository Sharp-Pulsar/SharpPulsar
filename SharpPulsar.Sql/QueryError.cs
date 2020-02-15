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
namespace SharpPulsar.Sql
{
	using JsonCreator = com.fasterxml.jackson.annotation.JsonCreator;
	using JsonProperty = com.fasterxml.jackson.annotation.JsonProperty;


	public class QueryError
	{
		public virtual Message {get;}
		public virtual SqlState {get;}
		public virtual ErrorCode {get;}
		public virtual ErrorName {get;}
		public virtual ErrorType {get;}
		public virtual ErrorLocation {get;}
		public virtual FailureInfo {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonCreator public QueryError(@JsonProperty("message") String message, @JsonProperty("sqlState") String sqlState, @JsonProperty("errorCode") int errorCode, @JsonProperty("errorName") String errorName, @JsonProperty("errorType") String errorType, @JsonProperty("errorLocation") ErrorLocation errorLocation, @JsonProperty("failureInfo") FailureInfo failureInfo)
		public QueryError(string Message, string SqlState, int ErrorCode, string ErrorName, string ErrorType, ErrorLocation ErrorLocation, FailureInfo FailureInfo)
		{
			this.Message = Message;
			this.SqlState = SqlState;
			this.ErrorCode = ErrorCode;
			this.ErrorName = ErrorName;
			this.ErrorType = ErrorType;
			this.ErrorLocation = ErrorLocation;
			this.FailureInfo = FailureInfo;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public String getMessage()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty public String getSqlState()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public int getErrorCode()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public String getErrorName()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public String getErrorType()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty public ErrorLocation getErrorLocation()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty public FailureInfo getFailureInfo()

		public override string ToString()
		{
			return toStringHelper(this).add("message", Message).add("sqlState", SqlState).add("errorCode", ErrorCode).add("errorName", ErrorName).add("errorType", ErrorType).add("errorLocation", ErrorLocation).add("failureInfo", FailureInfo).ToString();
		}
	}

}