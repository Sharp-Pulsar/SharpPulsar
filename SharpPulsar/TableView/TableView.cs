using System;
using System.Collections.Concurrent;
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

namespace Org.Apache.Pulsar.Client.Impl
{
	using Timeout = io.netty.util.Timeout;
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Org.Apache.Pulsar.Client.Api;
	using MessageId = Org.Apache.Pulsar.Client.Api.MessageId;
	using PulsarClientException = Org.Apache.Pulsar.Client.Api.PulsarClientException;
	using Reader = Org.Apache.Pulsar.Client.Api.Reader;
	using Org.Apache.Pulsar.Client.Api;
	using Org.Apache.Pulsar.Client.Api;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;
	public class TableView<T> : ITableView<T>
	{

		private readonly PulsarClientImpl client;
		private readonly Schema<T> schema;
		private readonly TableViewConfigurationData conf;

		private readonly ConcurrentMap<string, T> data;

		private readonly ConcurrentMap<string, Reader<T>> readers;

		private readonly IList<System.Action<string, T>> listeners;
		private readonly ReentrantLock listenersMutex;

		internal TableView(PulsarClientImpl Client, Schema<T> Schema, TableViewConfigurationData Conf)
		{
			this.client = Client;
			this.schema = Schema;
			this.conf = Conf;
			this.data = new ConcurrentDictionary<string, T>();
			this.readers = new ConcurrentDictionary<string, Reader<T>>();
			this.listeners = new List<BiConsumer<string, T>>();
			this.listenersMutex = new ReentrantLock();
		}

		internal virtual CompletableFuture<TableView<T>> Start()
		{
			return client.GetPartitionsForTopic(conf.getTopicName()).thenCompose(partitions =>
			{
			ISet<string> PartitionsSet = new HashSet<string>(partitions);
			IList<CompletableFuture<object>> Futures = new List<CompletableFuture<object>>();
			partitions.forEach(partition =>
			{
				if (!readers.containsKey(partition))
				{
					Futures.Add(NewReader(partition));
				}
			});
			readers.forEach((existingPartition, existingReader) =>
			{
				if (!PartitionsSet.Contains(existingPartition))
				{
					Futures.Add(existingReader.closeAsync().thenRun(() => readers.remove(existingPartition, existingReader)));
				}
			});
			return FutureUtil.WaitForAll(Futures).thenRun(() => SchedulePartitionsCheck());
			}).thenApply(__ => this);
		}

		private void SchedulePartitionsCheck()
		{
			client.Timer().newTimeout(this.checkForPartitionsChanges, conf.getAutoUpdatePartitionsSeconds(), TimeUnit.SECONDS);
		}

		private void CheckForPartitionsChanges(Timeout Timeout)
		{
			if (Timeout.isCancelled())
			{
				return;
			}

			Start().whenComplete((tw, ex) =>
			{
			if (ex != null)
			{
				log.warn("Failed to check for changes in number of partitions:", ex);
				SchedulePartitionsCheck();
			}
			});
		}

		public virtual int Size()
		{
			return data.size();
		}

		public virtual bool Empty
		{
			get
			{
				return data.isEmpty();
			}
		}

		public virtual bool ContainsKey(string Key)
		{
			return data.containsKey(Key);
		}

		public virtual T Get(string Key)
		{
		   return data.get(Key);
		}

		public virtual ISet<KeyValuePair<string, T>> EntrySet()
		{
		   return data.entrySet();
		}

		public virtual ISet<string> KeySet()
		{
			return data.keySet();
		}

		public virtual ICollection<T> Values()
		{
			return data.values();
		}

		public virtual void ForEach(System.Action<string, T> Action)
		{
			data.forEach(Action);
		}

		public virtual void ForEachAndListen(System.Action<string, T> Action)
		{
			// Ensure we iterate over all the existing entry _and_ start the listening from the exact next message
			try
			{
				listenersMutex.@lock();

				// Execute the action over existing entries
				ForEach(Action);

				listeners.Add(Action);
			}
			finally
			{
				listenersMutex.unlock();
			}
		}

		public virtual CompletableFuture<Void> CloseAsync()
		{
// JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			return FutureUtil.WaitForAll(readers.values().Select(Reader::closeAsync).ToList());
		}

// JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
// ORIGINAL LINE: @Override public void close() throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void Dispose()
		{
			try
			{
				CloseAsync().get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.Unwrap(E);
			}
		}

		private void HandleMessage(Message<T> Msg)
		{
			try
			{
				if (Msg.HasKey())
				{
					if (log.isDebugEnabled())
					{
						log.debug("Applying message from topic {}. key={} value={}", conf.getTopicName(), Msg.Key, Msg.Value);
					}

					try
					{
						listenersMutex.@lock();
						data.put(Msg.Key, Msg.Value);

						foreach (System.Action<string, T> Listener in listeners)
						{
							try
							{
								listener(Msg.Key, Msg.Value);
							}
							catch (Exception T)
							{
								log.error("Table view listener raised an exception", T);
							}
						}
					}
					finally
					{
						listenersMutex.unlock();
					}
				}
			}
			finally
			{
				Msg.Release();
			}
		}

		private CompletableFuture<Reader<T>> NewReader(string Partition)
		{
			return client.NewReader(schema).Topic(Partition).StartMessageId(MessageId.earliest).ReadCompacted(true).PoolMessages(true).CreateAsync().thenCompose(this.readAllExistingMessages);
		}

		private CompletableFuture<Reader<T>> ReadAllExistingMessages(Reader<T> Reader)
		{
			long StartTime = System.nanoTime();
			AtomicLong MessagesRead = new AtomicLong();

			CompletableFuture<Reader<T>> Future = new CompletableFuture<Reader<T>>();
			ReadAllExistingMessages(Reader, Future, StartTime, MessagesRead);
			return Future;
		}

		private void ReadAllExistingMessages(Reader<T> Reader, CompletableFuture<Reader<T>> Future, long StartTime, AtomicLong MessagesRead)
		{
			Reader.HasMessageAvailableAsync().thenAccept(hasMessage =>
			{
			if (hasMessage)
			{
				Reader.ReadNextAsync().thenAccept(msg =>
				{
					MessagesRead.incrementAndGet();
					HandleMessage(msg);
					ReadAllExistingMessages(Reader, Future, StartTime, MessagesRead);
				}).exceptionally(ex =>
				{
					Future.completeExceptionally(ex);
					return null;
				});
			}
			else
			{
				long EndTime = System.nanoTime();
				long DurationMillis = TimeUnit.NANOSECONDS.toMillis(EndTime - StartTime);
				log.info("Started table view for topic {} - Replayed {} messages in {} seconds", Reader.Topic, MessagesRead, DurationMillis / 1000.0);
				Future.complete(Reader);
				ReadTailMessages(Reader);
			}
			});
		}

		private void ReadTailMessages(Reader<T> Reader)
		{
			Reader.ReadNextAsync().thenAccept(msg =>
			{
			HandleMessage(msg);
			ReadTailMessages(Reader);
			}).exceptionally(ex =>
			{
			log.info("Reader {} was interrupted", Reader.Topic);
			return null;
		});
		}
	}

}