using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SharpPulsar.Sql.Precondition;

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
	public class FailureInfo
	{
		private static readonly Regex StackTracePattern = new Regex(@"(.*)\.(.*)\(([^:]*)(?::(.*))?\)");

		public string Type {get;}
		public string Message {get;}
		public FailureInfo Cause {get;}
        private ErrorLocation _errorLocation;
		public FailureInfo(string type, string message, FailureInfo cause, IList<FailureInfo> suppressed, IList<string> stack, ErrorLocation errorLocation)
		{
			ParameterCondition.RequireNonNull(type, "type", "type is null");
            ParameterCondition.RequireNonNull(suppressed, "suppressed", "suppressed is null");
            ParameterCondition.RequireNonNull(stack, "stack", "stack is null");

			this.Type = type;
			this.Message = message;
			this.Cause = cause;
			this.Suppressed = new List<FailureInfo>(suppressed);
			this.Stack = new List<string>(stack);
			this._errorLocation = errorLocation;
		}

		public virtual IList<FailureInfo> Suppressed { get; }

		public virtual IList<string> Stack { get; }

		public virtual Exception ToException()
		{
			return ToException(this);
		}

		private static FailureException ToException(FailureInfo failureInfo)
		{
			if (failureInfo == null)
			{
				return null;
			}
			FailureException failure = new FailureException(failureInfo.Type, failureInfo.Message, ToException(failureInfo.Cause));
			foreach (FailureInfo suppressed in failureInfo.Suppressed)
			{
				failure.AddSuppressed(ToException(suppressed));
			}
			ImmutableList.Builder<StackTraceElement> stackTraceBuilder = ImmutableList.builder();
			foreach (string stack in failureInfo.Stack)
			{
				stackTraceBuilder.add(ToStackTraceElement(stack));
			}
			ImmutableList<StackTraceElement> stackTrace = stackTraceBuilder.build();
			failure.StackTrace = stackTrace.toArray(new StackTraceElement[stackTrace.size()]);
			return failure;
		}

		public static StackTraceElement ToStackTraceElement(string stack)
		{
			Matcher matcher = StackTracePattern.matcher(stack);
			if (matcher.matches())
			{
				string declaringClass = matcher.group(1);
				string methodName = matcher.group(2);
				string fileName = matcher.group(3);
				int number = -1;
				if (fileName.Equals("Native Method"))
				{
					fileName = null;
					number = -2;
				}
				else if (matcher.group(4) != null)
				{
					number = int.Parse(matcher.group(4));
				}
				return new StackTraceElement(declaringClass, methodName, fileName, number);
			}
			return new StackTraceElement("Unknown", stack, null, -1);
		}

		public class FailureException : Exception
		{
			internal readonly string Type;

			public FailureException(string type, string message, FailureException cause) : base(message, cause)
			{
				this.Type = ParameterCondition.RequireNonNull(type, "type","type is null");
			}


			public override string ToString()
			{
				string message = outerInstance.Message;
				if (!string.ReferenceEquals(message, null))
				{
					return Type + ": " + message;
				}
				return Type;
			}
		}
	}

}