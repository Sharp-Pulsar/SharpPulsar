using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util.Internal;
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

		private readonly ConcurrentDictionary<string, T> _data;

		private readonly ConcurrentDictionary<string, IActorRef> _readers;

		private readonly IList<Action<string, T>> _listeners;
        private IUntypedActorContext _context;
        private ILoggingAdapter _log;
        private ICancelable _partitionChecker;
        private readonly IActorRef _self;
        private IActorRef _replyTo;
        private bool _isPersistentTopic;

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
            _isPersistentTopic = conf.TopicName.StartsWith(TopicDomain.Persistent.ToString());
            Receive<HandleMessage<T>>(hm =>
            {
                try
                {
                    var msg = hm.Message;
                    if (!string.IsNullOrWhiteSpace(msg.Key))
                    {
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug($"Applying message from topic {_conf.TopicName}. key={msg.Key} value={msg.Value}");
                        }

                        _data.TryAdd(msg.Key, msg.Value);

                        foreach (var listener in _listeners)
                        {
                            try
                            {
                                listener(msg.Key, msg.Value);
                            }
                            catch (Exception t)
                            {
                                _log.Error($"Table view listener raised an exception: {t}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.ToString());
                }
            });
            Receive<ForEachAction<T>>(a => ForEachAndListen(a.Action));
            ReceiveAsync<StartMessage>(async _ =>
            {
                _replyTo = Sender;
                try
                {                    
                    await Start();
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
		private async ValueTask Start()
		{
            var partitions = await _client.GetPartitionsForTopicAsync(_conf.TopicName);
            var partitionsSet = new HashSet<string>(partitions);
            var partitionTasks = new List<Task>();  
            foreach(var partition in partitions)
            {
                partitionTasks.Add(CreateReader(partition).AsTask());
            }
            await Task.WhenAll(partitionTasks)
                .ContinueWith(async _ => 
                { 
                    foreach(var kv in _readers)
                    {
                        if (!partitionsSet.Contains(kv.Key))
                        {
                            await kv.Value.GracefulStop(TimeSpan.FromSeconds(1));
                            _readers.Remove(kv.Key, out var _);
                        }
                    }
                }); 
            SchedulePartitionsCheck();
        }
        private async ValueTask CreateReader(string partition)
        {
            var r = await NewReader(partition);
            _readers.TryAdd(partition, r);
        }
		private void SchedulePartitionsCheck()
		{
			_partitionChecker = _context.System.Scheduler
                .Advanced
                .ScheduleOnceCancelable(_conf.AutoUpdatePartitionsSeconds, async () => await CheckForPartitionsChanges());
		}

		private async ValueTask CheckForPartitionsChanges()
		{
			if (_partitionChecker.IsCancellationRequested)
			{
				return;
			}
            try
            {
                await Start();
                
            }
            catch (Exception ex)
            {
                _log.Warning($"Failed to check for changes in number of partitions:{ex}");
            }
            finally
            {
                SchedulePartitionsCheck();
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
            _readers.Values.ForEach(r =>
            {
                r.Tell(PoisonPill.Instance);
            });
            base.PostStop();
        }

		private async ValueTask<IActorRef> NewReader(string partition)
		{
            var readerBuilder = new ReaderConfigBuilder<T>()
                .Topic(_conf.TopicName)
                .StartMessageId(IMessageId.Earliest)
                .AutoUpdatePartitions(true)
                .AutoUpdatePartitionsInterval(_conf.AutoUpdatePartitionsSeconds)
                .PoolMessages(true)
                .ReaderName(_conf.SubscriptionName)
                .ReadCompacted(true);

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

            var reader = await _client.NewReaderAsync(_schema, readerBuilder);
            return _context.ActorOf(PartitionReader<T>.Prop(reader));
		}

	}

}