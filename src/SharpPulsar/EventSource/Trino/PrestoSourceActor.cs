using System;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using SharpPulsar.EventSource.Messages;
using SharpPulsar.EventSource.Messages.Presto;
using SharpPulsar.Common.Naming;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.Utils;

namespace SharpPulsar.EventSource.Trino
{
    public class PrestoSourceActor : ReceiveActor
    {
        private readonly BufferBlock<IEventEnvelope> _buffer;
        private readonly long _toMessageId;
        private long _lastEventMessageId;
        private ICancelable _queryCancelable;
        private readonly IPrestoEventSourceMessage _message;
        private readonly TopicName _topicName;
        private readonly IAdvancedScheduler _scheduler;
        private readonly ILoggingAdapter _log;
        private readonly IActorRef _self;
        private long _currentOffset;
        private readonly int _partitionIndex;
        private readonly long _queryRange;
        public PrestoSourceActor(BufferBlock<IEventEnvelope> buffer, bool isLive, IPrestoEventSourceMessage message)
        {
            _buffer = buffer;
            _self = Self;
            _log = Context.GetLogger();
            _scheduler = Context.System.Scheduler.Advanced;
            _topicName = TopicName.Get(message.Topic);
            _message = message;
            _toMessageId = message.ToMessageId;
            _lastEventMessageId = _message.FromMessageId;
            _partitionIndex = ((MessageId)MessageIdUtils.GetMessageId(message.FromMessageId)).PartitionIndex;
            _queryRange = message.ToMessageId - message.FromMessageId;
            FirstQuery(isLive);
        }
        private void FirstQuery(bool isLive)
        {
            try
            {
                var max = _message.ToMessageId - _message.FromMessageId;
                var start = (MessageId)MessageIdUtils.GetMessageId(_message.FromMessageId);
                var end = (MessageId)MessageIdUtils.GetMessageId(_message.ToMessageId);
                var query = $"select {string.Join(", ", _message.Columns)},__message_id__, __publish_time__, __properties__, __key__, __producer_name__, __sequence_id__, __partition__ from \"{_message.Topic}\" where __partition__ = {start.PartitionIndex} AND CAST(split_part(replace(replace(__message_id__, '('), ')'), ',', 1) AS BIGINT) BETWEEN bigint '{start.LedgerId}' AND bigint '{end.LedgerId}' AND CAST(split_part(replace(replace(__message_id__, '('), ')'), ',', 2) AS BIGINT) BETWEEN bigint '{start.EntryId}' AND bigint '{end.EntryId}' ORDER BY __publish_time__ ASC LIMIT {max}";
                var options = _message.Options;
                options.Catalog = "pulsar";
                options.Schema = "" + _message.Tenant + "/" + _message.Namespace + "";
                options.Execute = query;
                var session = options.ToClientSession();
                var executor = new Executor(session, options, _self, _log);
                _log.Info($"Executing: {options.Execute}");
                _ = executor.Run().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
            if (isLive)
                LiveConsume();
            else Consume();
        }
        private void Query()
        {
            try
            {
                var max = _currentOffset - _lastEventMessageId;
                if (max > 0)
                {

                    var start = (MessageId)MessageIdUtils.GetMessageId(_lastEventMessageId);
                    var end = (MessageId)MessageIdUtils.GetMessageId(_currentOffset);
                    var query =
                        $"select {string.Join(", ", _message.Columns)}, __message_id__, __publish_time__, __properties__, __key__, __producer_name__, __sequence_id__, __partition__ from \"{_message.Topic}\" where __partition__ = {_partitionIndex} AND CAST(split_part(replace(replace(__message_id__, '('), ')'), ',', 1) AS BIGINT) BETWEEN bigint '{start.LedgerId}' AND bigint '{end.LedgerId}' AND CAST(split_part(replace(replace(__message_id__, '('), ')'), ',', 2) AS BIGINT) BETWEEN bigint '{start.EntryId + 1}' AND bigint '{end.EntryId}' ORDER BY __publish_time__ ASC LIMIT {_queryRange}";
                    var options = _message.Options;
                    options.Catalog = "pulsar";
                    options.Schema = "" + _message.Tenant + "/" + _message.Namespace + "";
                    options.Execute = query;
                    var session = options.ToClientSession();

                    var executor = new Executor(session, options, _self, _log);
                    _log.Info($"Executing: {options.Execute}");
                    _ = executor.Run().GetAwaiter().GetResult();
                    _lastEventMessageId = _currentOffset;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
            finally
            {
                _queryCancelable = _scheduler.ScheduleOnceCancelable(TimeSpan.FromSeconds(10), Query);
            }
        }
        private void Consume()
        {
            Receive<DataResponse>(c =>
            {
                for (var i = 0; i < c.Data.Count; i++)
                {
                    var msgData = c.Data.ElementAt(i);
                    var msg = msgData["__message_id__"].ToString().Trim('(', ')').Split(',').Select(int.Parse).ToArray();
                    var messageId = MessageIdUtils.GetOffset(new MessageId(msg[0], msg[1], msg[2]));
                    if (messageId <= _toMessageId)
                    {
                        var eventMessage = new EventEnvelope(msgData, messageId, _topicName.ToString());
                        _buffer.Post(eventMessage);
                        _currentOffset = messageId;
                    }
                    else Self.GracefulStop(TimeSpan.FromSeconds(5));
                }
                _buffer.Post(new EventStats(new StatsResponse(c.StatementStats)));
            });
            Receive<StatsResponse>(s =>
            {
                var stats = new EventStats(s);
                _buffer.Post(stats);
            });
            Receive<ErrorResponse>(s =>
            {
                var error = new EventError(s);
                _buffer.Post(error);
            });
            Receive<ReceiveTimeout>(t => { Self.GracefulStop(TimeSpan.FromSeconds(5)); });
            //to track last sequence id for lagging player
            Context.SetReceiveTimeout(TimeSpan.FromSeconds(30));
        }
        private void LiveConsume()
        {
            Receive<DataResponse>(c =>
            {
                for (var i = 0; i < c.Data.Count; i++)
                {
                    var msgData = c.Data.ElementAt(i);
                    var msg = msgData["__message_id__"].ToString().Trim('(', ')').Split(',').Select(int.Parse).ToArray();
                    var messageId = MessageIdUtils.GetOffset(new MessageId(msg[0], msg[1], msg[2]));
                    var eventMessage = new EventEnvelope(msgData, messageId, _topicName.ToString());
                    _buffer.Post(eventMessage);
                    _currentOffset = messageId;
                }
                _buffer.Post(new EventStats(new StatsResponse(c.StatementStats)));
            });
            Receive<StatsResponse>(s =>
            {
                var stats = new EventStats(s);
                _buffer.Post(stats);
            });
            Receive<ErrorResponse>(s =>
            {
                var error = new EventError(s);
                _buffer.Post(error);
            });
            _queryCancelable = _scheduler.ScheduleOnceCancelable(TimeSpan.FromSeconds(60), Query);
        }

        protected override void Unhandled(object message)
        {
            //Since we have saved the last consumed sequence id before the timeout,
            //we can discard any Messages, they will be replayed after all, from the last saved sequence id
        }

        protected override void PostStop()
        {
            _queryCancelable?.Cancel();
        }

        public static Props Prop(BufferBlock<IEventEnvelope> buffer, bool isLive, IPrestoEventSourceMessage message)
        {
            return Props.Create(() => new PrestoSourceActor(buffer, isLive, message));
        }

        public IStash Stash { get; set; }
    }

}
