using System;
using System.Collections.Generic;
using Akka.Actor;
using PrestoSharp;

namespace SharpPulsar.Akka.Sql
{
    public class SqlWorker: ReceiveActor
    {
        private readonly PrestoSqlDbConnection _connection;
        private readonly IActorRef _pulsarManager;
        public SqlWorker(string server, IActorRef pulsarManager)
        {
            _connection = new PrestoSqlDbConnection(server);
            _pulsarManager = pulsarManager;
            _connection.Open();//fake?

            Receive((Action<InternalCommands.Sql>)Query);

        }

        protected override void Unhandled(object message)
        {
            
        }

        private void Query(InternalCommands.Sql query)
        {
            var errorRow = 0;
            try
            {
                var q = query;
                q.Log($"Executing: {q.Query}");
                using var cmd = _connection.CreateCommand();
                cmd.CommandText = q.Query;
                using var reader = cmd.ExecuteReader();
                var rows = reader.RecordsAffected;
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
                        }
                        else
                        {
                            data[col] = value;
                        }
                    }

                    rows--;
                    errorRow++;
                    var hasRows = rows > 0;
                    _pulsarManager.Tell(new SqlData(hasRows, rows, data, metadata));
                }
            }
            catch (Exception ex)
            {
                _pulsarManager.Tell(new SqlData(false, errorRow + 1, null, null, true, ex));
                query.ExceptionHandler(ex);
            }
        }
        public static Props Prop(string server, IActorRef pulsarManager)
        {
            return Props.Create(() => new SqlWorker(server, pulsarManager));
        }
    }
}
