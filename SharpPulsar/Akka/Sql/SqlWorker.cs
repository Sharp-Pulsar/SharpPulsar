using System;
using System.Collections.Generic;
using System.Text.Json;
using Akka.Actor;
using PrestoSharp;
using SharpPulsar.Akka.InternalCommands;

namespace SharpPulsar.Akka.Sql
{
    public class SqlWorker: ReceiveActor
    {
        private PrestoSqlDbConnection _connection;
        public SqlWorker(string server)
        {
            _connection = new PrestoSqlDbConnection(server);
            _connection.Open();//fake?

            Receive<QueryData>(Query);

        }

        protected override void Unhandled(object message)
        {
            
        }

        private void Query(QueryData query)
        {
            try
            {
                var q = query;
                Context.System.Log.Debug($"Executing: {q.Query}");
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
                        try
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

                            payload["Message"] = JsonSerializer.Serialize(message,
                                new JsonSerializerOptions {WriteIndented = true});
                            if (q.IncludeMetadata)
                                payload["Metadata"] = JsonSerializer.Serialize(metadata,
                                    new JsonSerializerOptions {WriteIndented = true});
                            q.Handler(payload);
                        }
                        catch (Exception e)
                        {
                            q.ExceptionHandler(e);
                        }
                    }
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
