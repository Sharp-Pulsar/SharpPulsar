using DotNetty.Common.Internal;
using java.util.concurrent.atomic;
using SharpPulsar.Util.Atomic.Collections.Concurrent;
using SharpPulsar.Util.Atomic.Locking;
using System;
using System.Collections.Generic;
using System.Text;

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
namespace SharpPulsar.Util.Collections
{

	/// <summary>
	/// This implements a <seealso cref="BlockingQueue"/> backed by an array with no fixed capacity.
	/// 
	/// <para>When the capacity is reached, data will be moved to a bigger array.
	/// </para>
	/// </summary>
	public class GrowableArrayBlockingQueue<T> : AbstractQueue<T>, BlockingQueue<T>
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			isNotEmpty = headLock.NewCondition();
		}


		private readonly ReentrantLock headLock = new ReentrantLock();
		private readonly PaddedInt headIndex = new PaddedInt();
		private readonly PaddedInt tailIndex = new PaddedInt();
		private readonly ReentrantLock tailLock = new ReentrantLock();
		private ICondition isNotEmpty;

		private T[] data;
		private static readonly AtomicIntegerFieldUpdater<GrowableArrayBlockingQueue> SIZE_UPDATER = AtomicIntegerFieldUpdater.newUpdater(typeof(GrowableArrayBlockingQueue), "size");
		private volatile int size = 0;

		public GrowableArrayBlockingQueue() : this(64)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		public GrowableArrayBlockingQueue(int initialCapacity)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			headIndex.value = 0;
			tailIndex.value = 0;

			int capacity = MathUtil.FindNextPositivePowerOfTwo(initialCapacity);
			data = (T[]) new object[capacity];
		}

		public override T remove()
		{
			T item = poll();
			if (item == null)
			{
				throw new NoSuchElementException();
			}

			return item;
		}

		public override T Poll()
		{
			headLock.Lock();
			try
			{
				if (SIZE_UPDATER.get(this) > 0)
				{
					T item = data[headIndex.value];
					data[headIndex.value] = default(T);
					headIndex.value = (headIndex.value + 1) & (data.Length - 1);
					SIZE_UPDATER.decrementAndGet(this);
					return item;
				}
				else
				{
					return default(T);
				}
			}
			finally
			{
				headLock.Unlock();
			}
		}

		public override T Element()
		{
			T item = peek();
			if (item == null)
			{
				throw new NoSuchElementException();
			}

			return item;
		}

		public override T Peek()
		{
			headLock.@lock();
			try
			{
				if (SIZE_UPDATER.get(this) > 0)
				{
					return data[headIndex.value];
				}
				else
				{
					return default(T);
				}
			}
			finally
			{
				headLock.unlock();
			}
		}

		public override bool offer(T e)
		{
			// Queue is unbounded and it will never reject new items
			put(e);
			return true;
		}

		public override void put(T e)
		{
			tailLock.@lock();

			bool wasEmpty = false;

			try
			{
				if (SIZE_UPDATER.get(this) == data.Length)
				{
					expandArray();
				}

				data[tailIndex.value] = e;
				tailIndex.value = (tailIndex.value + 1) & (data.Length - 1);
				if (SIZE_UPDATER.getAndIncrement(this) == 0)
				{
					wasEmpty = true;
				}
			}
			finally
			{
				tailLock.unlock();
			}

			if (wasEmpty)
			{
				headLock.@lock();
				try
				{
					isNotEmpty.signal();
				}
				finally
				{
					headLock.unlock();
				}
			}
		}

		public override bool add(T e)
		{
			put(e);
			return true;
		}

		public override bool offer(T e, long timeout, TimeUnit unit)
		{
			// Queue is unbounded and it will never reject new items
			put(e);
			return true;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public T take() throws InterruptedException
		public override T take()
		{
			headLock.lockInterruptibly();

			try
			{
				while (SIZE_UPDATER.get(this) == 0)
				{
					isNotEmpty.await();
				}

				T item = data[headIndex.value];
				data[headIndex.value] = default(T);
				headIndex.value = (headIndex.value + 1) & (data.Length - 1);
				if (SIZE_UPDATER.decrementAndGet(this) > 0)
				{
					// There are still entries to consume
					isNotEmpty.signal();
				}
				return item;
			}
			finally
			{
				headLock.unlock();
			}
		}

		public T Poll(long timeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			headLock.lockInterruptibly();

			try
			{
				long timeoutNanos = unit.ToNanos(timeout);
				while (SIZE_UPDATER.get(this) == 0)
				{
					if (timeoutNanos <= 0)
					{
						return default(T);
					}

					timeoutNanos = isNotEmpty.awaitNanos(timeoutNanos);
				}

				T item = data[headIndex.value];
				data[headIndex.value] = default(T);
				headIndex.value = (headIndex.value + 1) & (data.Length - 1);
				if (SIZE_UPDATER.decrementAndGet(this) > 0)
				{
					// There are still entries to consume
					isNotEmpty.Signal();
				}
				return item;
			}
			finally
			{
				headLock.Unlock();
			}
		}

		public int RemainingCapacity()
		{
			return int.MaxValue;
		}

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: @Override public int drainTo(java.util.Collection<? super T> c)
		public override int drainTo<T1>(ICollection<T1> c)
		{
			return drainTo(c, int.MaxValue);
		}

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: @Override public int drainTo(java.util.Collection<? super T> c, int maxElements)
		public override int drainTo<T1>(ICollection<T1> c, int maxElements)
		{
			headLock.@lock();

			try
			{
				int drainedItems = 0;
				int size = SIZE_UPDATER.get(this);

				while (size > 0 && drainedItems < maxElements)
				{
					T item = data[headIndex.value];
					data[headIndex.value] = default(T);
					c.Add(item);

					headIndex.value = (headIndex.value + 1) & (data.Length - 1);
					--size;
					++drainedItems;
				}

				if (SIZE_UPDATER.addAndGet(this, -drainedItems) > 0)
				{
					// There are still entries to consume
					isNotEmpty.signal();
				}

				return drainedItems;
			}
			finally
			{
				headLock.unlock();
			}
		}

		public override void clear()
		{
			headLock.@lock();

			try
			{
				int size = SIZE_UPDATER.get(this);

				for (int i = 0; i < size; i++)
				{
					data[headIndex.value] = default(T);
					headIndex.value = (headIndex.value + 1) & (data.Length - 1);
				}

				if (SIZE_UPDATER.addAndGet(this, -size) > 0)
				{
					// There are still entries to consume
					isNotEmpty.signal();
				}
			}
			finally
			{
				headLock.unlock();
			}
		}

		public override bool remove(object o)
		{
			tailLock.@lock();
			headLock.@lock();

			try
			{
				int index = this.headIndex.value;
				int size = this.size_Conflict;

				for (int i = 0; i < size; i++)
				{
					T item = data[index];

					if (Objects.equals(item, o))
					{
						remove(index);
						return true;
					}

					index = (index + 1) & (data.Length - 1);
				}
			}
			finally
			{
				headLock.unlock();
				tailLock.unlock();
			}

			return false;
		}

		private void remove(int index)
		{
			int tailIndex = this.tailIndex.value;

			if (index < tailIndex)
			{
				Array.Copy(data, index + 1, data, index, tailIndex - index - 1);
				this.tailIndex.value--;
			}
			else
			{
				Array.Copy(data, index + 1, data, index, data.Length - index - 1);
				data[data.Length - 1] = data[0];
				if (tailIndex > 0)
				{
					Array.Copy(data, 1, data, 0, tailIndex);
					this.tailIndex.value--;
				}
				else
				{
					this.tailIndex.value = data.Length - 1;
				}
			}

			if (tailIndex > 0)
			{
				data[tailIndex - 1] = default(T);
			}
			else
			{
				data[data.Length - 1] = default(T);
			}

			SIZE_UPDATER.decrementAndGet(this);
		}

		public override int size()
		{
			return SIZE_UPDATER.get(this);
		}

		public override IEnumerator<T> iterator()
		{
			throw new System.NotSupportedException();
		}

		public virtual IList<T> toList()
		{
			IList<T> list = new List<T>(size());
			forEach(list.add);
			return list;
		}

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: @Override public void forEach(java.util.function.Consumer<? super T> action)
		public override void forEach<T1>(System.Action<T1> action)
		{
			tailLock.@lock();
			headLock.@lock();

			try
			{
				int headIndex = this.headIndex.value;
				int size = this.size_Conflict;

				for (int i = 0; i < size; i++)
				{
					T item = data[headIndex];

					action(item);

					headIndex = (headIndex + 1) & (data.Length - 1);
				}

			}
			finally
			{
				headLock.unlock();
				tailLock.unlock();
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			tailLock.@lock();
			headLock.@lock();

			try
			{
				int headIndex = this.headIndex.value;
				int size = SIZE_UPDATER.get(this);

				sb.Append('[');

				for (int i = 0; i < size; i++)
				{
					T item = data[headIndex];
					if (i > 0)
					{
						sb.Append(", ");
					}

					sb.Append(item);

					headIndex = (headIndex + 1) & (data.Length - 1);
				}

				sb.Append(']');
			}
			finally
			{
				headLock.unlock();
				tailLock.unlock();
			}
			return sb.ToString();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private void expandArray()
		private void expandArray()
		{
			// We already hold the tailLock
			headLock.@lock();

			try
			{
				int size = SIZE_UPDATER.get(this);
				int newCapacity = data.Length * 2;
				T[] newData = (T[]) new object[newCapacity];

				int oldHeadIndex = headIndex.value;
				int newTailIndex = 0;

				for (int i = 0; i < size; i++)
				{
					newData[newTailIndex++] = data[oldHeadIndex];
					oldHeadIndex = (oldHeadIndex + 1) & (data.Length - 1);
				}

				data = newData;
				headIndex.value = 0;
				tailIndex.value = size;
			}
			finally
			{
				headLock.unlock();
			}
		}

		internal sealed class PaddedInt
		{
			internal int value;

			// Padding to avoid false sharing
			public volatile int pi1 = 1;
			public volatile long p1 = 1L, p2 = 2L, p3 = 3L, p4 = 4L, p5 = 5L, p6 = 6L;

			public long exposeToAvoidOptimization()
			{
				return pi1 + p1 + p2 + p3 + p4 + p5 + p6;
			}
		}
	}

}