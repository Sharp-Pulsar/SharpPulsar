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


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.MoreObjects.toStringHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Immutable public class StageStats
	public class StageStats
	{
		public virtual StageId {get;}
		public virtual State {get;}
		private readonly bool done;
		public virtual Nodes {get;}
		public virtual TotalSplits {get;}
		public virtual QueuedSplits {get;}
		public virtual RunningSplits {get;}
		public virtual CompletedSplits {get;}
		public virtual CpuTimeMillis {get;}
		public virtual WallTimeMillis {get;}
		public virtual ProcessedRows {get;}
		public virtual ProcessedBytes {get;}
		private readonly IList<StageStats> subStages;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonCreator public StageStats(@JsonProperty("stageId") String stageId, @JsonProperty("state") String state, @JsonProperty("done") boolean done, @JsonProperty("nodes") int nodes, @JsonProperty("totalSplits") int totalSplits, @JsonProperty("queuedSplits") int queuedSplits, @JsonProperty("runningSplits") int runningSplits, @JsonProperty("completedSplits") int completedSplits, @JsonProperty("cpuTimeMillis") long cpuTimeMillis, @JsonProperty("wallTimeMillis") long wallTimeMillis, @JsonProperty("processedRows") long processedRows, @JsonProperty("processedBytes") long processedBytes, @JsonProperty("subStages") java.util.List<StageStats> subStages)
		public StageStats(string StageId, string State, bool Done, int Nodes, int TotalSplits, int QueuedSplits, int RunningSplits, int CompletedSplits, long CpuTimeMillis, long WallTimeMillis, long ProcessedRows, long ProcessedBytes, IList<StageStats> SubStages)
		{
			this.StageId = StageId;
			this.State = requireNonNull(State, "state is null");
			this.done = Done;
			this.Nodes = Nodes;
			this.TotalSplits = TotalSplits;
			this.QueuedSplits = QueuedSplits;
			this.RunningSplits = RunningSplits;
			this.CompletedSplits = CompletedSplits;
			this.CpuTimeMillis = CpuTimeMillis;
			this.WallTimeMillis = WallTimeMillis;
			this.ProcessedRows = ProcessedRows;
			this.ProcessedBytes = ProcessedBytes;
			this.subStages = ImmutableList.copyOf(requireNonNull(SubStages, "subStages is null"));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public String getStageId()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public String getState()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public boolean isDone()
		public virtual bool Done
		{
			get
			{
				return done;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public int getNodes()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public int getTotalSplits()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public int getQueuedSplits()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public int getRunningSplits()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public int getCompletedSplits()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public long getCpuTimeMillis()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public long getWallTimeMillis()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public long getProcessedRows()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public long getProcessedBytes()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public java.util.List<StageStats> getSubStages()
		public virtual IList<StageStats> SubStages
		{
			get
			{
				return subStages;
			}
		}

		public override string ToString()
		{
			return toStringHelper(this).add("state", State).add("done", done).add("nodes", Nodes).add("totalSplits", TotalSplits).add("queuedSplits", QueuedSplits).add("runningSplits", RunningSplits).add("completedSplits", CompletedSplits).add("cpuTimeMillis", CpuTimeMillis).add("wallTimeMillis", WallTimeMillis).add("processedRows", ProcessedRows).add("processedBytes", ProcessedBytes).add("subStages", subStages).ToString();
		}

		public static Builder Builder()
		{
			return new Builder();
		}

		public class Builder
		{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal string StageIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal string StateConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal bool DoneConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal int NodesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal int TotalSplitsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal int QueuedSplitsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal int RunningSplitsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal int CompletedSplitsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long CpuTimeMillisConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long WallTimeMillisConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long ProcessedRowsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long ProcessedBytesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal IList<StageStats> SubStagesConflict;

			public Builder()
			{
			}

			public virtual Builder setStageId(string StageId)
			{
				this.StageIdConflict = requireNonNull(StageId, "stageId is null");
				return this;
			}

			public virtual Builder setState(string State)
			{
				this.StateConflict = requireNonNull(State, "state is null");
				return this;
			}

			public virtual Builder setDone(bool Done)
			{
				this.DoneConflict = Done;
				return this;
			}

			public virtual Builder setNodes(int Nodes)
			{
				this.NodesConflict = Nodes;
				return this;
			}

			public virtual Builder setTotalSplits(int TotalSplits)
			{
				this.TotalSplitsConflict = TotalSplits;
				return this;
			}

			public virtual Builder setQueuedSplits(int QueuedSplits)
			{
				this.QueuedSplitsConflict = QueuedSplits;
				return this;
			}

			public virtual Builder setRunningSplits(int RunningSplits)
			{
				this.RunningSplitsConflict = RunningSplits;
				return this;
			}

			public virtual Builder setCompletedSplits(int CompletedSplits)
			{
				this.CompletedSplitsConflict = CompletedSplits;
				return this;
			}

			public virtual Builder setCpuTimeMillis(long CpuTimeMillis)
			{
				this.CpuTimeMillisConflict = CpuTimeMillis;
				return this;
			}

			public virtual Builder setWallTimeMillis(long WallTimeMillis)
			{
				this.WallTimeMillisConflict = WallTimeMillis;
				return this;
			}

			public virtual Builder setProcessedRows(long ProcessedRows)
			{
				this.ProcessedRowsConflict = ProcessedRows;
				return this;
			}

			public virtual Builder setProcessedBytes(long ProcessedBytes)
			{
				this.ProcessedBytesConflict = ProcessedBytes;
				return this;
			}

			public virtual Builder setSubStages(IList<StageStats> SubStages)
			{
				this.SubStagesConflict = ImmutableList.copyOf(requireNonNull(SubStages, "subStages is null"));
				return this;
			}

			public virtual StageStats Build()
			{
				return new StageStats(StageIdConflict, StateConflict, DoneConflict, NodesConflict, TotalSplitsConflict, QueuedSplitsConflict, RunningSplitsConflict, CompletedSplitsConflict, CpuTimeMillisConflict, WallTimeMillisConflict, ProcessedRowsConflict, ProcessedBytesConflict, SubStagesConflict);
			}
		}
	}

}