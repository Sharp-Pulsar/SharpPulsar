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
namespace SharpPulsar.Presto
{

	public class QueryResults : QueryStatusInfo, QueryData
    {
        public string _id;
        public Uri InfoUri { get; set; }
        public Uri PartialCancelUri { get; set; }
        public Uri NextUri { get; set; }
        public List<Column> Columns { get; set; }
        public List<List<object>> Data { get; set; }
        public StatementStats Stats { get; set; }
        public QueryError Error { get; set; }
        public List<PrestoWarning> Warnings { get; set; }
        public string UpdateType { get; set; }
        public long UpdateCount { get; set; }

		public QueryResults(string id, Uri infoUri, Uri partialCancelUri, Uri nextUri, IList<Column> columns, IList<IList<object>> data, StatementStats stats, QueryError error, IList<PrestoWarning> warnings, string updateType, long? updateCount) : this(id, infoUri, partialCancelUri, nextUri, columns, data, stats, error, warnings, updateType, updateCount)
		{
		}

		public QueryResults(string id, Uri infoUri, Uri partialCancelUri, Uri nextUri, IList<Column> columns, IEnumerable<IList<object>> data, StatementStats stats, QueryError error, IList<PrestoWarning> warnings, string updateType, long? updateCount)
		{
			this.Id = requireNonNull(id, "id is null");
			this.InfoUri = requireNonNull(infoUri, "infoUri is null");
			this.PartialCancelUri = partialCancelUri;
			this.NextUri = nextUri;
			this.columns = (columns != null) ? ImmutableList.copyOf(columns) : null;
			this.data = (data != null) ? unmodifiableIterable(data) : null;
			checkArgument(data == null || columns != null, "data present without columns");
			this.Stats = requireNonNull(stats, "stats is null");
			this.Error = error;
			this.warnings = ImmutableList.copyOf(requireNonNull(warnings, "warnings is null"));
			this.UpdateType = updateType;
			this.updateCount = updateCount;
		}

		public virtual IList<Column> Columns
		{
			get
			{
				return columns;
			}
		}

		public virtual IEnumerable<IList<object>> Data
		{
			get
			{
				return data;
			}
		}

		public virtual IList<PrestoWarning> Warnings
		{
			get
			{
				return warnings;
			}
		}

		public virtual long? UpdateCount
		{
			get
			{
				return updateCount;
			}
		}
		public virtual long? UpdateCount
		{
			get
			{
				return updateCount;
			}
		}

		public override string ToString()
		{
			return toStringHelper(this).add("id", Id).add("infoUri", InfoUri).add("partialCancelUri", PartialCancelUri).add("nextUri", NextUri).add("columns", columns).add("hasData", data != null).add("stats", Stats).add("error", Exception).add("updateType", UpdateType).add("updateCount", updateCount).ToString();
		}
	}

}