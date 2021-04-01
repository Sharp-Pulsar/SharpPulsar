using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpPulsar.Presto.Facebook.Type;

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
	public interface IStatementClient:IDisposable
	{
		string Query {get;}

		TimeZoneKey TimeZone {get;}

		bool Running {get;}

		bool ClientAborted {get;}

		bool ClientError {get;}

		bool Finished {get;}

		StatementStats Stats {get;}

		IQueryStatusInfo CurrentStatusInfo();

		IQueryData CurrentData();

		IQueryStatusInfo FinalStatusInfo();

		string SetCatalog {get;}
        
        string SetSchema {get; }

		string SetPath { get; }

		IDictionary<string, string> SetSessionProperties {get;}

		ISet<string> ResetSessionProperties {get;}

		IDictionary<string, SelectedRole> SetRoles {get;}

		IDictionary<string, string> AddedPreparedStatements {get;}

		ISet<string> DeallocatedPreparedStatements {get;}

		string StartedTransactionId {get;}

		bool ClearTransactionId {get;}

		ValueTask<bool> Advance();

		void CancelLeafStage();

		void Close();
	}

}