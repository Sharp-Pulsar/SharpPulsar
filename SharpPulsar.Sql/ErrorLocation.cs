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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.MoreObjects.toStringHelper;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Immutable public class ErrorLocation
	public class ErrorLocation
	{
		public virtual LineNumber {get;}
		public virtual ColumnNumber {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonCreator public ErrorLocation(@JsonProperty("lineNumber") int lineNumber, @JsonProperty("columnNumber") int columnNumber)
		public ErrorLocation(int LineNumber, int ColumnNumber)
		{
			checkArgument(LineNumber >= 1, "lineNumber must be at least one");
			checkArgument(ColumnNumber >= 1, "columnNumber must be at least one");

			this.LineNumber = LineNumber;
			this.ColumnNumber = ColumnNumber;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public int getLineNumber()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public int getColumnNumber()

		public override string ToString()
		{
			return toStringHelper(this).add("lineNumber", LineNumber).add("columnNumber", ColumnNumber).ToString();
		}
	}

}