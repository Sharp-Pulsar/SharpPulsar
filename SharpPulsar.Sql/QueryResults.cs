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
	
	public class QueryResults : QueryStatusInfo, QueryData
	{
		public virtual Id {get;}
		public virtual InfoUri {get;}
		public virtual PartialCancelUri {get;}
		public virtual NextUri {get;}
		private readonly IList<Column> columns;
		private readonly IEnumerable<IList<object>> data;
		public virtual Stats {get;}
		public virtual Exception {get;}
		private readonly IList<PrestoWarning> warnings;
		public virtual UpdateType {get;}
		private readonly long? updateCount;

        public QueryResults(string Id, URI InfoUri, URI PartialCancelUri, URI NextUri, IList<Column> Columns, IList<IList<object>> Data, StatementStats Stats, QueryError Error, IList<PrestoWarning> Warnings, string UpdateType, long? UpdateCount) : this(Id, InfoUri, PartialCancelUri, NextUri, Columns, fixData(Columns, Data), Stats, Error, firstNonNull(Warnings, ImmutableList.of()), UpdateType, UpdateCount)
		{
		}

		public QueryResults(string Id, URI InfoUri, URI PartialCancelUri, URI NextUri, IList<Column> Columns, IEnumerable<IList<object>> Data, StatementStats Stats, QueryError Error, IList<PrestoWarning> Warnings, string UpdateType, long? UpdateCount)
		{
			this.Id = requireNonNull(Id, "id is null");
			this.InfoUri = requireNonNull(InfoUri, "infoUri is null");
			this.PartialCancelUri = PartialCancelUri;
			this.NextUri = NextUri;
			this.columns = (Columns != null) ? ImmutableList.copyOf(Columns) : null;
			this.data = (Data != null) ? unmodifiableIterable(Data) : null;
			checkArgument(Data == null || Columns != null, "data present without columns");
			this.Stats = requireNonNull(Stats, "stats is null");
			this.Error = Error;
			this.warnings = ImmutableList.copyOf(requireNonNull(Warnings, "warnings is null"));
			this.UpdateType = UpdateType;
			this.updateCount = UpdateCount;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty @Override public String getId()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty @Override public java.net.URI getInfoUri()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty @Override public java.net.URI getPartialCancelUri()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty @Override public java.net.URI getNextUri()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty @Override public java.util.List<Column> getColumns()
		public virtual IList<Column> Columns
		{
			get
			{
				return columns;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty @Override public Iterable<java.util.List<Object>> getData()
		public virtual IEnumerable<IList<object>> Data
		{
			get
			{
				return data;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty @Override public StatementStats getStats()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty @Override public QueryError getError()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty @Override public java.util.List<com.facebook.presto.spi.PrestoWarning> getWarnings()
		public virtual IList<PrestoWarning> Warnings
		{
			get
			{
				return warnings;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty @Override public String getUpdateType()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty @Override public System.Nullable<long> getUpdateCount()
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