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
namespace SharpPulsar.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;
	using Lists = com.google.common.collect.Lists;
	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;
	using Consumer = SharpPulsar.Api.Consumer;
	using SharpPulsar.Api;
	using SharpPulsar.Impl.Conf;
	using Mode = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandGetTopicsOfNamespace.Mode;
	using NamespaceName = Org.Apache.Pulsar.Common.Naming.NamespaceName;
	using TopicName = Org.Apache.Pulsar.Common.Naming.TopicName;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class PatternMultiTopicsConsumerImpl<T> : MultiTopicsConsumerImpl<T>, TimerTask
	{
		private readonly Pattern topicsPattern;
		private readonly TopicsChangedListener topicsChangeListener;
		private readonly Mode subscriptionMode;
		private volatile Timeout recheckPatternTimeout = null;

		public PatternMultiTopicsConsumerImpl(Pattern TopicsPattern, PulsarClientImpl Client, ConsumerConfigurationData<T> Conf, ExecutorService ListenerExecutor, CompletableFuture<Consumer<T>> SubscribeFuture, Schema<T> Schema, Mode SubscriptionMode, ConsumerInterceptors<T> Interceptors) : base(Client, Conf, ListenerExecutor, SubscribeFuture, Schema, Interceptors, false)
		{
			this.topicsPattern = TopicsPattern;
			this.subscriptionMode = SubscriptionMode;

			if (this.NamespaceName == null)
			{
				this.NamespaceName = GetNameSpaceFromPattern(TopicsPattern);
			}
			checkArgument(GetNameSpaceFromPattern(TopicsPattern).ToString().Equals(this.NamespaceName.ToString()));

			this.topicsChangeListener = new PatternTopicsChangedListener(this);
			recheckPatternTimeout = Client.timer().newTimeout(this, Math.Min(1, Conf.PatternAutoDiscoveryPeriod), BAMCIS.Util.Concurrent.TimeUnit.MINUTES);
		}

		public static NamespaceName GetNameSpaceFromPattern(Pattern Pattern)
		{
			return TopicName.get(Pattern.pattern()).NamespaceObject;
		}

		// TimerTask to recheck topics change, and trigger subscribe/unsubscribe based on the change.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
		public override void Run(Timeout Timeout)
		{
			if (Timeout.Cancelled)
			{
				return;
			}

			CompletableFuture<Void> RecheckFuture = new CompletableFuture<Void>();
			IList<CompletableFuture<Void>> Futures = Lists.newArrayListWithExpectedSize(2);

			ClientConflict.Lookup.getTopicsUnderNamespace(NamespaceName, subscriptionMode).thenAccept(TopicsConflict.Value =>
			{
			if (log.DebugEnabled)
			{
				log.debug("Get topics under namespace {}, topics.size: {}", NamespaceName.ToString(), TopicsConflict.Count);
				TopicsConflict.forEach(topicName => log.debug("Get topics under namespace {}, topic: {}", NamespaceName.ToString(), topicName));
			}
			IList<string> NewTopics = PulsarClientImpl.TopicsPatternFilter(TopicsConflict, topicsPattern);
			IList<string> OldTopics = PatternMultiTopicsConsumerImpl.this.Topics;
			Futures.Add(topicsChangeListener.OnTopicsAdded(TopicsListsMinus(NewTopics, OldTopics)));
			Futures.Add(topicsChangeListener.OnTopicsRemoved(TopicsListsMinus(OldTopics, NewTopics)));
			FutureUtil.waitForAll(Futures).thenAccept(finalFuture => RecheckFuture.complete(null)).exceptionally(ex =>
			{
				log.warn("[{}] Failed to recheck topics change: {}", Topic, ex.Message);
				RecheckFuture.completeExceptionally(ex);
				return null;
			});
			});

			// schedule the next re-check task
			recheckPatternTimeout = ClientConflict.timer().newTimeout(PatternMultiTopicsConsumerImpl.this, Math.Min(1, Conf.PatternAutoDiscoveryPeriod), BAMCIS.Util.Concurrent.TimeUnit.MINUTES);
		}

		public virtual Pattern Pattern
		{
			get
			{
				return this.topicsPattern;
			}
		}

		public interface TopicsChangedListener
		{
			// unsubscribe and delete ConsumerImpl in the `consumers` map in `MultiTopicsConsumerImpl` based on added topics.
			CompletableFuture<Void> OnTopicsRemoved(ICollection<string> RemovedTopics);
			// subscribe and create a list of new ConsumerImpl, added them to the `consumers` map in `MultiTopicsConsumerImpl`.
			CompletableFuture<Void> OnTopicsAdded(ICollection<string> AddedTopics);
		}

		public class PatternTopicsChangedListener : TopicsChangedListener
		{
			private readonly PatternMultiTopicsConsumerImpl<T> outerInstance;

			public PatternTopicsChangedListener(PatternMultiTopicsConsumerImpl<T> outerInstance)
			{
				this.outerInstance = OuterInstance;
			}

			public override CompletableFuture<Void> OnTopicsRemoved(ICollection<string> RemovedTopics)
			{
				CompletableFuture<Void> RemoveFuture = new CompletableFuture<Void>();

				if (RemovedTopics.Count == 0)
				{
					RemoveFuture.complete(null);
					return RemoveFuture;
				}

				IList<CompletableFuture<Void>> Futures = Lists.newArrayListWithExpectedSize(outerInstance.TopicsConflict.Count);
				RemovedTopics.ForEach(outerInstance.Topic => Futures.Add(outerInstance.RemoveConsumerAsync(outerInstance.Topic)));
				FutureUtil.waitForAll(Futures).thenAccept(finalFuture => RemoveFuture.complete(null)).exceptionally(ex =>
				{
				log.warn("[{}] Failed to subscribe topics: {}", outerInstance.Topic, ex.Message);
				RemoveFuture.completeExceptionally(ex);
				return null;
				});
				return RemoveFuture;
			}

			public override CompletableFuture<Void> OnTopicsAdded(ICollection<string> AddedTopics)
			{
				CompletableFuture<Void> AddFuture = new CompletableFuture<Void>();

				if (AddedTopics.Count == 0)
				{
					AddFuture.complete(null);
					return AddFuture;
				}

				IList<CompletableFuture<Void>> Futures = Lists.newArrayListWithExpectedSize(outerInstance.TopicsConflict.Count);
				AddedTopics.ForEach(outerInstance.Topic => Futures.Add(outerInstance.SubscribeAsync(outerInstance.Topic, false)));
				FutureUtil.waitForAll(Futures).thenAccept(finalFuture => AddFuture.complete(null)).exceptionally(ex =>
				{
				log.warn("[{}] Failed to unsubscribe topics: {}", outerInstance.Topic, ex.Message);
				AddFuture.completeExceptionally(ex);
				return null;
				});
				return AddFuture;
			}
		}

		// get topics, which are contained in list1, and not in list2
		public static IList<string> TopicsListsMinus(IList<string> List1, IList<string> List2)
		{
			HashSet<string> S1 = new HashSet<string>(List1);
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
			S1.removeAll(List2);
			return S1.ToList();
		}

		public override CompletableFuture<Void> CloseAsync()
		{
			Timeout Timeout = recheckPatternTimeout;
			if (Timeout != null)
			{
				Timeout.cancel();
				recheckPatternTimeout = null;
			}
			return base.CloseAsync();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting Timeout getRecheckPatternTimeout()
		public virtual Timeout RecheckPatternTimeout
		{
			get
			{
				return recheckPatternTimeout;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(PatternMultiTopicsConsumerImpl));
	}

}