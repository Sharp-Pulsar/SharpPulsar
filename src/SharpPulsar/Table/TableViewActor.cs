using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util.Internal;
using App.Metrics.Concurrency;
using Avro.Generic;
using Avro.Util;
using SharpPulsar.Builder;
using SharpPulsar.Common.Naming;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Table.Messages;
/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>

namespace SharpPulsar.Table
{
    internal class TableViewActor<T> : ReceiveActor
	{

		private readonly PulsarClient _client;
		private readonly ISchema<T> _schema;
		private readonly TableViewConfigurationData _conf;
        private readonly Reader<T> _reader;
		private readonly ConcurrentDictionary<string, T> _data;

		private readonly ImmutableDictionary<string, IActorRef> _readers;

		private readonly IList<Action<string, T>> _listeners;
        private IUntypedActorContext _context;
        private ILoggingAdapter _log;
        private readonly IActorRef _self;
        private IActorRef _replyTo;
        private bool _isPersistentTopic;
        private ITopicCompactionStrategy<T> _compactionStrategy;
        private string _guid;
        /// <summary>
		/// Store the refresh tasks. When read to the position recording in the right map,
		/// then remove the position in the right map. If the right map is empty, complete the future in the left.
		/// There should be no timeout exception here, because the caller can only retry for TimeoutException.
		/// It will only be completed exceptionally when no more messages can be read.
		/// </summary>
		private readonly ImmutableDictionary<string, IDictionary<string, ITopicMessageId>> _pendingRefreshRequests;

        /// <summary>
        /// This map stored the read position of each partition. It is used for the following case:
        /// <para>
        ///      1. Get last message ID.
        ///      2. Receive message p1-1:1, p2-1:1, p2-1:2, p3-1:1
        ///      3. Receive response of step1 {|p1-1:1|p2-2:2|p3-3:6|}
        ///      4. No more messages are written to this topic.
        ///      As a result, the refresh operation will never be completed.
        /// </para>
        /// </summary>
        private readonly ImmutableDictionary<string, IMessageId> _lastReadPositions;

        public TableViewActor(PulsarClient client, ISchema<T> schema, TableViewConfigurationData conf)
		{
            _self = Self;
            _log = Context.GetLogger();
            _context = Context;
			_client = client;
			_schema = schema;
			_conf = conf;
			_data = new ConcurrentDictionary<string, T>();
			_readers = new ConcurrentDictionary<string, IActorRef>().ToImmutableDictionary();
			_listeners = new List<Action<string, T>>();
            _compactionStrategy = ITopicCompactionStrategy<T>.Load(conf.TopicCompactionStrategyClassName);
            _isPersistentTopic = conf.TopicName.StartsWith(TopicDomain.Persistent.ToString());

            _pendingRefreshRequests =  new ConcurrentDictionary<string, IDictionary<string, ITopicMessageId>>().ToImmutableDictionary();  
            _lastReadPositions = new ConcurrentDictionary<string, IMessageId>().ToImmutableDictionary();    

            var readerBuilder = new ReaderConfigBuilder<T>()
                .Topic(_conf.TopicName)
                .StartMessageId(IMessageId.Earliest)
                .AutoUpdatePartitions(true)
                .AutoUpdatePartitionsInterval(_conf.AutoUpdatePartitionsSeconds)
                .PoolMessages(true)
                .ReaderName(_conf.SubscriptionName);

            if (_isPersistentTopic)
            {
                readerBuilder.ReadCompacted(true);
            }
            var cryptoKeyReader = _conf.CryptoKeyReader;
            if (cryptoKeyReader != null)
            {
                readerBuilder.CryptoKeyReader(cryptoKeyReader);
            }

            readerBuilder.CryptoFailureAction(_conf.CryptoFailureAction);

            _reader = _client.NewReader(_schema, readerBuilder);
            Receive<ForEachAction<T>>(a => ForEachAndListen(a.Action));
            Receive<StartMessage>(_ =>
            {
                _replyTo = Sender;
                try
                {
                    Akka.Dispatch.ActorTaskScheduler.RunTask(async () =>
                    {
                        await Start();
                    });
                    _replyTo.Tell(new AskResponse());
                }
                catch(Exception ex)
                {
                    _replyTo.Tell(new AskResponse(ex));
                }
            });
            ReceiveAsync<RecReadTailMessages>(async r => await ReadTailMessages(r.Reader));
            ReceiveAsync<RecReadAllExistingMessages>(async r => await ReadAllExistingMessages(r.Reader, r.StartTime, r.MessagesRead, r.MaxMessageIds));
            
            ReceiveAsync<RefeshData>(async _ => await RefreshAsync());
            //Keys.ToHashSet(); 
            Receive<TableDataSize>(_ => Sender.Tell(_data.Count()));
            Receive<TableDataEmpty>(_ => Sender.Tell(_data.Count() == 0));
            Receive<TableDataKey>(get => {

                Sender.Tell(_data.ContainsKey(get.Key));
            });
            Receive<TableDataGet>(get => {
                _data.TryGetValue(get.Key, out var v);
                Sender.Tell(v);
            });
            Receive<TableDataEntrySet>(_ => Sender.Tell(_data.Select(kv => new KeyValuePair<string, T>(kv.Key, kv.Value)).ToHashSet()));
            Receive<TableDataKeySet>(_ => Sender.Tell(_data.Keys.ToHashSet()));
            Receive<TableDataValues>(_ => Sender.Tell(_data.Values));

        }
        public static Props Prop(PulsarClient client, ISchema<T> schema, TableViewConfigurationData conf)
        {
            return Props.Create(() => new TableViewActor<T>(client, schema, conf));
        }
        private void HandleMessage(IMessage<T> msg)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(msg?.Key))
                {
                    var key = msg.Key;
                    var cur = msg.Size() > 0 ? msg.Value : default;

                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"Applying message from topic {_conf.TopicName}. key={key} value={cur}");
                    }
                    var update = true;
                    if (_compactionStrategy != null)
                    {
                        var prev = _data[key];
                        update = !_compactionStrategy.ShouldKeepLeft(prev, cur);
                        if (!update)
                        {
                            _log.Info($"Skipped the message from topic {_conf.TopicName}. key={key} value={cur} prev={prev}");
                            _compactionStrategy.HandleSkippedMessage(key, cur);
                        }
                    }
                    if (update)
                    {
                        try
                        {
                            if (null == cur)
                            {
                                _data.Remove(key, out _);
                            }
                            else
                            {
                                _data.TryAdd(key, cur);
                            }


                            foreach (var listener in _listeners)
                            {
                                try
                                {
                                    listener(key, cur);
                                }
                                catch (Exception t)
                                {
                                    _log.Error($"Table view listener raised an exception: {t}");
                                }
                            }
                            CheckAllFreshTask(msg); 
                        }
                        finally { }

                    }

                }
            }
            catch (Exception e)
            {
                _log.Error(e.ToString());
            }
        }

        private async ValueTask Start()
		{
            if (!_isPersistentTopic)
                await ReadTailMessages(_reader);
            else
                await ReadAllExistingMessages(_reader);
            
        }
        private async ValueTask ReadAllExistingMessages(Reader<T> reader)
        {
            var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var messagesRead = new AtomicLong();
            var ids = await  GetLastMessageIds(reader);
            Self.Tell(new RecReadAllExistingMessages(reader, startTime, messagesRead, ids));
            //await ReadAllExistingMessages(reader, startTime, messagesRead, ids);
        }
        private async ValueTask<IDictionary<string, ITopicMessageId>> GetLastMessageIds(Reader<T> reader)
        {
            var lastMessageIds = await reader.LastMessageIdsAsync();
            IDictionary<string, ITopicMessageId> maxMessageIds = new ConcurrentDictionary<string, ITopicMessageId>();
            lastMessageIds.ForEach(topicMessageId =>
            {
                maxMessageIds[topicMessageId.OwnerTopic] = topicMessageId;
            });
            return maxMessageIds;
        }

        private void FilterReceivedMessages(IDictionary<string, ITopicMessageId> lastMessageIds)
        {
            // The `lastMessageIds` and `readPositions` is concurrency-safe data types.
            foreach (var topicMessageId in lastMessageIds)
            {
                var messageId = _lastReadPositions[topicMessageId.Key];
                if (messageId != null && topicMessageId.Value.CompareTo(messageId) <= 0)
                {
                    lastMessageIds.Remove(topicMessageId.Key);
                }
            }
           
        }

        private void ForEach(Action<string, T> action)
		{
			_data.ForEach(kv=> action(kv.Key, kv.Value));
		}

		private void ForEachAndListen(Action<string, T> action)
		{
			// Ensure we iterate over all the existing entry _and_ start the listening from the exact next message
			try
			{				
				// Execute the action over existing entries
				ForEach(action);

				_listeners.Add(action);
			}
			finally
			{
			}
		}
        protected override void PostStop()
        {
            base.PostStop();
        }
        private async ValueTask RefreshAsync()
        {
            var guid = Guid.NewGuid().ToString();
            try
            {               
                if(_pendingRefreshRequests.Count == 0)
                    _guid = guid;

                var lastMessageIds = await GetLastMessageIds(_reader);
                _pendingRefreshRequests.Add(_guid, lastMessageIds);
                FilterReceivedMessages(lastMessageIds);
                if (lastMessageIds.Count == 0)
                {
                    _pendingRefreshRequests.Remove(_guid);
                }
            }
            catch
            {
                _pendingRefreshRequests.Remove(_guid);
            }
            
        }

        private bool CheckFreshTask(IDictionary<string, ITopicMessageId> maxMessageIds, IMessageId messageId, string topicName)
        {
            // The message received from multi-consumer/multi-reader is processed to TopicMessageImpl.
            var maxMessageId = maxMessageIds[topicName];
            // We need remove the partition from the maxMessageIds map
            // once the partition has been read completely.
            if (maxMessageId != null && messageId.CompareTo(maxMessageId) >= 0)
            {
                maxMessageIds.Remove(topicName);
            }
            if (maxMessageIds.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CheckAllFreshTask(IMessage<T> msg)
        {
            foreach (var fresh in _pendingRefreshRequests)
            {
                var topicName = msg.Topic;
                var messageId = msg.MessageId;
                if (CheckFreshTask(fresh.Value/*maxMessageIds*/, messageId, topicName))
                {                  
                    _pendingRefreshRequests.Remove(fresh.Key);
                }
            }            
        }

        private async ValueTask ReadAllExistingMessages(Reader<T> reader, long startTime, AtomicLong messagesRead, IDictionary<string, ITopicMessageId> maxMessageIds)
        {
            try
            {
                var hasMessage = await reader.HasMessageAvailableAsync();
                if (hasMessage)
                {
                    try
                    {
                        var msg = await reader.ReadNextAsync();
                        messagesRead.Increment();
                        var topicName = msg.Topic;
                        var messageId = msg.MessageId;
                        HandleMessage(msg);
                        if (!CheckFreshTask(maxMessageIds, messageId, topicName))
                        {
                            Self.Tell(new RecReadAllExistingMessages(reader, startTime, messagesRead, maxMessageIds));
                            //await ReadAllExistingMessages(reader, startTime, messagesRead, maxMessageIds);
                        }

                        //await ReadAllExistingMessages(reader, startTime, messagesRead);
                    }
                    catch (Exception ex)
                    {
                        if (ex is PulsarClientException.AlreadyClosedException)
                        {
                            _log.Error($"Reader {reader.Topic} was closed while reading existing messages.{ex}");
                        }
                        else
                        {
                            _log.Warning($"Reader {reader.Topic} was interrupted while reading existing messages", ex.ToString());
                        }                        
                        //await Self.GracefulStop(TimeSpan.FromSeconds(1));
                        //((IInternalActorRef)Self).Stop();   
                    }
                }
                else
                {
                    // Reached the end
                    var endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    var durationMillis = endTime - startTime;
                    _log.Info($"Started table view for topic {reader.Topic} - Replayed {messagesRead} messages in {durationMillis / 1000.0} seconds");

                    Self.Tell(new RecReadTailMessages(reader));
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                //await Self.GracefulStop(TimeSpan.FromSeconds(1));
            }
        }

        private async ValueTask ReadTailMessages(Reader<T> reader)
        {
            try
            {
                var msg = await reader.ReadNextAsync();
                HandleMessage(msg);
                Self.Tell(new RecReadTailMessages(reader));
                //await ReadTailMessages(reader);
            }
            catch(Exception ex) 
            {
                if (ex is PulsarClientException.AlreadyClosedException)
                {
                    _log.Error($"Reader {reader.Topic} was closed while reading tail messages.{ex}");
                    // Fail all refresh request when no more messages can be read.
                    _pendingRefreshRequests.Keys.ForEach(future =>
                    {
                        _pendingRefreshRequests.Remove(future);
                    });
                }
                else
                {
                    // Retrying on the other exceptions such as NotConnectedException
                    try
                    {
                        await Task.Delay(50);
                    }
                    catch //(InterruptedException)
                    {
                        //Thread.CurrentThread.Interrupt();
                    }

                    _log.Error($"Reader {reader.Topic} was interrupted while reading tail messages.", ex.ToString());
                    _log.Warning($"Reader {reader.Topic} was interrupted while reading tail messages. " + "Retrying..", ex);
                    //await ReadTailMessages(reader);
                    Self.Tell(new RecReadTailMessages(reader));
                }

                //await Self.GracefulStop(TimeSpan.FromSeconds(1));
                //((IInternalActorRef)Self).Stop();
            } 
        }
        internal readonly record struct RecReadTailMessages(Reader<T> Reader);
        internal readonly record struct RecReadAllExistingMessages
            (Reader<T> Reader, long StartTime, AtomicLong MessagesRead, IDictionary<string, ITopicMessageId> MaxMessageIds);
    }

}