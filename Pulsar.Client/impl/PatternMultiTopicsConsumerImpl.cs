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
namespace org.apache.pulsar.client.impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;
	using Lists = com.google.common.collect.Lists;
	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;
	using Consumer = org.apache.pulsar.client.api.Consumer;
	using Schema = org.apache.pulsar.client.api.Schema;
	using org.apache.pulsar.client.impl.conf;
	using Mode = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetTopicsOfNamespace.Mode;
	using NamespaceName = org.apache.pulsar.common.naming.NamespaceName;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class PatternMultiTopicsConsumerImpl<T> : MultiTopicsConsumerImpl<T>, TimerTask
	{
		private readonly Pattern topicsPattern;
		private readonly TopicsChangedListener topicsChangeListener;
		private readonly Mode subscriptionMode;
		private volatile Timeout recheckPatternTimeout = null;

		public PatternMultiTopicsConsumerImpl(Pattern topicsPattern, PulsarClientImpl client, ConsumerConfigurationData<T> conf, ExecutorService listenerExecutor, CompletableFuture<Consumer<T>> subscribeFuture, Schema<T> schema, Mode subscriptionMode, ConsumerInterceptors<T> interceptors) : base(client, conf, listenerExecutor, subscribeFuture, schema, interceptors, false)
		{
			this.topicsPattern = topicsPattern;
			this.subscriptionMode = subscriptionMode;

			if (this.namespaceName == null)
			{
				this.namespaceName = getNameSpaceFromPattern(topicsPattern);
			}
			checkArgument(getNameSpaceFromPattern(topicsPattern).ToString().Equals(this.namespaceName.ToString()));

			this.topicsChangeListener = new PatternTopicsChangedListener(this);
			recheckPatternTimeout = client.timer().newTimeout(this, Math.Min(1, conf.PatternAutoDiscoveryPeriod), TimeUnit.MINUTES);
		}

		public static NamespaceName getNameSpaceFromPattern(Pattern pattern)
		{
			return TopicName.get(pattern.pattern()).NamespaceObject;
		}

		// TimerTask to recheck topics change, and trigger subscribe/unsubscribe based on the change.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
		public override void run(Timeout timeout)
		{
			if (timeout.Cancelled)
			{
				return;
			}

			CompletableFuture<Void> recheckFuture = new CompletableFuture<Void>();
			IList<CompletableFuture<Void>> futures = Lists.newArrayListWithExpectedSize(2);

			client.Lookup.getTopicsUnderNamespace(namespaceName, subscriptionMode).thenAccept(topics.Value =>
			{
			if (log.DebugEnabled)
			{
				log.debug("Get topics under namespace {}, topics.size: {}", namespaceName.ToString(), topics.Count);
				topics.forEach(topicName => log.debug("Get topics under namespace {}, topic: {}", namespaceName.ToString(), topicName));
			}
			IList<string> newTopics = PulsarClientImpl.topicsPatternFilter(topics, topicsPattern);
			IList<string> oldTopics = PatternMultiTopicsConsumerImpl.this.Topics;
			futures.Add(topicsChangeListener.onTopicsAdded(topicsListsMinus(newTopics, oldTopics)));
			futures.Add(topicsChangeListener.onTopicsRemoved(topicsListsMinus(oldTopics, newTopics)));
			FutureUtil.waitForAll(futures).thenAccept(finalFuture => recheckFuture.complete(null)).exceptionally(ex =>
			{
				log.warn("[{}] Failed to recheck topics change: {}", topic, ex.Message);
				recheckFuture.completeExceptionally(ex);
				return null;
			});
			});

			// schedule the next re-check task
			recheckPatternTimeout = client.timer().newTimeout(PatternMultiTopicsConsumerImpl.this, Math.Min(1, conf.PatternAutoDiscoveryPeriod), TimeUnit.MINUTES);
		}

		public virtual Pattern Pattern
		{
			get
			{
				return this.topicsPattern;
			}
		}

		internal interface TopicsChangedListener
		{
			// unsubscribe and delete ConsumerImpl in the `consumers` map in `MultiTopicsConsumerImpl` based on added topics.
			CompletableFuture<Void> onTopicsRemoved(ICollection<string> removedTopics);
			// subscribe and create a list of new ConsumerImpl, added them to the `consumers` map in `MultiTopicsConsumerImpl`.
			CompletableFuture<Void> onTopicsAdded(ICollection<string> addedTopics);
		}

		private class PatternTopicsChangedListener : TopicsChangedListener
		{
			private readonly PatternMultiTopicsConsumerImpl<T> outerInstance;

			public PatternTopicsChangedListener(PatternMultiTopicsConsumerImpl<T> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual CompletableFuture<Void> onTopicsRemoved(ICollection<string> removedTopics)
			{
				CompletableFuture<Void> removeFuture = new CompletableFuture<Void>();

				if (removedTopics.Count == 0)
				{
					removeFuture.complete(null);
					return removeFuture;
				}

				IList<CompletableFuture<Void>> futures = Lists.newArrayListWithExpectedSize(outerInstance.topics.Count);
				removedTopics.ForEach(outerInstance.topic => futures.Add(outerInstance.removeConsumerAsync(outerInstance.topic)));
				FutureUtil.waitForAll(futures).thenAccept(finalFuture => removeFuture.complete(null)).exceptionally(ex =>
				{
				log.warn("[{}] Failed to subscribe topics: {}", outerInstance.topic, ex.Message);
				removeFuture.completeExceptionally(ex);
				return null;
				});
				return removeFuture;
			}

			public virtual CompletableFuture<Void> onTopicsAdded(ICollection<string> addedTopics)
			{
				CompletableFuture<Void> addFuture = new CompletableFuture<Void>();

				if (addedTopics.Count == 0)
				{
					addFuture.complete(null);
					return addFuture;
				}

				IList<CompletableFuture<Void>> futures = Lists.newArrayListWithExpectedSize(outerInstance.topics.Count);
				addedTopics.ForEach(outerInstance.topic => futures.Add(outerInstance.subscribeAsync(outerInstance.topic, false)));
				FutureUtil.waitForAll(futures).thenAccept(finalFuture => addFuture.complete(null)).exceptionally(ex =>
				{
				log.warn("[{}] Failed to unsubscribe topics: {}", outerInstance.topic, ex.Message);
				addFuture.completeExceptionally(ex);
				return null;
				});
				return addFuture;
			}
		}

		// get topics, which are contained in list1, and not in list2
		public static IList<string> topicsListsMinus(IList<string> list1, IList<string> list2)
		{
			HashSet<string> s1 = new HashSet<string>(list1);
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
			s1.removeAll(list2);
			return s1.ToList();
		}

		public override CompletableFuture<Void> closeAsync()
		{
			Timeout timeout = recheckPatternTimeout;
			if (timeout != null)
			{
				timeout.cancel();
				recheckPatternTimeout = null;
			}
			return base.closeAsync();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting Timeout getRecheckPatternTimeout()
		internal virtual Timeout RecheckPatternTimeout
		{
			get
			{
				return recheckPatternTimeout;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(PatternMultiTopicsConsumerImpl));
	}

}