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

using System.Text.Json.Serialization;
using SharpPulsar.Sql.Presto.Facebook.Type;

namespace SharpPulsar.Sql.Presto
{
    public class StatementStats
	{
		[JsonPropertyName("state")]
		public string State {get; set; }
        [JsonPropertyName("queued")]
		public bool Queued { get; set; }
        [JsonPropertyName("scheduled")]
		public bool Scheduled { get; set; }
        [JsonPropertyName("nodes")]
		public int Nodes { get; set; }
        [JsonPropertyName("totalSplits")]
		public int TotalSplits { get; set; }
        [JsonPropertyName("queuedSplits")]
		public int QueuedSplits { get; set; }
        [JsonPropertyName("runningSplits")]
		public int RunningSplits { get; set; }
        [JsonPropertyName("completedSplits")]
		public int CompletedSplits { get; set; }
        [JsonPropertyName("cpuTimeMillis")]
		public long CpuTimeMillis { get; set; }
		[JsonPropertyName("wallTimeMillis")]
		public long WallTimeMillis { get; set; }
		[JsonPropertyName("queuedTimeMillis")]
		public long QueuedTimeMillis { get; set; }
		[JsonPropertyName("elapsedTimeMillis")]
		public long ElapsedTimeMillis { get; set; }
		[JsonPropertyName("processedRows")]
		public long ProcessedRows { get; set; }

		[JsonPropertyName("processedBytes")]
		public long ProcessedBytes { get; set; }

		[JsonPropertyName("physicalInputBytes")]
		public long PhysicalInputBytes { get; set; }

		[JsonPropertyName("peakMemoryBytes")]
		public long PeakMemoryBytes { get; set; }

		[JsonPropertyName("spilledBytes")]
		public long SpilledBytes { get; set; }

		[JsonPropertyName("rootStage")]
		public StageStats RootStage { get; set; }

		public override string ToString()
		{
			return StringHelper.Build(this).Add("state", State).Add("queued", Queued).Add("scheduled", Scheduled).Add("nodes", Nodes).Add("totalSplits", TotalSplits).Add("queuedSplits", QueuedSplits).Add("runningSplits", RunningSplits).Add("completedSplits", CompletedSplits).Add("cpuTimeMillis", CpuTimeMillis).Add("wallTimeMillis", WallTimeMillis).Add("queuedTimeMillis", QueuedTimeMillis).Add("elapsedTimeMillis", ElapsedTimeMillis).Add("processedRows", ProcessedRows).Add("processedBytes", ProcessedBytes).Add("physicalInputBytes", PhysicalInputBytes).Add("peakMemoryBytes", PeakMemoryBytes).Add("spilledBytes", SpilledBytes).Add("rootStage", RootStage).ToString();
		}
		
	}

}