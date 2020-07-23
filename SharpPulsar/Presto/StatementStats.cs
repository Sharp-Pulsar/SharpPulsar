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

using SharpPulsar.Precondition;

namespace SharpPulsar.Presto
{
	public class StatementStats
	{
		public string State {get;}
		private readonly bool queued;
		private readonly bool scheduled;
		public int Nodes {get;}
		public int TotalSplits {get;}
		public int QueuedSplits {get;}
		public int RunningSplits {get;}
		public int CompletedSplits {get;}
		public long CpuTimeMillis {get;}
		public long WallTimeMillis {get;}
		public long QueuedTimeMillis {get;}
		public long ElapsedTimeMillis {get;}
		public long ProcessedRows {get;}
		public long ProcessedBytes {get;}
		public long PeakMemoryBytes {get;}
		public long SpilledBytes {get;}
		public StageStats RootStage {get;}
        public StatementStats(string state, bool queued, bool scheduled, int nodes, int totalSplits, int queuedSplits, int runningSplits, int completedSplits, long cpuTimeMillis, long wallTimeMillis, long queuedTimeMillis, long elapsedTimeMillis, long processedRows, long processedBytes, long peakMemoryBytes, long spilledBytes, StageStats rootStage)
		{
			State = Condition.RequireNonNull(state, "state is null");
			this.queued = queued;
			this.scheduled = scheduled;
			Nodes = nodes;
			TotalSplits = totalSplits;
			QueuedSplits = queuedSplits;
			RunningSplits = runningSplits;
			CompletedSplits = completedSplits;
			CpuTimeMillis = cpuTimeMillis;
			WallTimeMillis = wallTimeMillis;
			QueuedTimeMillis = queuedTimeMillis;
			ElapsedTimeMillis = elapsedTimeMillis;
			ProcessedRows = processedRows;
			ProcessedBytes = processedBytes;
			PeakMemoryBytes = peakMemoryBytes;
			SpilledBytes = spilledBytes;
			RootStage = rootStage;
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

		public virtual bool Scheduled
		{
			get
			{
				return scheduled;
			}
		}

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


		public override string ToString()
		{
			return toStringHelper(this).add("state", State).add("queued", queued).add("scheduled", scheduled).add("nodes", Nodes).add("totalSplits", TotalSplits).add("queuedSplits", QueuedSplits).add("runningSplits", RunningSplits).add("completedSplits", CompletedSplits).add("cpuTimeMillis", CpuTimeMillis).add("wallTimeMillis", WallTimeMillis).add("queuedTimeMillis", QueuedTimeMillis).add("elapsedTimeMillis", ElapsedTimeMillis).add("processedRows", ProcessedRows).add("processedBytes", ProcessedBytes).add("peakMemoryBytes", PeakMemoryBytes).add("spilledBytes", SpilledBytes).add("rootStage", RootStage).ToString();
		}

		public static Builder builder()
		{
			return new Builder();
		}

		public class Builder
		{
			internal string state;
			internal bool queued;
			internal bool scheduled;
			internal int nodes;
			internal int totalSplits;
			internal int queuedSplits;
			internal int runningSplits;
			internal int completedSplits;
			internal long cpuTimeMillis;
			internal long wallTimeMillis;
			internal long queuedTimeMillis;
			internal long elapsedTimeMillis;
			internal long processedRows;
			internal long processedBytes;
			internal long peakMemoryBytes;
			internal long spilledBytes;
			internal StageStats rootStage;

			public Builder()
			{
			}

			public virtual Builder setState(string state)
			{
				this.state = requireNonNull(state, "state is null");
				return this;
			}

			public virtual Builder setNodes(int nodes)
			{
				this.nodes = nodes;
				return this;
			}

			public virtual Builder setQueued(bool queued)
			{
				this.queued = queued;
				return this;
			}

			public virtual Builder setScheduled(bool scheduled)
			{
				this.scheduled = scheduled;
				return this;
			}

			public virtual Builder setTotalSplits(int totalSplits)
			{
				this.totalSplits = totalSplits;
				return this;
			}

			public virtual Builder setQueuedSplits(int queuedSplits)
			{
				this.queuedSplits = queuedSplits;
				return this;
			}

			public virtual Builder setRunningSplits(int runningSplits)
			{
				this.runningSplits = runningSplits;
				return this;
			}

			public virtual Builder setCompletedSplits(int completedSplits)
			{
				this.completedSplits = completedSplits;
				return this;
			}

			public virtual Builder setCpuTimeMillis(long cpuTimeMillis)
			{
				this.cpuTimeMillis = cpuTimeMillis;
				return this;
			}

			public virtual Builder setWallTimeMillis(long wallTimeMillis)
			{
				this.wallTimeMillis = wallTimeMillis;
				return this;
			}

			public virtual Builder setQueuedTimeMillis(long queuedTimeMillis)
			{
				this.queuedTimeMillis = queuedTimeMillis;
				return this;
			}

			public virtual Builder setElapsedTimeMillis(long elapsedTimeMillis)
			{
				this.elapsedTimeMillis = elapsedTimeMillis;
				return this;
			}

			public virtual Builder setProcessedRows(long processedRows)
			{
				this.processedRows = processedRows;
				return this;
			}

			public virtual Builder setProcessedBytes(long processedBytes)
			{
				this.processedBytes = processedBytes;
				return this;
			}

			public virtual Builder setPeakMemoryBytes(long peakMemoryBytes)
			{
				this.peakMemoryBytes = peakMemoryBytes;
				return this;
			}

			public virtual Builder setSpilledBytes(long spilledBytes)
			{
				this.spilledBytes = spilledBytes;
				return this;
			}

			public virtual Builder setRootStage(StageStats rootStage)
			{
				this.rootStage = rootStage;
				return this;
			}

			public virtual StatementStats build()
			{
				return new StatementStats(state, queued, scheduled, nodes, totalSplits, queuedSplits, runningSplits, completedSplits, cpuTimeMillis, wallTimeMillis, queuedTimeMillis, elapsedTimeMillis, processedRows, processedBytes, peakMemoryBytes, spilledBytes, rootStage);
			}
		}
	}

}