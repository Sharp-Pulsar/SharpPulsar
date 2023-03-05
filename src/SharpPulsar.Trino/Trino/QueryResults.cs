using System.Text.Json.Serialization;

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
namespace SharpPulsar.Trino.Trino
{

    public class QueryResults : IQueryStatusInfo, IQueryData
    {
        [JsonPropertyName("columns")]
        public IList<Column> Columns { get; set; }

        [JsonPropertyName("data")]
        public IEnumerable<IList<object>> Data { get; set; }

        [JsonPropertyName("warnings")]
        public IList<Warning> Warnings { get; set; }

        [JsonPropertyName("updateCount")]
        public long? UpdateCount { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("infoUri")]
        public Uri InfoUri { get; set; }

        [JsonPropertyName("partialCancelUri")]
        public Uri PartialCancelUri { get; set; }

        [JsonPropertyName("nextUri")]
        public Uri NextUri { get; set; }

        [JsonPropertyName("stats")]
        public StatementStats Stats { get; set; }

        [JsonPropertyName("error")]
        public QueryError Error { get; set; }

        [JsonPropertyName("updateType")]
        public string UpdateType { get; set; }

        public override string ToString()
        {
            return StringHelper.Build(this).Add("id", Id).Add("infoUri", InfoUri).Add("partialCancelUri", PartialCancelUri).Add("nextUri", NextUri).Add("columns", Columns).Add("hasData", Data != null && Data.Any()).Add("stats", Stats).Add("error", Error).Add("updateType", UpdateType).Add("updateCount", UpdateCount).ToString();
        }
    }

}