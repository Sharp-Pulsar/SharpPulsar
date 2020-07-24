using System.Collections.Generic;
using System.Text.RegularExpressions;
using SharpPulsar.Precondition;

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
	public class FailureInfo
	{
		private static readonly Regex StackTracePattern = new Regex(@"(.*)\.(.*)\(([^:]*)(?::(.*))?\)");

		public string Type {get;}
		public string Message {get;}
		public FailureInfo Cause {get;}
        private ErrorLocation _errorLocation;
		public FailureInfo(string type, string message, FailureInfo cause, IList<FailureInfo> suppressed, IList<string> stack, ErrorLocation errorLocation)
		{
			Condition.RequireNonNull(type, "type", "type is null");
            Condition.RequireNonNull(suppressed, "suppressed", "suppressed is null");
            Condition.RequireNonNull(stack, "stack", "stack is null");

			Type = type;
			Message = message;
			Cause = cause;
			Suppressed = new List<FailureInfo>(suppressed);
			Stack = new List<string>(stack);
			_errorLocation = errorLocation;
		}

		public virtual IList<FailureInfo> Suppressed { get; }

		public virtual IList<string> Stack { get; }

		
	}
}