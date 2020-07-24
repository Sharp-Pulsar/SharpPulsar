using System;
using System.Collections.Generic;
using System.Linq;
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

	public class QueryResults : QueryStatusInfo, IQueryData
    {
		public IList<Column> Columns { get; set; }

        public IEnumerable<IList<object>> Data { get; set; }

        public IList<PrestoWarning> Warnings { get; set; }

        public long? UpdateCount { get; set; }

        public string Id { get; set; }

        public Uri InfoUri { get; set; }

        public Uri PartialCancelUri { get; set; }

        public Uri NextUri { get; set; }

        public StatementStats Stats { get; set; }

        public QueryError Error { get; set; }

        public string UpdateType { get; set; }

        public override string ToString()
		{
			return StringHelper.Build(this).Add("id", Id).Add("infoUri", InfoUri).Add("partialCancelUri", PartialCancelUri).Add("nextUri", NextUri).Add("columns", Columns).Add("hasData", Data != null && Data.Any()).Add("stats", Stats).Add("error", Error).Add("updateType", UpdateType).Add("updateCount", UpdateCount).ToString();
		}
	}

}