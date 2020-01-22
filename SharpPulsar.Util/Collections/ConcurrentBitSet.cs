using System.Collections;

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
namespace oSharpPulsar.Util.Collections
{

	/// <summary>
	/// Safe multithreaded version of {@code BitSet}.
	/// </summary>
	public class ConcurrentBitSet : BitArray
	{

		private const long serialVersionUID = 1L;
		private readonly StampedLock rwLock = new StampedLock();

		/// <summary>
		/// Creates a bit set whose initial size is large enough to explicitly represent bits with indices in the range
		/// {@code 0} through {@code nbits-1}. All bits are initially {@code false}.
		/// </summary>
		/// <param name="nbits">
		///            the initial size of the bit set </param>
		/// <exception cref="NegativeArraySizeException">
		///             if the specified initial size is negative </exception>
		public ConcurrentBitSet(int nbits) : base(nbits)
		{
		}

		public override bool get(int bitIndex)
		{
			long stamp = rwLock.tryOptimisticRead();
			bool isSet = base.Get(bitIndex);
			if (!rwLock.validate(stamp))
			{
				stamp = rwLock.readLock();
				try
				{
					isSet = base.Get(bitIndex);
				}
				finally
				{
					rwLock.unlockRead(stamp);
				}
			}
			return isSet;
		}

		public override void set(int bitIndex)
		{
			long stamp = rwLock.tryOptimisticRead();
			base.Set(bitIndex, true);
			if (!rwLock.validate(stamp))
			{
				stamp = rwLock.readLock();
				try
				{
					base.Set(bitIndex, true);
				}
				finally
				{
					rwLock.unlockRead(stamp);
				}
			}
		}

		public override void set(int fromIndex, int toIndex)
		{
			long stamp = rwLock.tryOptimisticRead();
			base.Set(fromIndex, toIndex);
			if (!rwLock.validate(stamp))
			{
				stamp = rwLock.readLock();
				try
				{
					base.Set(fromIndex, toIndex);
				}
				finally
				{
					rwLock.unlockRead(stamp);
				}
			}
		}

		public override int nextSetBit(int fromIndex)
		{
			long stamp = rwLock.tryOptimisticRead();
			int bit = base.nextSetBit(fromIndex);
			if (!rwLock.validate(stamp))
			{
				stamp = rwLock.readLock();
				try
				{
					bit = base.nextSetBit(fromIndex);
				}
				finally
				{
					rwLock.unlockRead(stamp);
				}
			}
			return bit;
		}

		public override int nextClearBit(int fromIndex)
		{
			long stamp = rwLock.tryOptimisticRead();
			int bit = base.nextClearBit(fromIndex);
			if (!rwLock.validate(stamp))
			{
				stamp = rwLock.readLock();
				try
				{
					bit = base.nextClearBit(fromIndex);
				}
				finally
				{
					rwLock.unlockRead(stamp);
				}
			}
			return bit;
		}

		public override int previousSetBit(int fromIndex)
		{
			long stamp = rwLock.tryOptimisticRead();
			int bit = base.previousSetBit(fromIndex);
			if (!rwLock.validate(stamp))
			{
				stamp = rwLock.readLock();
				try
				{
					bit = base.previousSetBit(fromIndex);
				}
				finally
				{
					rwLock.unlockRead(stamp);
				}
			}
			return bit;
		}

		public override int previousClearBit(int fromIndex)
		{
			long stamp = rwLock.tryOptimisticRead();
			int bit = base.previousClearBit(fromIndex);
			if (!rwLock.validate(stamp))
			{
				stamp = rwLock.readLock();
				try
				{
					bit = base.previousClearBit(fromIndex);
				}
				finally
				{
					rwLock.unlockRead(stamp);
				}
			}
			return bit;
		}

		public override bool Empty
		{
			get
			{
				long stamp = rwLock.tryOptimisticRead();
				bool isEmpty = base.Empty;
				if (!rwLock.validate(stamp))
				{
					// Fallback to read lock
					stamp = rwLock.readLock();
					try
					{
						isEmpty = base.Empty;
					}
					finally
					{
						rwLock.unlockRead(stamp);
					}
				}
				return isEmpty;
			}
		}
	}
}