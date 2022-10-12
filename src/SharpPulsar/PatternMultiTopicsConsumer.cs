using Akka;
using Akka.Actor;
using Akka.Util.Internal;
using DotNetty.Common.Utilities;
using Google.Protobuf.Collections;
using SharpPulsar.Common;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Precondition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Joins;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static SharpPulsar.PatternMultiTopicsConsumer<T>;
using static SharpPulsar.Protocol.Proto.CommandGetTopicsOfNamespace;

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
namespace SharpPulsar
{

    internal class PatternMultiTopicsConsumer<T>: MultiTopicsConsumer<T>
	{
		private readonly Regex _topicsPattern;
        private  string _topicsHash;
		private readonly Mode _subscriptionMode;
		private readonly IActorRef _lookup;
		protected internal NamespaceName NamespaceName;
		private ICancelable _recheckPatternTimeout = null;
		private readonly IActorContext _context;
		private readonly IActorRef _self;

        private readonly ITopicsChangedListener _topicsChangeListener;
        private readonly ValueTask<TopicListWatcher> _watcherFuture;

        public PatternMultiTopicsConsumer(Regex topicsPattern, string topicsHash, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ConsumerConfigurationData<T> conf, ISchema<T> schema, Mode subscriptionMode, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture) :base (stateActor, client, lookup, cnxPool, idGenerator, conf, schema, false, clientConfiguration, subscribeFuture)
		{
			_self = Self;
			_lookup = lookup;
			_context = Context;
			_topicsPattern = topicsPattern;
            _topicsHash = topicsHash;
            _subscriptionMode = subscriptionMode;
			if(NamespaceName == null)
			{
				NamespaceName = GetNameSpaceFromPattern(topicsPattern);
			}
            Condition.CheckArgument(GetNameSpaceFromPattern(topicsPattern).ToString().Equals(NamespaceName.ToString()));
            _recheckPatternTimeout = Context.System.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromSeconds(Math.Max(1, Conf.PatternAutoDiscoveryPeriod)), async () => { await Run(); });
            _topicsChangeListener = new PatternTopicsChangedListener(this);
            
            _watcherFuture = new CompletableFuture<TopicListWatcher>();
            if (_subscriptionMode == Mode.Persistent)
            {
                long WatcherId = Client.NewTopicListWatcherId();
                new TopicListWatcher(_topicsChangeListener, client, topicsPattern, WatcherId, NamespaceName, topicsHash, State);
                watcherFuture.exceptionally(ex =>
                {
                    log.debug("Unable to create topic list watcher. Falling back to only polling for new topics", ex);
                    return null;
                });
            }
            else
            {
                log.debug("Not creating topic list watcher for subscription mode {}", SubscriptionMode);
                watcherFuture.complete(null);
            }
            ReceiveAsync<TopicsAdded>(async t =>
            {
                var addedTopics = t.AddedTopics;
                if (addedTopics.Count == 0)
                {
                    return;
                }
                foreach (var add in addedTopics)
                {
                    await Subscribe(add, false);
                }
            });
            Receive<TopicsRemoved>(t =>
            {
                var removedTopics = t.RemovedTopics;
                if (removedTopics.Count == 0)
                {
                    return;
                }

                var futures = new List<ValueTask>(PartitionedTopics.Count);

                removedTopics.ForEach(delegate (string topic)
                {
                    _self.Tell(new RemoveTopicConsumer(topic));
                });
            });

        }
        public static Props Prop(Regex topicsPattern, string topicsHash, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ConsumerConfigurationData<T> conf, ISchema<T> schema, Mode subscriptionMode, ClientConfigurationData clientConfiguration, TaskCompletionSource<IActorRef> subscribeFuture)
        {
            return Props.Create(()=> new PatternMultiTopicsConsumer<T>(topicsPattern, topicsHash, stateActor, client, lookup, cnxPool, idGenerator, conf, schema, subscriptionMode, clientConfiguration, subscribeFuture));
        }
        
        private async ValueTask Run()
		{
            var topics = _context.GetChildren().ToList();
            try
            {
				var ask = await _lookup.Ask<AskResponse>(new GetTopicsUnderNamespace(NamespaceName, _subscriptionMode, _topicsPattern.Match().ToString(), _topicsHash)).ConfigureAwait(false);
                var response = ask.ConvertTo<GetTopicsUnderNamespaceResponse>();
                var topicsFound = response.Topics;
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"Get topics under namespace {NamespaceName}, topics.size: {topics.Count}, topicsHash: {response.TopicsHash}, filtered: {response.GetHashCode}");
					PartitionedTopics.ForEach(t => _log.Debug($"Get topics under namespace {NamespaceName}, topic: {t.Key}"));
				}
                IList<string> oldTopics = new List<string>();
                foreach (string partition in response.Topics)
                {
                    var topicName = TopicName.Get(partition);

                    if (!topicName.Partitioned || !oldTopics.Contains(topicName.PartitionedTopicName))
                    {
                        oldTopics.Add(partition);
                    }
                }
                await UpdateSubscriptions(_topicsPattern, response, _topicsChangeListener, oldTopics);
               
            }
			catch(Exception ex)
            {
				_log.Error($"[{topics}] Failed to recheck topics change: {ex}");
            }
            finally
            {
				_recheckPatternTimeout = _context.System.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromSeconds(Math.Max(1, Conf.PatternAutoDiscoveryPeriod)), async () => { await Run(); });
            }
			if (_recheckPatternTimeout.IsCancellationRequested)
			{
				return;
			}
			
		}
        private async ValueTask UpdateSubscriptions(Regex topicsPattern, GetTopicsUnderNamespaceResponse result, ITopicsChangedListener topicsChangedListener, IList<string> oldTopics)
        {
            _topicsHash = result.TopicsHash;
            if (!result.Changed)
            {
                return ;
            }

            IList<string> newTopics;
            if (result.Filtered)
            {
                newTopics = result.Topics;
            }
            else
            {
                newTopics = TopicList.FilterTopics(result.Topics, topicsPattern);
            }

            await topicsChangedListener.OnTopicsAdded(TopicList.Minus(newTopics, oldTopics));
            topicsChangedListener.OnTopicsRemoved(TopicList.Minus(oldTopics, newTopics));
        }
        public virtual Regex Pattern
		{
			get
			{
				return _topicsPattern;
			}
		}
        private string TopicsHash
        {
            set
            {
                _topicsHash = value;
            }
        }
      
		private NamespaceName GetNameSpaceFromPattern(Regex pattern)
		{
			return TopicName.Get(pattern.ToString()).NamespaceObject;
		}
		// get topics that match 'topicsPattern' from original topics list
		// return result should contain only topic names, without partition part
		private IList<string> TopicsPatternFilter(IList<string> original, Regex topicsPattern)
		{
			var pattern = topicsPattern.ToString().Contains("://") ? new Regex(Regex.Split(topicsPattern.ToString(), @"\:\/\/")[1]) : topicsPattern;

			return original.Select(TopicName.Get).Select(x => x.ToString()).Where(topic => pattern.Match(Regex.Split(topic, @"\:\/\/")[1]).Success).ToList();
		}
		
        protected override void PostStop()
        {
			_recheckPatternTimeout?.Cancel();
			base.PostStop();
        }
	}
	public sealed class RecheckTopics
    {
		public static RecheckTopics Instance = new RecheckTopics();
    }
}