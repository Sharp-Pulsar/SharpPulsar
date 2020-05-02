using System;
using System.Collections.Generic;
using System.Text.Json;
using Akka.Actor;
using Akka.Event;
using PrestoSharp;
using SharpPulsar.Akka.InternalCommands;

namespace SharpPulsar.Akka.Sql
{
    public class SqlWorker: ReceiveActor
    {
        private PrestoSqlDbConnection _connection;
        private readonly ILoggingAdapter _log = Context.GetLogger();
        public SqlWorker(string server)
        {
            _connection = new PrestoSqlDbConnection(server);
            _connection.Open();//fake?

            Receive((Action<InternalCommands.Sql>)this.Query);

        }

        protected override void Unhandled(object message)
        {
            
        }

        private void Query(InternalCommands.Sql query)
        {
            try
            {
                var q = query;
                q.Log($"Executing: {q.Query}");
                using var cmd = _connection.CreateCommand();
                cmd.CommandText = q.Query;
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var payload = new Dictionary<string, string>();
                    var message = new Dictionary<string, object>();
                    var metadata = new Dictionary<string, object>();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var col = reader.GetName(i);
                        var value = reader.GetValue(i);
                        if (col.StartsWith("__") && col.EndsWith("__"))
                        {
                            metadata[col.Trim('_')] = value;
                        }
                        else
                        {
                            message[col] = value;
                        }
                    }
                    payload["Message"] = JsonSerializer.Serialize(message,
                        new JsonSerializerOptions { WriteIndented = true });
                    if (q.IncludeMetadata)
                        payload["Metadata"] = JsonSerializer.Serialize(metadata,
                            new JsonSerializerOptions { WriteIndented = true });
                    q.Handler(payload);
                }

                q.Handler(new Dictionary<string, string> {{"Finished", "true"}});
            }
            catch (Exception ex)
            {
                query.ExceptionHandler(ex);
            }
        }
        public static Props Prop(string server)
        {
            return Props.Create(() => new SqlWorker(server));
        }
    }
}
