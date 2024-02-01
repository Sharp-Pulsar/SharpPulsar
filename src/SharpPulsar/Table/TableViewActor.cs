using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util.Internal;
using App.Metrics.Concurrency;
using Avro.Generic;
using Avro.Util;
using SharpPulsar.Builder;
using SharpPulsar.Common.Naming;
using SharpPulsar.Interfaces;
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

		private readonly ConcurrentDictionary<string, IActorRef> _readers;

		private readonly IList<Action<string, T>> _listeners;
        private IUntypedActorContext _context;
        private ILoggingAdapter _log;
        private readonly IActorRef _self;
        private IActorRef _replyTo;
        private bool _isPersistentTopic;
        private ITopicCompactionStrategy<T> _compactionStrategy;

        public TableViewActor(PulsarClient client, ISchema<T> schema, TableViewConfigurationData conf, ConcurrentDictionary<string, T> data)
		{
            _self = Self;
            _log = Context.GetLogger();
            _context = Context;
			_client = client;
			_schema = schema;
			_conf = conf;
			_data = data;
			_readers = new ConcurrentDictionary<string, IActorRef>();
			_listeners = new List<Action<string, T>>();
            _compactionStrategy = ITopicCompactionStrategy<T>.Load(conf.TopicCompactionStrategyClassName);
            _isPersistentTopic = conf.TopicName.StartsWith(TopicDomain.Persistent.ToString());

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
            
		}
        public static Props Prop(PulsarClient client, ISchema<T> schema, TableViewConfigurationData conf, ConcurrentDictionary<string, T> data)
        {
            return Props.Create(() => new TableViewActor<T>(client, schema, conf, data));
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
                    }
                    if (update)
                    {
                        try
                        {
                            if (null == cur)
                            {
                                _data.TryRemove(key, out _);
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
            await ReadAllExistingMessages(reader, startTime, messagesRead);
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

        private async ValueTask ReadAllExistingMessages(Reader<T> reader, long startTime, AtomicLong messagesRead)
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
                        HandleMessage(msg);
                        await ReadAllExistingMessages(reader, startTime, messagesRead);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Reader {reader.Topic} was interrupted while reading existing messages", ex.ToString());
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

                    await ReadTailMessages(reader);
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
                await ReadTailMessages(reader);
            }
            catch(Exception ex) 
            {
                _log.Error($"Reader {reader.Topic} was interrupted while reading tail messages.", ex.ToString());
                //await Self.GracefulStop(TimeSpan.FromSeconds(1));
                //((IInternalActorRef)Self).Stop();
            } 
        }
    }

}