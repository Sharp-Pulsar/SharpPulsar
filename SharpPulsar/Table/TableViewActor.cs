using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using SharpPulsar.Builder;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Table.Messages;
using SharpPulsar.User;

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
            Receive<HandleMessage<T>>(hm => Handle(hm.Message));
            Receive<ForEachAction<T>>(a => ForEachAndListen(a.Action));
            ReceiveAsync<StartMessage>(async _ =>
            {
                var sender = Sender;
                try
                {                    
                    await Start();
                    sender.Tell(new AskResponse());
                }
                catch(Exception ex)
                {

                    sender.Tell(new AskResponse(ex));
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
            partitions.ForEach(async partition =>
            {
                if (!_readers.ContainsKey(partition))
                {
                    var r = await NewReader(partition);
                    _readers.TryAdd(partition, r);
                }
            });
            _readers.ForEach( async kv =>
            {
                if (!partitionsSet.Contains(kv.Key))
                {
                    await kv.Value.GracefulStop(TimeSpan.FromSeconds(1));
                    _readers.Remove(kv.Key, out var _);
                }
            });
            SchedulePartitionsCheck();
        }

		private void SchedulePartitionsCheck()
		{
			_partitionChecker = _context.System.Scheduler
                .Advanced
                .ScheduleRepeatedlyCancelable(TimeSpan.FromSeconds(5), _conf.AutoUpdatePartitionsSeconds, () => CheckForPartitionsChanges());
		}

		private void CheckForPartitionsChanges()
		{
			if (_partitionChecker.IsCancellationRequested)
			{
				return;
			}
            try
            {
                Start();
                
            }
            catch (Exception ex)
            {
                _log.Warning($"Failed to check for changes in number of partitions:{ex}");
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
		private void Handle(IMessage<T> msg)
		{
            if (msg.HasKey())
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"Applying message from topic {_conf.TopicName}. key={msg.Key} value={msg.Value}");
                }

                try
                {
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
                finally
                {

                }
            }
        }

		private async ValueTask<IActorRef> NewReader(string partition)
		{
            var readerBuilder = new ReaderConfigBuilder<T>()
                .Topic(partition)
                .StartMessageId(IMessageId.Earliest)
                .ReadCompacted(true);
			var reader = await _client.NewReaderAsync(_schema, readerBuilder);
            return _context.ActorOf(PartitionReader<T>.Prop(reader));
		}

	}

}