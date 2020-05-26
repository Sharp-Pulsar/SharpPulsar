using System;
using System.Collections.Generic;
using System.Globalization;
using Akka.Actor;
using PrestoSharp;
using SharpPulsar.Akka.InternalCommands;

namespace SharpPulsar.Akka.Sql.Live
{
    public class LiveQuery : ReceiveActor
    {
        private readonly PrestoSqlDbConnection _connection;
        private IActorRef _pulsarManager;
        private string _lastPublishTime;
        private LiveSql _sql;
        private int _runCount;
        public LiveQuery(IActorRef pulsar, LiveSql sql)
        {
            _connection = new PrestoSqlDbConnection(sql.Server);
            _connection.Open();//fake?
            _pulsarManager = pulsar;
            _sql = sql;
            _lastPublishTime = sql.StartAtPublishTime.ToShortDateString();
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(_sql.Frequency), Self, new RunQuery(), Nobody.Instance );
            Receive<RunQuery>(r => { Execute(); });
            Receive<LiveSql>(l =>
            {
                _sql = l;
                _lastPublishTime = l.StartAtPublishTime.ToShortDateString();
            });
        }

        private void Execute()
        {
            try
            {
                var text = _sql.Command.Replace("{time}", $"date '2020-5-24 05:8:34'");
                _sql.Log($"{_runCount} => Executing: {text}");
                using var cmd = _connection.CreateCommand();
                //check for __publish_time__ > {time} when submitting query
                cmd.CommandText = text;
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var data = new Dictionary<string, object>();
                    var metadata = new Dictionary<string, object>();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var col = reader.GetName(i);
                        var value = reader.GetValue(i);
                        if (col.StartsWith("__") && col.EndsWith("__"))
                        {
                            metadata[col.Trim('_')] = value;
                            if (col == "__publish_time__")
                                _lastPublishTime = value.ToString();
                        }
                        else
                        {
                            data[col] = value;
                        }
                    }
                    _pulsarManager.Tell(new LiveSqlData(data, metadata, _sql.Topic));
                }
                
            }
            catch (Exception ex)
            {
                _sql.ExceptionHandler(ex);
            }

            _runCount++;
        }
        protected override void Unhandled(object message)
        {
            
        }

        protected override void PostStop()
        {
            _connection.Dispose();
        }

        public static Props Prop(IActorRef pulsar, LiveSql sql)
        {
            return Props.Create(() => new LiveQuery(pulsar, sql));
        }
        internal class RunQuery
        {
            
        }
    }
}

