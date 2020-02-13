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
//	import static Math.min;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Immutable public class StatementStats
	public class StatementStats
	{
		public virtual State {get;}
		private readonly bool queued;
		private readonly bool scheduled;
		public virtual Nodes {get;}
		public virtual TotalSplits {get;}
		public virtual QueuedSplits {get;}
		public virtual RunningSplits {get;}
		public virtual CompletedSplits {get;}
		public virtual CpuTimeMillis {get;}
		public virtual WallTimeMillis {get;}
		public virtual QueuedTimeMillis {get;}
		public virtual ElapsedTimeMillis {get;}
		public virtual ProcessedRows {get;}
		public virtual ProcessedBytes {get;}
		public virtual PeakMemoryBytes {get;}
		public virtual SpilledBytes {get;}
		public virtual RootStage {get;}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonCreator public StatementStats(@JsonProperty("state") String state, @JsonProperty("queued") boolean queued, @JsonProperty("scheduled") boolean scheduled, @JsonProperty("nodes") int nodes, @JsonProperty("totalSplits") int totalSplits, @JsonProperty("queuedSplits") int queuedSplits, @JsonProperty("runningSplits") int runningSplits, @JsonProperty("completedSplits") int completedSplits, @JsonProperty("cpuTimeMillis") long cpuTimeMillis, @JsonProperty("wallTimeMillis") long wallTimeMillis, @JsonProperty("queuedTimeMillis") long queuedTimeMillis, @JsonProperty("elapsedTimeMillis") long elapsedTimeMillis, @JsonProperty("processedRows") long processedRows, @JsonProperty("processedBytes") long processedBytes, @JsonProperty("peakMemoryBytes") long peakMemoryBytes, @JsonProperty("spilledBytes") long spilledBytes, @JsonProperty("rootStage") StageStats rootStage)
		public StatementStats(string State, bool Queued, bool Scheduled, int Nodes, int TotalSplits, int QueuedSplits, int RunningSplits, int CompletedSplits, long CpuTimeMillis, long WallTimeMillis, long QueuedTimeMillis, long ElapsedTimeMillis, long ProcessedRows, long ProcessedBytes, long PeakMemoryBytes, long SpilledBytes, StageStats RootStage)
		{
			this.State = requireNonNull(State, "state is null");
			this.queued = Queued;
			this.scheduled = Scheduled;
			this.Nodes = Nodes;
			this.TotalSplits = TotalSplits;
			this.QueuedSplits = QueuedSplits;
			this.RunningSplits = RunningSplits;
			this.CompletedSplits = CompletedSplits;
			this.CpuTimeMillis = CpuTimeMillis;
			this.WallTimeMillis = WallTimeMillis;
			this.QueuedTimeMillis = QueuedTimeMillis;
			this.ElapsedTimeMillis = ElapsedTimeMillis;
			this.ProcessedRows = ProcessedRows;
			this.ProcessedBytes = ProcessedBytes;
			this.PeakMemoryBytes = PeakMemoryBytes;
			this.SpilledBytes = SpilledBytes;
			this.RootStage = RootStage;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public String getState()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public boolean isQueued()
		public virtual bool Queued
		{
			get
			{
				return queued;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public boolean isScheduled()
		public virtual bool Scheduled
		{
			get
			{
				return scheduled;
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
//ORIGINAL LINE: @JsonProperty public long getQueuedTimeMillis()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public long getElapsedTimeMillis()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public long getProcessedRows()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public long getProcessedBytes()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public long getPeakMemoryBytes()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable @JsonProperty public StageStats getRootStage()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public java.util.OptionalDouble getProgressPercentage()
		public virtual double? ProgressPercentage
		{
			get
			{
				if (!scheduled || TotalSplits == 0)
				{
					return double?.empty();
				}
				return double?.of(min(100, (CompletedSplits * 100.0) / TotalSplits));
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonProperty public long getSpilledBytes()

		public override string ToString()
		{
			return toStringHelper(this).add("state", State).add("queued", queued).add("scheduled", scheduled).add("nodes", Nodes).add("totalSplits", TotalSplits).add("queuedSplits", QueuedSplits).add("runningSplits", RunningSplits).add("completedSplits", CompletedSplits).add("cpuTimeMillis", CpuTimeMillis).add("wallTimeMillis", WallTimeMillis).add("queuedTimeMillis", QueuedTimeMillis).add("elapsedTimeMillis", ElapsedTimeMillis).add("processedRows", ProcessedRows).add("processedBytes", ProcessedBytes).add("peakMemoryBytes", PeakMemoryBytes).add("spilledBytes", SpilledBytes).add("rootStage", RootStage).ToString();
		}

		public static Builder Builder()
		{
			return new Builder();
		}

		public class Builder
		{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal string StateConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal bool QueuedConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal bool ScheduledConflict;
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
			internal long QueuedTimeMillisConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long ElapsedTimeMillisConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long ProcessedRowsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long ProcessedBytesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long PeakMemoryBytesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal long SpilledBytesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal StageStats RootStageConflict;

			public Builder()
			{
			}

			public virtual Builder setState(string State)
			{
				this.StateConflict = requireNonNull(State, "state is null");
				return this;
			}

			public virtual Builder setNodes(int Nodes)
			{
				this.NodesConflict = Nodes;
				return this;
			}

			public virtual Builder setQueued(bool Queued)
			{
				this.QueuedConflict = Queued;
				return this;
			}

			public virtual Builder setScheduled(bool Scheduled)
			{
				this.ScheduledConflict = Scheduled;
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

			public virtual Builder setQueuedTimeMillis(long QueuedTimeMillis)
			{
				this.QueuedTimeMillisConflict = QueuedTimeMillis;
				return this;
			}

			public virtual Builder setElapsedTimeMillis(long ElapsedTimeMillis)
			{
				this.ElapsedTimeMillisConflict = ElapsedTimeMillis;
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

			public virtual Builder setPeakMemoryBytes(long PeakMemoryBytes)
			{
				this.PeakMemoryBytesConflict = PeakMemoryBytes;
				return this;
			}

			public virtual Builder setSpilledBytes(long SpilledBytes)
			{
				this.SpilledBytesConflict = SpilledBytes;
				return this;
			}

			public virtual Builder setRootStage(StageStats RootStage)
			{
				this.RootStageConflict = RootStage;
				return this;
			}

			public virtual StatementStats Build()
			{
				return new StatementStats(StateConflict, QueuedConflict, ScheduledConflict, NodesConflict, TotalSplitsConflict, QueuedSplitsConflict, RunningSplitsConflict, CompletedSplitsConflict, CpuTimeMillisConflict, WallTimeMillisConflict, QueuedTimeMillisConflict, ElapsedTimeMillisConflict, ProcessedRowsConflict, ProcessedBytesConflict, PeakMemoryBytesConflict, SpilledBytesConflict, RootStageConflict);
			}
		}
	}

}