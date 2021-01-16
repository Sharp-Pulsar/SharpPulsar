using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Akka.Actor;
using Akka.Event;
using Nito.AsyncEx;
using PulsarAdmin;
using SharpPulsar.Akka.EventSource.Messages;
using SharpPulsar.Akka.EventSource.Messages.Presto;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Akka.Sql.Client;
using SharpPulsar.Akka.Sql.Message;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl;

namespace SharpPulsar.Akka.EventSource.Presto.Tagged
{
    public class PrestoTaggedSourceActor : ReceiveActor
    {
        private readonly IActorRef _pulsarManager;
        private readonly EventMessageId _endId;
        private EventMessageId _lastEventMessageId;
        private ICancelable _queryCancelable;
        private readonly HttpClient _httpClient;
        private readonly IPrestoEventSourceMessage _message;
        private readonly TopicName _topicName;
        private readonly IAdvancedScheduler _scheduler;
        private readonly ILoggingAdapter _log;
        private readonly IActorRef _self;
        private long _sequenceId;
        private Tag _tag;
        public PrestoTaggedSourceActor(IActorRef pulsarManager, EventMessageId startId, EventMessageId endId, bool isLive, HttpClient httpClient, IPrestoEventSourceMessage message, Tag tag)
        {
            _tag = tag;
            _self = Self;
            _log = Context.GetLogger();
            _scheduler = Context.System.Scheduler.Advanced;
            _topicName = TopicName.Get(message.Topic);
            _httpClient = httpClient;
            _message = message;
            _endId = endId;
            _lastEventMessageId = endId;
            _sequenceId = startId.Index;
            _pulsarManager = pulsarManager;
            FirstQuery(startId, endId, isLive);
        }
        private void FirstQuery(EventMessageId start, EventMessageId end, bool isLive)
        {
            try
            {
                var max = end.Index - start.Index;
                var query =
                    $"select {string.Join(", ", _message.Columns)}, __message_id__, __publish_time__, __properties__, __key__, __producer_name__, __sequence_id__, __partition__ from \"{_message.Topic}\" where CAST(split_part(replace(replace(__message_id__, '('), ')'), ',', 1) AS BIGINT) BETWEEN bigint '{start.LedgerId}' AND bigint '{end.LedgerId}' AND CAST(split_part(replace(replace(__message_id__, '('), ')'), ',', 2) AS BIGINT) BETWEEN bigint '{start.EntryId}' AND bigint '{end.EntryId}' AND element_at(cast(json_parse(__properties__) as map(varchar, varchar)), '{_tag.Key}') = '{_tag.Value}' ORDER BY __publish_time__ ASC LIMIT {max}";
                var options = _message.Options;
                options.Catalog = "pulsar";
                options.Schema = "" + _message.Tenant + "/" + _message.Namespace + "";
                options.Execute = query;
                var session = options.ToClientSession();
                var executor = new Executor(session, options, _self, _log);
                _log.Info($"Executing: {options.Execute}");
                executor.Run();
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
                var ids = NextFlow();
                var max = ids.Index - _lastEventMessageId.Index;
                if (max > 0)
                {
                    var query =
                        $"select {string.Join(", ", _message.Columns)}, __message_id__, __publish_time__, __properties__, __key__, __producer_name__, __sequence_id__, __partition__ from \"{_message.Topic}\" where CAST(split_part(replace(replace(__message_id__, '('), ')'), ',', 1) AS BIGINT) BETWEEN bigint '{_lastEventMessageId.LedgerId}' AND bigint '{ids.LedgerId}' AND CAST(split_part(replace(replace(__message_id__, '('), ')'), ',', 2) AS BIGINT) BETWEEN bigint '{_lastEventMessageId.EntryId + 1}' AND bigint '{ids.EntryId}' AND element_at(cast(json_parse(__properties__) as map(varchar, varchar)), '{_tag.Key}') = '{_tag.Value}' ORDER BY __publish_time__ ASC LIMIT {max}";
                    var options = _message.Options; 
                    options.Catalog = "pulsar";
                    options.Schema = "" + _message.Tenant + "/" + _message.Namespace + "";
                    options.Execute = query;
                    var session = options.ToClientSession();
                    var executor = new Executor(session, options, _self, _log);
                    _log.Info($"Executing: {options.Execute}");
                    executor.Run();
                    _lastEventMessageId = ids;
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
        private EventMessageId NextFlow()
        {
            var adminRestapi = new PulsarAdminRESTAPI(_message.AdminUrl, _httpClient, true);
            var statsTask = adminRestapi.GetInternalStats1Async(_message.Tenant, _message.Namespace, _message.Topic);
            var stats = SynchronizationContextSwitcher.NoContext(async () => await statsTask).Result;
            var start = MessageIdHelper.NextFlow(stats);
            var startMessageId = new EventMessageId(start.Ledger, start.Entry, start.Index);
            return startMessageId;
        }
        private void Consume()
        {
            Receive<DataResponse>(c =>
            {
                var msg = c.Metadata["message_id"].ToString().Trim('(', ')').Split(',').Select(int.Parse).ToArray();
                var messageId = new MessageId(msg[0], msg[1], msg[2]);
                if (messageId.LedgerId <= _endId.LedgerId && messageId.EntryId <= _endId.EntryId)
                {
                    var eventMessage = new EventEnvelope(c.Data, c.Metadata, _sequenceId, _topicName.ToString());
                    _pulsarManager.Tell(eventMessage);
                    _sequenceId++;
                }
                else Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
            Receive<StatsResponse>(s =>
            {
                var stats = new EventStats(s);
                _pulsarManager.Tell(stats);
            });
            Receive<ErrorResponse>(s =>
            {
                var error = new EventError(s);
                _pulsarManager.Tell(error);
            });
            Receive<ReceiveTimeout>(t => { Self.GracefulStop(TimeSpan.FromSeconds(5)); });
            //to track last sequence id for lagging player
            Context.SetReceiveTimeout(TimeSpan.FromSeconds(30));
        }
        private void LiveConsume()
        {
            Receive<DataResponse>(c =>
            {
                var eventMessage = new EventEnvelope(c.Data, c.Metadata, _sequenceId, _topicName.ToString());
                _pulsarManager.Tell(eventMessage); 
                _sequenceId++;
            });
            Receive<StatsResponse>(s =>
            {
                var stats = new EventStats(s);
                _pulsarManager.Tell(stats);
            });
            Receive<ErrorResponse>(s =>
            {
                var error = new EventError(s);
                _pulsarManager.Tell(error);
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

        public static Props Prop(IActorRef pulsarManager, EventMessageId start, EventMessageId endId, bool isLive, HttpClient httpClient, IPrestoEventSourceMessage message, Tag tag)
        {
            return Props.Create(() => new PrestoTaggedSourceActor(pulsarManager, start, endId, isLive, httpClient, message, tag));
        }

        public IStash Stash { get; set; }
    }

}
