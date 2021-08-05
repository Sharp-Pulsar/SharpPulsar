using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Sql.Live;
using SharpPulsar.Sql.Message;
using SharpPulsar.Sql.Naming;

namespace SharpPulsar.Sql.Public
{
    public class SqlInstance<T>
    {
        private readonly IActorRef _queryActor;
        public static SqlInstance<SqlData> NewSql(ActorSystem system)
        {
            var actor = system.ActorOf(SqlWorker.Prop());
            return new SqlInstance<SqlData>(actor);
        }
        public static SqlInstance<LiveSqlData> NewLiveSql(ActorSystem system, LiveSqlSession sql)
        {
            var actor = system.ActorOf(LiveQuery.Prop(sql));
            return new SqlInstance<LiveSqlData>(actor);
        }
        public SqlInstance(IActorRef actor)
        {
            _queryActor = actor;
        }
        /// <summary>
        /// Reads query results as IAsyncEnumerable<T>
        /// </summary>
        /// <param name="timeOut">The query may not have finished before this method is call. TimeSpan can be supplied to wait</param>
        /// <returns></returns>
        public async IAsyncEnumerable<T> ReadsAsync(TimeSpan? timeOut = null)
        {
            var data = await ReadData(timeOut).ConfigureAwait(false);
            while (data != null)
            {
                yield return data;
                data = await ReadData(timeOut);
            }
        }
        public T Read(TimeSpan? timeOut = null)
        {
            return ReadAsync(timeOut).GetAwaiter().GetResult();
        }
        public async ValueTask<T> ReadAsync(TimeSpan? timeOut = null)
        {
            return await ReadData(timeOut);
        }
        public void SendQuery(ISqlQuery query)
        {
            var d = typeof(T).Name;
            if(d == "SqlData" && query is SqlQuery dat)
            {
                var hasQuery = !string.IsNullOrWhiteSpace(dat.ClientOptions.Execute);
                if (string.IsNullOrWhiteSpace(dat.ClientOptions.Server) || dat.ExceptionHandler == null || string.IsNullOrWhiteSpace(dat.ClientOptions.Execute) || dat.Log == null)
                    throw new ArgumentException("'Sql' is in an invalid state: null field not allowed");
                if (hasQuery)
                {
                    dat.ClientOptions.Execute.TrimEnd(';');
                }
                else
                {
                    dat.ClientOptions.Execute = File.ReadAllText(dat.ClientOptions.File);
                }

                var q = new SqlSession(dat.ClientOptions.ToClientSession(), dat.ClientOptions, dat.ExceptionHandler, dat.Log);
               _queryActor.Tell(q);
            }
            else if(d == "LiveSqlData" && query is LiveSqlQuery data)
            {
                var hasQuery = !string.IsNullOrWhiteSpace(data.ClientOptions.Execute);

                if (string.IsNullOrWhiteSpace(data.ClientOptions.Server) || data.ExceptionHandler == null || string.IsNullOrWhiteSpace(data.ClientOptions.Execute) || data.Log == null || string.IsNullOrWhiteSpace(data.Topic))
                    throw new ArgumentException("'Sql' is in an invalid state: null field not allowed");

                if (hasQuery)
                {
                    data.ClientOptions.Execute.TrimEnd(';');
                }
                else
                {
                    data.ClientOptions.Execute = File.ReadAllText(data.ClientOptions.File);
                }

                if (!data.ClientOptions.Execute.Contains("__publish_time__ > {time}"))
                {
                    if (data.ClientOptions.Execute.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException("add '__publish_time__ > {time}' to where clause");
                    }
                    throw new ArgumentException("add 'where __publish_time__ > {time}' to '" + data.ClientOptions.Execute + "'");
                }
                if (!TopicName.IsValid(data.Topic))
                    throw new ArgumentException($"Topic '{data.Topic}' failed validation");

                var q = new LiveSqlSession(data.ClientOptions.ToClientSession(), data.ClientOptions, data.Frequency, data.StartAtPublishTime, TopicName.Get(data.Topic).ToString(), data.Log, data.ExceptionHandler);
                _queryActor.Tell(q);
            }
            else
            {
                throw new ArgumentException($"Mismatch: {d} cannot accept query of type {query.GetType().Name}");
            }
        }

        private async  ValueTask<T> ReadData(TimeSpan? timeSpan)
        {
            try
            {
                if (timeSpan.HasValue)
                    return await _queryActor.Ask<T>(Message.Read.Instance, timeSpan.Value);

                return await _queryActor.Ask<T>(Message.Read.Instance);
            }
            catch (Exception e)
            {
                return default;
            }
        }
    }
}
