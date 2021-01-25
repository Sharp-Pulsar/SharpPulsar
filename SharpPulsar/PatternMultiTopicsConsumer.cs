using System;
using System.Collections.Generic;

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

	public class PatternMultiTopicsConsumer<T> : MultiTopicsConsumer<T>, TimerTask
	{
		private readonly Pattern _topicsPattern;
		private readonly TopicsChangedListener _topicsChangeListener;
		private readonly Mode _subscriptionMode;
		protected internal NamespaceName NamespaceName;
		private volatile Timeout _recheckPatternTimeout = null;

		public PatternMultiTopicsConsumer(Pattern topicsPattern, PulsarClientImpl client, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, CompletableFuture<Consumer<T>> subscribeFuture, Schema<T> schema, Mode subscriptionMode, ConsumerInterceptors<T> interceptors) : base(client, conf, listenerExecutor, subscribeFuture, schema, interceptors, false)
		{
			this._topicsPattern = topicsPattern;
			this._subscriptionMode = subscriptionMode;

			if(this.NamespaceName == null)
			{
				this.NamespaceName = GetNameSpaceFromPattern(topicsPattern);
			}
			checkArgument(GetNameSpaceFromPattern(topicsPattern).ToString().Equals(this.NamespaceName.ToString()));

			this._topicsChangeListener = new PatternTopicsChangedListener(this);
			this._recheckPatternTimeout = client.Timer().newTimeout(this, Math.Max(1, conf.PatternAutoDiscoveryPeriod), TimeUnit.SECONDS);
		}

		public static NamespaceName GetNameSpaceFromPattern(Pattern pattern)
		{
			return TopicName.Get(pattern.pattern()).NamespaceObject;
		}

		public override void Run(Timeout timeout)
		{
			if(timeout.Cancelled)
			{
				return;
			}

			CompletableFuture<Void> recheckFuture = new CompletableFuture<Void>();
			IList<CompletableFuture<Void>> futures = Lists.newArrayListWithExpectedSize(2);

			ClientConflict.Lookup.getTopicsUnderNamespace(NamespaceName, _subscriptionMode).thenAccept(TopicsConflict =>
			{
			if(_log.DebugEnabled)
			{
				_log.debug("Get topics under namespace {}, topics.size: {}", NamespaceName.ToString(), TopicsConflict.Count);
				TopicsConflict.forEach(topicName => _log.debug("Get topics under namespace {}, topic: {}", NamespaceName.ToString(), topicName));
			}
			IList<string> newTopics = PulsarClientImpl.TopicsPatternFilter(TopicsConflict, _topicsPattern);
			IList<string> oldTopics = PatternMultiTopicsConsumer.this.Topics;
			futures.Add(_topicsChangeListener.OnTopicsAdded(TopicsListsMinus(newTopics, oldTopics)));
			futures.Add(_topicsChangeListener.OnTopicsRemoved(TopicsListsMinus(oldTopics, newTopics)));
			FutureUtil.WaitForAll(futures).thenAccept(finalFuture => recheckFuture.complete(null)).exceptionally(ex =>
			{
				_log.warn("[{}] Failed to recheck topics change: {}", Topic, ex.Message);
				recheckFuture.completeExceptionally(ex);
				return null;
			});
			});

			// schedule the next re-check task
			this._recheckPatternTimeout = ClientConflict.Timer().newTimeout(PatternMultiTopicsConsumer.this, Math.Max(1, Conf.PatternAutoDiscoveryPeriod), TimeUnit.SECONDS);
		}

		public virtual Pattern Pattern
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

		private class PatternTopicsChangedListener : TopicsChangedListener
		{
			private readonly PatternMultiTopicsConsumer<T> _outerInstance;

			public PatternTopicsChangedListener(PatternMultiTopicsConsumer<T> outerInstance)
			{
				this._outerInstance = outerInstance;
			}

			public virtual CompletableFuture<Void> OnTopicsRemoved(ICollection<string> removedTopics)
			{
				CompletableFuture<Void> removeFuture = new CompletableFuture<Void>();

				if(removedTopics.Count == 0)
				{
					removeFuture.complete(null);
					return removeFuture;
				}

				IList<CompletableFuture<Void>> futures = Lists.newArrayListWithExpectedSize(outerInstance.TopicsConflict.Count);
				removedTopics.ForEach(outerInstance.Topic => futures.Add(outerInstance.RemoveConsumerAsync(outerInstance.Topic)));
				FutureUtil.WaitForAll(futures).thenAccept(finalFuture => removeFuture.complete(null)).exceptionally(ex =>
				{
				_log.warn("[{}] Failed to subscribe topics: {}", outerInstance.Topic, ex.Message);
				removeFuture.completeExceptionally(ex);
				return null;
				});
				return removeFuture;
			}

			public virtual CompletableFuture<Void> OnTopicsAdded(ICollection<string> addedTopics)
			{
				CompletableFuture<Void> addFuture = new CompletableFuture<Void>();

				if(addedTopics.Count == 0)
				{
					addFuture.complete(null);
					return addFuture;
				}

				IList<CompletableFuture<Void>> futures = Lists.newArrayListWithExpectedSize(outerInstance.TopicsConflict.Count);
				addedTopics.ForEach(outerInstance.Topic => futures.Add(outerInstance.SubscribeAsync(outerInstance.Topic, false)));
				FutureUtil.WaitForAll(futures).thenAccept(finalFuture => addFuture.complete(null)).exceptionally(ex =>
				{
				_log.warn("[{}] Failed to unsubscribe topics: {}", outerInstance.Topic, ex.Message);
				addFuture.completeExceptionally(ex);
				return null;
				});
				return addFuture;
			}
		}

		// get topics, which are contained in list1, and not in list2
		public static IList<string> TopicsListsMinus(IList<string> list1, IList<string> list2)
		{
			HashSet<string> s1 = new HashSet<string>(list1);
			s1.RemoveAll(list2);
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