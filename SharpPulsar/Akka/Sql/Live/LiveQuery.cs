using System;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.Sql.Client;
using SharpPulsar.Akka.Sql.Message;

namespace SharpPulsar.Akka.Sql.Live
{
    public class LiveQuery : ReceiveActor
    {
        private string _lastPublishTime;
        private readonly LiveSqlSession _sql;
        private string _execute;
        private int _runCount;
        private readonly IActorContext _context;
        private ICancelable _executeCancelable;
        public LiveQuery(IActorRef pulsar, LiveSqlSession sql)
        {
            _sql = sql;
            _execute = sql.ClientOptions.Execute;
            var p = sql.StartAtPublishTime;
            _lastPublishTime = $"{p.Year}-{p.Month}-{p.Day} {p.Hour}:{p.Minute}:{p.Second}.{p.Millisecond}";
            _executeCancelable = Context.System.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(_sql.Frequency), Execute);
            _context = Context;
            Receive<IQueryResponse>(q => { pulsar.Tell(new LiveSqlData(q, _sql.Topic)); });
            Receive<LiveSqlSession>(l =>
            {
                _execute = l.ClientOptions.Execute;
                var pd = l.StartAtPublishTime;
                _lastPublishTime = $"{pd.Year}-{pd.Month}-{pd.Day} {pd.Hour}:{pd.Minute}:{pd.Second}.{pd.Millisecond}";
            });
        }

        private void Execute()
        {
            try
            {
                var text = _execute.Replace("{time}", $"timestamp '{_lastPublishTime}'");
                _sql.Log($"{_runCount} => Executing: {text}");
                _sql.ClientOptions.Execute = text;
                var q = _sql;
                var executor = new Executor(q.ClientSession, q.ClientOptions, Self, Context.System.Log);
                q.Log($"Executing: {q.ClientOptions.Execute}");
                executor.Run();

            }
            catch (Exception ex)
            {
                _sql.ExceptionHandler(ex);
            }
            finally
            {
                _executeCancelable = _context.System.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(_sql.Frequency), Execute);
                
            }
            _runCount++;
        }
        protected override void Unhandled(object message)
        {
            
        }

        protected override void PostStop()
        {
            _executeCancelable?.Cancel();
        }

        public static Props Prop(IActorRef pulsar, LiveSqlSession sql)
        {
            return Props.Create(() => new LiveQuery(pulsar, sql));
        }
        internal class RunQuery
        {
            
        }
    }
}

