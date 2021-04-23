using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using SharpPulsar.Precondition;
using SharpPulsar.Presto;
using SharpPulsar.Presto.Facebook.Type;
using SharpPulsar.Sql.Message;

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
namespace SharpPulsar.Sql.Client
{

    public class Query
    {
        private readonly IStatementClient _client;
        private readonly ILoggingAdapter _log;
        private readonly IActorRef _handler;

        public Query(IStatementClient client, IActorRef handler, ILoggingAdapter log)
        {
            _client = Condition.RequireNonNull(client, "client is null");
            _log = log;
            _handler = handler;
        }

        public string SetCatalog => _client.SetCatalog;

        public string SetSchema => _client.SetSchema;

        public virtual IDictionary<string, string> SetSessionProperties => _client.SetSessionProperties;

        public virtual ISet<string> ResetSessionProperties => _client.ResetSessionProperties;

        public virtual IDictionary<string, SelectedRole> SetRoles => _client.SetRoles;

        public virtual IDictionary<string, string> AddedPreparedStatements => _client.AddedPreparedStatements;

        public virtual ISet<string> DeallocatedPreparedStatements => _client.DeallocatedPreparedStatements;

        public virtual string StartedTransactionId => _client.StartedTransactionId;

        public virtual bool ClearTransactionId => _client.ClearTransactionId;

        public async ValueTask<bool> MaterializeQueryOutput()
        {
            await ProcessInitialStatusUpdates ();

            // if running or finished
            if (_client.Running || _client.Finished && _client.FinalStatusInfo().Error == null)
            {
                IQueryStatusInfo results = _client.Running ? _client.CurrentStatusInfo() : _client.FinalStatusInfo();
                if (results.UpdateType != null)
                {
                    RenderUpdate(results);
                }
                else if (results.Columns == null)
                {
                    _log.Error($"Query {results.Id} has no columns\n");
                    return false;
                }
                else
                {
                    await RenderResults(results.Columns.ToList(), _client.Stats);
                }

                return true;
            }
            else
            {
                _handler.Tell(new StatsResponse(_client.Stats));
                Condition.CheckArgument(!_client.Running);

                // Print all warnings at the end of the query
                _log.Debug(JsonSerializer.Serialize(_client.FinalStatusInfo().Warnings, new JsonSerializerOptions { WriteIndented = true }));

                if (_client.ClientAborted)
                {
                    _log.Debug("Query aborted by user");
                    return false;
                }
                if (_client.ClientError)
                {
                    _log.Debug("Query is gone (server restarted?)");
                    return false;
                }

                if (_client.FinalStatusInfo().Error != null || _client.FinalStatusInfo().Warnings != null)
                {
                    var error = _client.FinalStatusInfo().Error;
                    var warning = _client.FinalStatusInfo().Warnings;
                    _log.Warning(JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true }));

                    _handler.Tell(new ErrorResponse(error, warning?.ToList()));
                    return false;
                }
                return true;
            }
        }

        private async ValueTask RenderResults(List<Column> columns, StatementStats stats)
        {
            var dataList = new List<Dictionary<string, object>>();
            while (_client.Running)
            {
                var cData = _client.CurrentData().Data;
                if (cData != null)
                {
                    var currentData = cData.ToList();
                    for (var i = 0; i < currentData.Count; i++)
                    {
                        var data = new Dictionary<string, object>();
                        var value = currentData[i];
                        for (var y = 0; y < value.Count; y++)
                        {
                            var col = columns[y].Name; 
                            data[col] = value[y];
                        }
                        dataList.Add(data);
                    }
                }
                await _client.Advance();
            }
            _handler.Tell(new DataResponse(dataList, stats));
        }
        private async ValueTask ProcessInitialStatusUpdates()
        {
            while (_client.Running && _client.CurrentData().Data == null)
            {
                try
                {
                    _log.Debug(JsonSerializer.Serialize(_client.CurrentStatusInfo().Warnings, new JsonSerializerOptions { WriteIndented = true }));
                    await _client.Advance();
                }
                catch(Exception ex)
                {
                    _log.Error(ex.ToString());
                }
            }
            IList<Presto.Warning> warnings;
            if (_client.Running)
            {
                warnings = _client.CurrentStatusInfo().Warnings;
            }
            else
            {
                warnings = _client.FinalStatusInfo().Warnings;
            }
            _log.Warning(JsonSerializer.Serialize(warnings, new JsonSerializerOptions { WriteIndented = true }));
        }

        private void RenderUpdate(IQueryStatusInfo results)
        {
            string status = results.UpdateType;
            if (results.UpdateCount != null)
            {
                long count = results.UpdateCount.Value;
                var row = count != 1 ? "s" : string.Empty;
                status += $": {count} row{row}";
            }
            _log.Info(status);
        }

    }

}