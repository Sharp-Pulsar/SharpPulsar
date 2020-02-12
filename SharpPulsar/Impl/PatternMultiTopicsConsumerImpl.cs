using System;
using System.Collections.Generic;
using System.Linq;
using DotNetty.Common.Utilities;
using Microsoft.Extensions.Logging;
using SharpPulsar.Common.Naming;
using SharpPulsar.Util;

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
namespace SharpPulsar.Impl
{
    using Api;
    using Conf;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using static Protocol.Proto.CommandGetTopicsOfNamespace.Types;

    public class PatternMultiTopicsConsumerImpl<T> : MultiTopicsConsumerImpl<T>, ITimerTask
	{
		private readonly Regex _topicsPattern;
		private readonly TopicsChangedListener _topicsChangeListener;
		private readonly Mode _subscriptionMode;
		private volatile ITimeout _recheckPatternTimeout;

		public PatternMultiTopicsConsumerImpl(Regex topicsPattern, PulsarClientImpl client, ConsumerConfigurationData<T> conf, TaskCompletionSource<IConsumer<T>> subscribeTask, ISchema<T> schema, Mode subscriptionMode, ConsumerInterceptors<T> interceptors, ScheduledThreadPoolExecutor executor) : base(client, conf, subscribeTask, schema, interceptors, false, executor)
		{
			this._topicsPattern = topicsPattern;
			this._subscriptionMode = subscriptionMode;

			if (Namespacename == null)
			{
				Namespacename = GetNameSpaceFromPattern(topicsPattern);
			}
			if(!GetNameSpaceFromPattern(topicsPattern).ToString().Equals(Namespacename.ToString()))
				throw new ArgumentException();

			_topicsChangeListener = new PatternTopicsChangedListener(this);
			_recheckPatternTimeout = client.Timer.NewTimeout(this, TimeSpan.FromMinutes(Math.Min(1, conf.PatternAutoDiscoveryPeriod)));
		}

		public static NamespaceName GetNameSpaceFromPattern(Regex pattern)
		{
			return TopicName.Get(pattern.ToString()).NamespaceObject;
		}

		public new void Run(ITimeout timeout)
		{
			if (timeout.Canceled)
			{
				return;
			}

			var recheckTask = new TaskCompletionSource<Task>();
			IList<Task> tasks = new List<Task>(2);

			Client.Lookup.GetTopicsUnderNamespace(Namespacename, _subscriptionMode).AsTask().ContinueWith(task =>
            {
                var topics = task.Result.ToList();
				if (Log.IsEnabled(LogLevel.Debug))
			    {
				    Log.LogDebug("Get topics under namespace {}, topics.size: {}", Namespacename.ToString(), topics.Count);
				    topics.ForEach(topicName => Log.LogDebug("Get topics under namespace {}, topic: {}", Namespacename.ToString(), topicName));
			    }
			    var newTopics = PulsarClientImpl.TopicsPatternFilter(topics, _topicsPattern);
			    var oldTopics = Topics;
			    tasks.Add(_topicsChangeListener.OnTopicsAdded(TopicsListsMinus(newTopics, oldTopics)).AsTask());
			    tasks.Add(_topicsChangeListener.OnTopicsRemoved(TopicsListsMinus(oldTopics, newTopics)).AsTask());
			    Task.WhenAll(tasks).ContinueWith(finalTask =>
                {
                    if (finalTask.IsFaulted)
                    {
                        Log.LogWarning("[{}] Failed to recheck topics change: {}", Topic, finalTask.Exception.Message);
                        recheckTask.SetException(finalTask.Exception);
                        return;
					}
                    recheckTask.SetResult(null);

                });
			});

			// schedule the next re-check task
			_recheckPatternTimeout = Client.Timer.NewTimeout(this, TimeSpan.FromMinutes(Math.Min(1, Conf.PatternAutoDiscoveryPeriod)));
		}

		public virtual Regex Pattern => _topicsPattern;

        public interface TopicsChangedListener
		{
			// unsubscribe and delete ConsumerImpl in the `consumers` map in `MultiTopicsConsumerImpl` based on added topics.
			ValueTask OnTopicsRemoved(ICollection<string> removedTopics);
			// subscribe and create a list of new ConsumerImpl, added them to the `consumers` map in `MultiTopicsConsumerImpl`.
			ValueTask OnTopicsAdded(ICollection<string> addedTopics);
		}

		public class PatternTopicsChangedListener : TopicsChangedListener
		{
			private readonly PatternMultiTopicsConsumerImpl<T> _outerInstance;

			public PatternTopicsChangedListener(PatternMultiTopicsConsumerImpl<T> outerInstance)
			{
				this._outerInstance = outerInstance;
			}

			public ValueTask OnTopicsRemoved(ICollection<string> removedTopics)
			{
				var removeTask = new TaskCompletionSource<Task>();

				if (removedTopics.Count == 0)
				{
					removeTask.SetResult(null);
					return new ValueTask(removeTask.Task);
				}

				var tasks = new List<Task>(_outerInstance.Topics.Count);
				removedTopics.ToList().ForEach(x => tasks.Add(_outerInstance.RemoveConsumerAsync(x).AsTask()));
				Task.WhenAll(tasks).ContinueWith(finalTask =>
                {
                    if (finalTask.IsFaulted)
                    {
                        var ex = finalTask.Exception;
                        Log.LogWarning("[{}] Failed to subscribe topics: {}", _outerInstance.Topic, ex.Message);
                        removeTask.SetException(ex);
                        return;
					}

                    removeTask.SetResult(null);
                });
				return new ValueTask(removeTask.Task); 
			}

			public ValueTask OnTopicsAdded(ICollection<string> addedTopics)
			{
				var addTask = new TaskCompletionSource<Task>();

				if (addedTopics.Count == 0)
				{
					addTask.SetResult(null);
					return new ValueTask(addTask.Task); 
				}

				var tasks = new List<Task>(_outerInstance.Topics.Count);
				addedTopics.ToList().ForEach(x => tasks.Add(_outerInstance.SubscribeAsync(_outerInstance.Topic, false)));
				Task.WhenAll(tasks).ContinueWith(finalTask =>
                {
                    if (finalTask.IsFaulted)
                    {
                        var ex = finalTask.Exception;
                        Log.LogWarning("[{}] Failed to unsubscribe topics: {}", _outerInstance.Topic, ex.Message);
                        addTask.SetException(ex);
                        return;
					}
                    addTask.SetResult(null);
                });
				return new ValueTask(addTask.Task); 
			}
		}

		// get topics, which are contained in list1, and not in list2
		public static IList<string> TopicsListsMinus(IList<string> list1, IList<string> list2)
		{
			var s1 = new HashSet<string>(list1);
			list2.ToList().ForEach(x => s1.Remove(x));
			return s1.ToList();
		}

		public override ValueTask CloseAsync()
		{
			var timeout = _recheckPatternTimeout;
			if (timeout != null)
			{
				timeout.Cancel();
				_recheckPatternTimeout = null;
			}
			return base.CloseAsync();
		}

		public virtual ITimeout RecheckPatternTimeout => _recheckPatternTimeout;

        private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(PatternMultiTopicsConsumerImpl<T>));
	}

}