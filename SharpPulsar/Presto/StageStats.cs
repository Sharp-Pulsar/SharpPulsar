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

		public StageStats(string stageId, string state, bool done, int nodes, int totalSplits, int queuedSplits, int runningSplits, int completedSplits, long cpuTimeMillis, long wallTimeMillis, long processedRows, long processedBytes, IList<StageStats> subStages)
		{
			this.StageId = stageId;
			this.State = requireNonNull(state, "state is null");
			this.done = done;
			this.Nodes = nodes;
			this.TotalSplits = totalSplits;
			this.QueuedSplits = queuedSplits;
			this.RunningSplits = runningSplits;
			this.CompletedSplits = completedSplits;
			this.CpuTimeMillis = cpuTimeMillis;
			this.WallTimeMillis = wallTimeMillis;
			this.ProcessedRows = processedRows;
			this.ProcessedBytes = processedBytes;
			this.subStages = ImmutableList.copyOf(requireNonNull(subStages, "subStages is null"));
		}

		public virtual bool Done
		{
			get
			{
				return done;
			}
		}

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

		public static Builder builder()
		{
			return new Builder();
		}

		public class Builder
		{
			internal string stageId;
			internal string state;
			internal bool done;
			internal int nodes;
			internal int totalSplits;
			internal int queuedSplits;
			internal int runningSplits;
			internal int completedSplits;
			internal long cpuTimeMillis;
			internal long wallTimeMillis;
			internal long processedRows;
			internal long processedBytes;
			internal IList<StageStats> subStages;

			public Builder()
			{
			}

			public virtual Builder setStageId(string stageId)
			{
				this.stageId = requireNonNull(stageId, "stageId is null");
				return this;
			}

			public virtual Builder setState(string state)
			{
				this.state = requireNonNull(state, "state is null");
				return this;
			}

			public virtual Builder setDone(bool done)
			{
				this.done = done;
				return this;
			}

			public virtual Builder setNodes(int nodes)
			{
				this.nodes = nodes;
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

			public virtual Builder setSubStages(IList<StageStats> subStages)
			{
				this.subStages = ImmutableList.copyOf(requireNonNull(subStages, "subStages is null"));
				return this;
			}

			public virtual StageStats build()
			{
				return new StageStats(stageId, state, done, nodes, totalSplits, queuedSplits, runningSplits, completedSplits, cpuTimeMillis, wallTimeMillis, processedRows, processedBytes, subStages);
			}
		}
	}

}