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
using SharpPulsar.Presto.Facebook.Type;

namespace SharpPulsar.Presto
{
	public class StatementStats
	{
		public string State {get; set; }
		public bool Queued { get; set; }
		public bool Scheduled { get; set; }
		public int Nodes { get; set; }
		public int TotalSplits { get; set; }
		public int QueuedSplits { get; set; }
		public int RunningSplits { get; set; }
		public int CompletedSplits { get; set; }
		public long CpuTimeMillis { get; set; }
		public long WallTimeMillis { get; set; }
		public long QueuedTimeMillis { get; set; }
		public long ElapsedTimeMillis { get; set; }
		public long ProcessedRows { get; set; }
		public long ProcessedBytes { get; set; }
		public long PeakMemoryBytes { get; set; }
		public long SpilledBytes { get; set; }
		public StageStats RootStage { get; set; }

		public override string ToString()
		{
			return StringHelper.Build(this).Add("state", State).Add("queued", Queued).Add("scheduled", Scheduled).Add("nodes", Nodes).Add("totalSplits", TotalSplits).Add("queuedSplits", QueuedSplits).Add("runningSplits", RunningSplits).Add("completedSplits", CompletedSplits).Add("cpuTimeMillis", CpuTimeMillis).Add("wallTimeMillis", WallTimeMillis).Add("queuedTimeMillis", QueuedTimeMillis).Add("elapsedTimeMillis", ElapsedTimeMillis).Add("processedRows", ProcessedRows).Add("processedBytes", ProcessedBytes).Add("peakMemoryBytes", PeakMemoryBytes).Add("spilledBytes", SpilledBytes).Add("rootStage", RootStage).ToString();
		}
		
	}

}