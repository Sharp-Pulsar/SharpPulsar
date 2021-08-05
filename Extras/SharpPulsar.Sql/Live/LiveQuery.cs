using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Akka.Actor;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;

namespace SharpPulsar.Sql.Live
{
    public class LiveQuery : ReceiveActor
    {
        private string _lastPublishTime;
        private readonly LiveSqlSession _sql;
        private string _execute;
        private int _runCount;
        private readonly IActorContext _context;
        private readonly ICancelable _executeCancelable;
        private readonly IActorRef _self;
        private readonly BufferBlock<LiveSqlData> _buffer;
        public LiveQuery(LiveSqlSession sql)
        {
            _buffer = new BufferBlock<LiveSqlData>();
            _self = Self;
            _sql = sql;
            _execute = sql.ClientOptions.Execute;
            var p = sql.StartAtPublishTime;
            _lastPublishTime = $"{p.Year}-{p.Month}-{p.Day} {p.Hour}:{p.Minute}:{p.Second}.{p.Millisecond}";
            _executeCancelable = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(_sql.Frequency), TimeSpan.FromMilliseconds(_sql.Frequency), Self, ExecuteQuery.Instance, Self);
            _context = Context;

            Receive<IQueryResponse>(q => { _buffer.Post(new LiveSqlData(q, _sql.Topic)); });
            Receive<Read>(_ =>
            {
                _buffer.TryReceive(out var data);
                Sender.Tell(data);
            });
            ReceiveAsync<ExecuteQuery>(async _=> await Execute());
            Receive<LiveSqlSession>(l =>
            {
                _execute = l.ClientOptions.Execute;
                var pd = l.StartAtPublishTime;
                _lastPublishTime = $"{pd.Year}-{pd.Month}-{pd.Day} {pd.Hour}:{pd.Minute}:{pd.Second}.{pd.Millisecond}";
            });
        }

        private async ValueTask Execute()
        {
            try
            {
                var text = _execute.Replace("{time}", $"timestamp '{_lastPublishTime}'");
                _sql.Log($"{_runCount} => Executing: {text}");
                _sql.ClientOptions.Execute = text;
                var q = _sql;
                var executor = new Executor(q.ClientSession, q.ClientOptions, _self, _context.System.Log);
                q.Log($"Executing: {q.ClientOptions.Execute}");
                await executor .Run();

            }
            catch (Exception ex)
            {
                _sql.ExceptionHandler(ex);
                Context.System.Log.Error(ex.ToString());
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

        public static Props Prop(LiveSqlSession sql)
        {
            return Props.Create(() => new LiveQuery(sql));
        }
    }
    public sealed class ExecuteQuery
    {
        public static ExecuteQuery Instance = new ExecuteQuery();
    }
}

