using Akka.Actor;
using SharpPulsar.Messages;
using SharpPulsar.Sql;
using SharpPulsar.Sql.Live;
using System;
using System.Collections.Generic;
using System.Threading;

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
        public IEnumerable<T> Read()
        {
            var data = _queue.Receive();
            while (data != null)
            {
                yield return data;
            }
        }
        public void Query(ISqlQuery query)
        {
            var d = typeof(T).GetType().Name;
            if(d == "SqlData" && query is SqlSession)
            {
                var q = (SqlSession)query;
                _queryActor.Tell(q);
            }
            else if(d == "LiveSqlData" && query is LiveSqlSession)
            {
                var q = (LiveSqlSession)query;
                _queryActor.Tell(q);
            }
            else
            {
                throw new ArgumentException($"Mismatch: {d} cannot accept query of type {query.GetType().Name}");
            }
        }
    }
}
