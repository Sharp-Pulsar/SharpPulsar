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
	public sealed class PrestoHeaders
	{
		public const string PrestoUser = "X-Presto-User";
		public const string PrestoSource = "X-Presto-Source";
		public const string PrestoCatalog = "X-Presto-Catalog";
		public const string PrestoSchema = "X-Presto-Schema";
		public const string PrestoTimeZone = "X-Presto-Time-Zone";
		public const string PrestoLanguage = "X-Presto-Language";
		public const string PrestoTraceToken = "X-Presto-Trace-Token";
		public const string PrestoSession = "X-Presto-Session";
		public const string PrestoSetCatalog = "X-Presto-Set-Catalog";
		public const string PrestoSetSchema = "X-Presto-Set-Schema";
		public const string PrestoSetSession = "X-Presto-Set-Session";
		public const string PrestoClearSession = "X-Presto-Clear-Session";
		public const string PrestoSetRole = "X-Presto-Set-Role";
		public const string PrestoRole = "X-Presto-Role";
		public const string PrestoPreparedStatement = "X-Presto-Prepared-Statement";
		public const string PrestoAddedPrepare = "X-Presto-Added-Prepare";
		public const string PrestoDeallocatedPrepare = "X-Presto-Deallocated-Prepare";
		public const string PrestoTransactionId = "X-Presto-Transaction-Id";
		public const string PrestoStartedTransactionId = "X-Presto-Started-Transaction-Id";
		public const string PrestoClearTransactionId = "X-Presto-Clear-Transaction-Id";
		public const string PrestoClientInfo = "X-Presto-Client-Info";
		public const string PrestoClientTags = "X-Presto-Client-Tags";
		public const string PrestoResourceEstimate = "X-Presto-Resource-Estimate";
		public const string PrestoExtraCredential = "X-Presto-Extra-Credential";

		public const string PrestoCurrentState = "X-Presto-Current-State";
		public const string PrestoMaxWait = "X-Presto-Max-Wait";
		public const string PrestoMaxSize = "X-Presto-Max-Size";
		public const string PrestoTaskInstanceId = "X-Presto-Task-Instance-Id";
		public const string PrestoPageToken = "X-Presto-Page-Sequence-Id";
		public const string PrestoPageNextToken = "X-Presto-Page-End-Sequence-Id";
		public const string PrestoBufferComplete = "X-Presto-Buffer-Complete";

		private PrestoHeaders()
		{
		}
	}

}