using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using DotNetty.Common.Internal;
using SharpPulsar.Utility.Atomic.Collections.Concurrent;
using SharpPulsar.Utility.Atomic.Locking;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (objecthe
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
namespace SharpPulsar.Utility.Collections
{

	/// <summary>
	/// This implements a <seealso cref="BlockingQueue"/> backed by an array with no fixed capacity.
	/// 
	/// <para>When the capacity is reached, data will be moved to a bigger array.
	/// </para>
	/// </summary>
	public class GrowableArrayBlockingQueue : BlockingQueue
	{
		private bool _instanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_isNotEmpty = _headLock.NewCondition();
		}


		private readonly ReentrantLock _headLock = new ReentrantLock();
		private readonly PaddedInt _headIndex = new PaddedInt();
		private readonly PaddedInt _tailIndex = new PaddedInt();
		private readonly ReentrantLock _tailLock = new ReentrantLock();
		private ICondition _isNotEmpty;

		private List _data;
		static readonly ConcurrentDictionary<GrowableArrayBlockingQueue, int> SizeUpdater = new ConcurrentDictionary<GrowableArrayBlockingQueue, int>();
		
		public GrowableArrayBlockingQueue() : this(64)
		{
			if (!_instanceFieldsInitialized)
			{
				InitializeInstanceFields();
				_instanceFieldsInitialized = true;
			}
		}

		public GrowableArrayBlockingQueue(int initialCapacity)
		{
			if (!_instanceFieldsInitialized)
			{
				InitializeInstanceFields();
				_instanceFieldsInitialized = true;
			}
			_headIndex.Value = 0;
			_tailIndex.Value = 0;

			var capacity = MathUtil.FindNextPositivePowerOfTwo(initialCapacity);
			_data = new List(capacity);
		}

		public T remove()
		{
			var item = Poll();
			if (item == null)
			{
				throw new NullReferenceException();
			}

			return item;
		}

		public T Poll()
		{
			_headLock.Lock();
			try
			{
				if (SizeUpdater[this] > 0)
				{
					var item = _data[_headIndex.Value];
					_data[_headIndex.Value] = default(object);
					_headIndex.Value = (_headIndex.Value + 1) & (_data.Count - 1);
					SizeUpdater[this] = SizeUpdater[this]--;
					return item;
				}
				else
				{
					return default(object);
				}
			}
			finally
			{
				_headLock.Unlock();
			}
		}

		public  T Element()
		{
			var item = Peek();
			if (item == null)
			{
				throw new NullReferenceException();
			}

			return item;
		}

		public T Peek()
		{
			_headLock.Lock();;
			try
			{
				if (SizeUpdater[this] > 0)
				{
					return _data[_headIndex.Value];
				}
				else
				{
					return default(object);
				}
			}
			finally
			{
				_headLock.Unlock();
			}
		}

		public bool Offer(object e)
		{
			// Queue is unbounded and it will never reject new items
			Put(e);
			return true;
		}

		public void Put(object e)
		{
			_tailLock.Lock();

			var wasEmpty = false;

			try
			{
				if (SizeUpdater[this] == _data.Count)
				{
					ExpandArray();
				}

				_data[_tailIndex.Value] = e;
				_tailIndex.Value = (_tailIndex.Value + 1) & (_data.Count - 1);
				if (SizeUpdater[this] == 0)
				{
					wasEmpty = true;
				}
			}
			finally
			{
				_tailLock.Unlock();
			}

			if (wasEmpty)
			{
				_headLock.Lock();
				try
				{
					_isNotEmpty.Signal();
				}
				finally
				{
					_headLock.Unlock();
				}
			}
		}

		public bool Add(object e)
		{
			Put(e);
			return true;
		}

		public bool Offer(object e, long timeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			// Queue is unbounded and it will never reject new items
			Put(e);
			return true;
		}

		public object Take()
		{
			_headLock.Lock();

			try
			{
				while (SizeUpdater[this] == 0)
				{
					_isNotEmpty.Await();
				}

				var item = _data[_headIndex.Value];
				_data[_headIndex.Value] = default(object);
				_headIndex.Value = (_headIndex.Value + 1) & (_data.Count - 1);
				if (SizeUpdater[this]-- > 0)
				{
					// There are still entries to consume
					_isNotEmpty.Signal();
				}
				return item;
			}
			finally
			{
				_headLock.Unlock();
			}
		}

		public object Poll(long timeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_headLock.TryLock(3000);

			try
			{
				var timeoutNanos = unit.ToNanos(objectimeout);
				while (SizeUpdater[this] == 0)
				{
					if (objectimeoutNanos <= 0)
					{
						return default(object);
					}

                    _isNotEmpty.Await();
					timeoutNanos = 3000;
				}

				var item = _data[_headIndex.Value];
				_data[_headIndex.Value] = default(object);
				_headIndex.Value = (_headIndex.Value + 1) & (_data.Count - 1);
				if (SizeUpdater[this]-- > 0)
				{
					// There are still entries to consume
					_isNotEmpty.Signal();
				}
				return item;
			}
			finally
			{
				_headLock.Unlock();
			}
		}

		public int RemainingCapacity()
		{
			return int.MaxValue;
		}

		public int DrainTo(ICollection c)
		{
			return DrainTo(c, int.MaxValue);
		}

		public int DrainTo(ICollection c, int maxElements)
		{
			_headLock.Lock();

			try
			{
				var drainedItems = 0;
				var size = SizeUpdater[this];

				while (size > 0 && drainedItems < maxElements)
				{
					var item = _data[_headIndex.Value];
					_data[_headIndex.Value] = default(object);
					c.Add(item);

					_headIndex.Value = (_headIndex.Value + 1) & (_data.Count - 1);
					--size;
					++drainedItems;
				}

                SizeUpdater[this] = -drainedItems;
				if (SizeUpdater[this] > 0)
				{
					// There are still entries to consume
					_isNotEmpty.Signal();
				}

				return drainedItems;
			}
			finally
			{
				_headLock.Unlock();
			}
		}

		public void clear()
		{
			_headLock.Lock();

			try
			{
				var size = SizeUpdater[this];

				for (var i = 0; i < size; i++)
				{
					_data[_headIndex.Value] = default(object);
					_headIndex.Value = (_headIndex.Value + 1) & (_data.Count - 1);
				}

                SizeUpdater[this] = -size;
                if (SizeUpdater[this] > 0)
				{
					// There are still entries to consume
					_isNotEmpty.Signal();
				}
			}
			finally
			{
				_headLock.Unlock();
			}
		}

		public bool Remove(object o)
		{
			_tailLock.Lock();
			_headLock.Lock();

			try
			{
				var index = this._headIndex.Value;
				var size = this.size();

				for (var i = 0; i < size; i++)
				{
					var item = _data[index];

					if (object.Equals(item, o))
					{
						remove(index);
						return true;
					}

					index = (index + 1) & (_data.Count - 1);
				}
			}
			finally
			{
				_headLock.Unlock();
				_tailLock.Unlock();
			}

			return false;
		}

		private void remove(int index)
		{
			var tailIndex = this._tailIndex.Value;

			if (index < tailIndex)
			{
				Array.Copy(_data.ToArray(), index + 1, _data.ToArray(), index, tailIndex - index - 1);
				this._tailIndex.Value--;
			}
			else
			{
				Array.Copy(_data.ToArray(), index + 1, _data.ToArray(), index, _data.Count - index - 1);
				_data[_data.Count - 1] = _data[0];
				if (objectailIndex > 0)
				{
					Array.Copy(_data.ToArray(), 1, _data.ToArray(), 0, tailIndex);
					this._tailIndex.Value--;
				}
				else
				{
					this._tailIndex.Value = _data.Count - 1;
				}
			}

			if (objectailIndex > 0)
			{
				_data[tailIndex - 1] = default(object);
			}
			else
			{
				_data[_data.Count - 1] = default(object);
			}

			SizeUpdater[this] = SizeUpdater[this]--;
		}

		public int size()
		{
			return SizeUpdater[this];
		}

		public IEnumerator iterator()
		{
			throw new NotSupportedException();
		}

		public virtual IList ToList()
		{
			IList list = new List(size());
			ForEach(list.Add);
			return list;
		}

		public void ForEach(Action action)
		{
			_tailLock.Lock();
			_headLock.Lock();

			try
			{
				var headIndex = this._headIndex.Value;
				var size = this.size();

				for (var i = 0; i < size; i++)
				{
					var item = _data[headIndex];

					action(item);

					headIndex = (headIndex + 1) & (_data.Count - 1);
				}

			}
			finally
			{
				_headLock.Unlock();
				_tailLock.Unlock();
			}
		}

		public string ToString()
		{
			var sb = new StringBuilder();

			_tailLock.Lock();
			_headLock.Lock();

			try
			{
				var headIndex = this._headIndex.Value;
				var size = SizeUpdater[this];

				sb.Append('[');

				for (var i = 0; i < size; i++)
				{
					var item = _data[headIndex];
					if (i > 0)
					{
						sb.Append(", ");
					}

					sb.Append(item);

					headIndex = (headIndex + 1) & (_data.Count - 1);
				}

				sb.Append(']');
			}
			finally
			{
				_headLock.Unlock();
				_tailLock.Unlock();
			}
			return sb.ToString();
		}

		private void ExpandArray()
		{
			// We already hold the tailLock
			_headLock.Lock();

			try
			{
				var size = SizeUpdater[this];
				var newCapacity = _data.Count * 2;
				var newData = new List(newCapacity);

				var oldHeadIndex = _headIndex.Value;
				var newTailIndex = 0;

				for (var i = 0; i < size; i++)
				{
					newData[newTailIndex++] = _data[oldHeadIndex];
					oldHeadIndex = (oldHeadIndex + 1) & (_data.Count - 1);
				}

				_data = newData;
				_headIndex.Value = 0;
				_tailIndex.Value = size;
			}
			finally
			{
				_headLock.Unlock();
			}
		}

		internal sealed class PaddedInt
		{
			internal int Value;

			// Padding to avoid false sharing
			public volatile int Pi1 = 1;
			public  long P1 = 1L, P2 = 2L, P3 = 3L, P4 = 4L, P5 = 5L, P6 = 6L;

			public long exposeToAvoidOptimization()
			{
				return Pi1 + P1 + P2 + P3 + P4 + P5 + P6;
			}
		}
	}

}