using System;
using System.Collections.Generic;

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
	using ImmutableList = com.google.common.collect.ImmutableList;



//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Immutable public class FailureInfo
	public class FailureInfo
	{
		private static readonly Pattern STACK_TRACE_PATTERN = Pattern.compile(@"(.*)\.(.*)\(([^:]*)(?::(.*))?\)");

		public virtual Type {get;}
		public virtual Message {get;}
		public virtual Cause {get;}
		private readonly IList<FailureInfo> suppressed;
		private readonly IList<string> stack;
		public virtual ErrorLocation {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonCreator public FailureInfo(@JsonProperty("type") String type, @JsonProperty("message") String message, @JsonProperty("cause") FailureInfo cause, @JsonProperty("suppressed") java.util.List<FailureInfo> suppressed, @JsonProperty("stack") java.util.List<String> stack, @JsonProperty("errorLocation") @Nullable ErrorLocation errorLocation)
		public FailureInfo(string Type, string Message, FailureInfo Cause, IList<FailureInfo> Suppressed, IList<string> System.Collections.Stack, ErrorLocation ErrorLocation)
		{
			requireNonNull(Type, "type is null");
			requireNonNull(Suppressed, "suppressed is null");
			requireNonNull(Stack, "stack is null");

			this.Type = Type;
			this.Message = Message;
			this.Cause = Cause;
			this.suppressed = ImmutableList.copyOf(Suppressed);
			this.stack = ImmutableList.copyOf(Stack);
			this.ErrorLocation = ErrorLocation;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public String getType()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty public String getMessage()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty public FailureInfo getCause()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public java.util.List<FailureInfo> getSuppressed()
		public virtual IList<FailureInfo> Suppressed
		{
			get
			{
				return suppressed;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public java.util.List<String> getStack()
		public virtual IList<string> Stack
		{
			get
			{
				return stack;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty public ErrorLocation getErrorLocation()

		public virtual Exception ToException()
		{
			return ToException(this);
		}

		private static FailureException ToException(FailureInfo FailureInfo)
		{
			if (FailureInfo == null)
			{
				return null;
			}
			FailureException Failure = new FailureException(FailureInfo.Type, FailureInfo.Message, ToException(FailureInfo.Cause));
			foreach (FailureInfo Suppressed in FailureInfo.Suppressed)
			{
				Failure.addSuppressed(ToException(Suppressed));
			}
			ImmutableList.Builder<StackTraceElement> StackTraceBuilder = ImmutableList.builder();
			foreach (string Stack in FailureInfo.Stack)
			{
				StackTraceBuilder.add(ToStackTraceElement(Stack));
			}
			ImmutableList<StackTraceElement> StackTrace = StackTraceBuilder.build();
			Failure.StackTrace = StackTrace.toArray(new StackTraceElement[StackTrace.size()]);
			return Failure;
		}

		public static StackTraceElement ToStackTraceElement(string Stack)
		{
			Matcher Matcher = STACK_TRACE_PATTERN.matcher(Stack);
			if (Matcher.matches())
			{
				string DeclaringClass = Matcher.group(1);
				string MethodName = Matcher.group(2);
				string FileName = Matcher.group(3);
				int Number = -1;
				if (FileName.Equals("Native Method"))
				{
					FileName = null;
					Number = -2;
				}
				else if (Matcher.group(4) != null)
				{
					Number = int.Parse(Matcher.group(4));
				}
				return new StackTraceElement(DeclaringClass, MethodName, FileName, Number);
			}
			return new StackTraceElement("Unknown", Stack, null, -1);
		}

		public class FailureException : Exception
		{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal readonly string TypeConflict;

			public FailureException(string Type, string Message, FailureException Cause) : base(Message, Cause)
			{
				this.TypeConflict = requireNonNull(Type, "type is null");
			}


			public override string ToString()
			{
				string Message = outerInstance.Message;
				if (!string.ReferenceEquals(Message, null))
				{
					return TypeConflict + ": " + Message;
				}
				return TypeConflict;
			}
		}
	}

}