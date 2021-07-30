using Akka.Actor;
using Akka.Util.Internal;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Precondition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
		private readonly Mode _subscriptionMode;
		private readonly IActorRef _lookup;
		protected internal NamespaceName NamespaceName;
		private ICancelable _recheckPatternTimeout = null;
		private IActorContext _context;
		private IActorRef _self;

		public PatternMultiTopicsConsumer(Regex topicsPattern, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ConsumerConfigurationData<T> conf, ISchema<T> schema, Mode subscriptionMode, ClientConfigurationData clientConfiguration) :base (stateActor, client, lookup, cnxPool, idGenerator, conf, Context.System.Scheduler.Advanced, schema, false, clientConfiguration)
		{
			_self = Self;
			_lookup = lookup;
			_context = Context;
			_topicsPattern = topicsPattern;
			_subscriptionMode = subscriptionMode;
			if(NamespaceName == null)
			{
				NamespaceName = GetNameSpaceFromPattern(topicsPattern);
			}
			Condition.CheckArgument(GetNameSpaceFromPattern(topicsPattern).ToString().Equals(NamespaceName.ToString()));
            _recheckPatternTimeout = Context.System.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromSeconds(Math.Max(1, Conf.PatternAutoDiscoveryPeriod)), async () => { await TopicReChecker(); });
            
        }
        public static Props Prop(Regex topicsPattern, IActorRef stateActor, IActorRef client, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ConsumerConfigurationData<T> conf, ISchema<T> schema, Mode subscriptionMode, ClientConfigurationData clientConfiguration)
        {
            return Props.Create(()=> new PatternMultiTopicsConsumer<T>(topicsPattern, stateActor, client, lookup, cnxPool, idGenerator, conf, schema, subscriptionMode, clientConfiguration));
        }
        
        private async ValueTask TopicReChecker()
		{
            try
            {
				var response = await _lookup.Ask<GetTopicsUnderNamespaceResponse>(new GetTopicsUnderNamespace(NamespaceName, _subscriptionMode)).ConfigureAwait(false);
				var topicsFound = response.Topics;
				var topics = _context.GetChildren().ToList();
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"Get topics under namespace {NamespaceName}, topics.size: {topics.Count}");
					TopicsMap.ForEach(t => _log.Debug($"Get topics under namespace {NamespaceName}, topic: {t.Key}"));
				}
				var newTopics = TopicsPatternFilter(topicsFound, _topicsPattern);
				var oldTopics = Topics;
				OnTopicsAdded(TopicsListsMinus(newTopics, oldTopics));
				OnTopicsRemoved(TopicsListsMinus(oldTopics, newTopics));
            }
			catch(Exception ex)
            {
				_log.Error(ex.ToString());
            }
            finally
            {
				_recheckPatternTimeout = _context.System.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromSeconds(Math.Max(1, Conf.PatternAutoDiscoveryPeriod)), async () => { await TopicReChecker(); });
            }
			if (_recheckPatternTimeout.IsCancellationRequested)
			{
				return;
			}
			
		}

		public virtual Regex Pattern
		{
			get
			{
				return _topicsPattern;
			}
		}

		private void OnTopicsRemoved(ICollection<string> removedTopics)
		{
			if (removedTopics.Count == 0)
			{
				return;
			}
			foreach(var t in removedTopics)
            {
				_self.Tell(new RemoveTopicConsumer(t));
            }
		}

		private void  OnTopicsAdded(ICollection<string> addedTopics)
		{
			if (addedTopics.Count == 0)
			{				
				return;
			}
			foreach(var t in addedTopics)
            {
                _self.Tell(new SubscribeAndCreateTopicIfDoesNotExist(t, false));
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
		// get topics, which are contained in list1, and not in list2
		internal IList<string> TopicsListsMinus(IList<string> list1, IList<string> list2)
		{
			var s1 = new HashSet<string>(list1);
            foreach (var l in list2)
            {
				s1.Remove(l);
            }
			return s1.ToList();
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