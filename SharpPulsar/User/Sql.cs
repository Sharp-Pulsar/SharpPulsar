using Akka.Actor;
using SharpPulsar.Common.Naming;
using SharpPulsar.Messages;
using SharpPulsar.Sql;
using SharpPulsar.Sql.Live;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SharpPulsar.User
{
    public class Sql<T>
    {
        private readonly SqlQueue<T> _queue;
        private readonly IActorRef _queryActor;
        public static Sql<SqlData> NewSql(ActorSystem system)
        {
            var q = new SqlQueue<SqlData>();
            var actor = system.ActorOf(SqlWorker.Prop(q));
            return new Sql<SqlData>(actor, q);
        }
        public static Sql<LiveSqlData> NewLiveSql(ActorSystem system, LiveSqlSession sql)
        {
            var q = new SqlQueue<LiveSqlData>();
            var actor = system.ActorOf(LiveQuery.Prop(q, sql));
            return new Sql<LiveSqlData>(actor, q);
        }
        public Sql(IActorRef actor, SqlQueue<T> queue)
        {
            _queue = queue;
            _queryActor = actor;
        }
        /// <summary>
        /// Reads query results as IAsyncEnumerable<T>
        /// </summary>
        /// <param name="timeOut">The query may not have finished before this method is call. TimeSpan can be supplied to wait</param>
        /// <returns></returns>
        public async IAsyncEnumerable<T> ReadResults(TimeSpan? timeOut = null)
        {
            var data = _queue.Receive(); 
            if (data == null && timeOut.HasValue)
            {
                await Task.Delay((int)timeOut.Value.TotalMilliseconds).ConfigureAwait(false);
                data = _queue.Receive();
            }
            while (data != null)
            {
                yield return data;
                data = _queue.Receive();
            }
        }
        public T ReadQueryResult(TimeSpan? timeOut = null)
        {
            return ReadQueryResultAsync(timeOut).GetAwaiter().GetResult();
        }
        public async ValueTask<T> ReadQueryResultAsync(TimeSpan? timeOut = null)
        {
            var data = _queue.Receive();
            if(data == null && timeOut.HasValue)
            {
                await Task.Delay((int)timeOut.Value.TotalMilliseconds).ConfigureAwait(false);
                data = _queue.Receive();
            }
            return data;
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
    }
}
