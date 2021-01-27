using Akka.Actor;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Precondition;
using SharpPulsar.Queues;
using SharpPulsar.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

	public class PatternMultiTopicsConsumer<T> : MultiTopicsConsumer<T>
	{
		private readonly Regex _topicsPattern;
		private readonly TopicsChangedListener _topicsChangeListener;
		private readonly Mode _subscriptionMode;
		private readonly IActorRef _client;
		protected internal NamespaceName NamespaceName;
		private ICancelable _recheckPatternTimeout = null;
		private IActorContext _context;
		private HashSet<string> _discoveredTopics;
		private ISchema<T> _schema;

		public PatternMultiTopicsConsumer(Regex topicsPattern, IActorRef client, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, ISchema<T> schema, Mode subscriptionMode, ConsumerInterceptors<T> interceptors, ClientConfigurationData clientConfiguration, ConsumerQueueCollections<T> queue) :base (client, true,conf, listenerExecutor, schema, interceptors, false, clientConfiguration, queue)
		{
			_schema = schema;
			_discoveredTopics = new HashSet<string>();
			_context = Context;
			_topicsPattern = topicsPattern;
			_subscriptionMode = subscriptionMode;

			if(this.NamespaceName == null)
			{
				this.NamespaceName = GetNameSpaceFromPattern(topicsPattern);
			}
			Condition.CheckArgument(GetNameSpaceFromPattern(topicsPattern).ToString().Equals(this.NamespaceName.ToString()));

			this._topicsChangeListener = new PatternTopicsChangedListener(this);
			_recheckPatternTimeout = client.Timer().newTimeout(this, Math.Max(1, conf.PatternAutoDiscoveryPeriod), TimeUnit.SECONDS);
		}


		public override void Run()
		{
			if(_recheckPatternTimeout.IsCancellationRequested)
			{
				return;
			}

			var topicsFound = _client.AskFor<GetTopicsOfNamespaceResponse>(new GetTopicsUnderNamespace(NamespaceName, _subscriptionMode)).Response.Topics;
			var topics = _context.GetChildren().ToList();
			if (_log.IsDebugEnabled)
			{
				_log.Debug($"Get topics under namespace {NamespaceName}, topics.size: {topics.Count}");
				_discoveredTopics.ToList().ForEach(topicName => _log.Debug($"Get topics under namespace {NamespaceName}, topic: {topicName}"));
			}
			IList<string> newTopics = TopicsPatternFilter(topicsFound, _topicsPattern);
			IList<string> oldTopics = _discoveredTopics.ToList();
			futures.Add(_topicsChangeListener.OnTopicsAdded(TopicsListsMinus(newTopics, oldTopics)));
			futures.Add(_topicsChangeListener.OnTopicsRemoved(TopicsListsMinus(oldTopics, newTopics)));
			FutureUtil.WaitForAll(futures).thenAccept(finalFuture => recheckFuture.complete(null)).exceptionally(ex =>
			{
				_log.warn("[{}] Failed to recheck topics change: {}", Topic, ex.Message);
				recheckFuture.completeExceptionally(ex);
				return null;
			});
			// schedule the next re-check task
			this._recheckPatternTimeout = ClientConflict.Timer().newTimeout(PatternMultiTopicsConsumer.this, Math.Max(1, Conf.PatternAutoDiscoveryPeriod), TimeUnit.SECONDS);
		}

		public virtual Regex Pattern
		{
			get
			{
				return this._topicsPattern;
			}
		}

		internal interface TopicsChangedListener
		{
			// unsubscribe and delete ConsumerImpl in the `consumers` map in `MultiTopicsConsumerImpl` based on added topics.
			CompletableFuture<Void> OnTopicsRemoved(ICollection<string> removedTopics);
			// subscribe and create a list of new ConsumerImpl, added them to the `consumers` map in `MultiTopicsConsumerImpl`.
			CompletableFuture<Void> OnTopicsAdded(ICollection<string> addedTopics);
		}

		private void OnTopicsRemoved(ICollection<string> removedTopics)
		{
			if (removedTopics.Count == 0)
			{
				return;
			}
			foreach(var t in removedTopics)
            {
				var name = t.ToAkkaNaming();
				var child = _context.Child(name);
				if (!child.IsNobody())
					child.GracefulStop(TimeSpan.FromSeconds(2));
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
				var name = t.ToAkkaNaming();
				_context.ActorOf(MultiTopicsConsumer<T>.NewMultiTopicsConsumer(_client, t, Conf, ListenerExecutor, false, _schema, Interceptors, clientConfiguration, ConsumerQueue, true), name);
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
		public static IList<string> TopicsListsMinus(IList<string> list1, IList<string> list2)
		{
			HashSet<string> s1 = new HashSet<string>(list1);
            foreach (var l in list2)
            {
				s1.Remove(l);
            }
			return s1.ToList();
		}

		public override CompletableFuture<Void> CloseAsync()
		{
			Timeout timeout = _recheckPatternTimeout;
			if(timeout != null)
			{
				timeout.cancel();
				_recheckPatternTimeout = null;
			}
			return base.CloseAsync();
		}

		internal virtual Timeout RecheckPatternTimeout
		{
			get
			{
				return _recheckPatternTimeout;
			}
		}

	}

}