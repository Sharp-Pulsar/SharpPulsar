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
namespace org.apache.pulsar.common.util.collections
{

	using LongPair = org.apache.pulsar.common.util.collections.ConcurrentLongPairSet.LongPair;
	using LongPairConsumer = org.apache.pulsar.common.util.collections.ConcurrentLongPairSet.LongPairConsumer;

	/// <summary>
	/// Hash set where values are composed of pairs of longs.
	/// </summary>
	public interface LongPairSet
	{

		/// <summary>
		/// Adds composite value of item1 and item2 to set.
		/// </summary>
		/// <param name="item1"> </param>
		/// <param name="item2">
		/// @return </param>
		bool add(long item1, long item2);

		/// <summary>
		/// Removes composite value of item1 and item2 from set.
		/// </summary>
		/// <param name="item1"> </param>
		/// <param name="item2">
		/// @return </param>
		bool remove(long item1, long item2);

		/// <summary>
		/// Removes composite value of item1 and item2 from set if provided predicate <seealso cref="LongPairPredicate"/> matches.
		/// </summary>
		/// <param name="filter">
		/// @return </param>
		int removeIf(LongPairSet_LongPairPredicate filter);

		/// <summary>
		/// Execute <seealso cref="LongPairConsumer"/> processor for each entry in the set.
		/// </summary>
		/// <param name="processor"> </param>
		void forEach(LongPairConsumer processor);

		/// <returns> a new list of all keys (makes a copy) </returns>
		ISet<LongPair> items();

		/// <returns> a new list of keys with max provided numberOfItems (makes a copy) </returns>
		ISet<LongPair> items(int numberOfItems);

		/// 
		/// <param name="numberOfItems"> </param>
		/// <param name="longPairConverter">
		///            converts (long,long) pair to <T> object
		/// </param>
		/// <returns> a new list of keys with max provided numberOfItems </returns>
		ISet<T> items<T>(int numberOfItems, LongPairSet_LongPairFunction<T> longPairConverter);

		/// <summary>
		/// Check if set is empty.
		/// 
		/// @return
		/// </summary>
		bool Empty {get;}

		/// <summary>
		/// Removes all items from set.
		/// </summary>
		void clear();

		/// <summary>
		/// Predicate to checks for a key-value pair where both of them have long types.
		/// </summary>

		/// <summary>
		/// Returns size of the set.
		/// 
		/// @return
		/// </summary>
		long size();

		/// <summary>
		/// Checks if given (item1,item2) composite value exists into set.
		/// </summary>
		/// <param name="item1"> </param>
		/// <param name="item2">
		/// @return </param>
		bool contains(long item1, long item2);

		/// <summary>
		/// Represents a function that accepts two long arguments and produces a result. This is the two-arity specialization
		/// of <seealso cref="Function"/>.
		/// </summary>
		/// @param <T>
		///            the type of the result of the function
		///  </param>
	}

	public interface LongPairSet_LongPairPredicate
	{
		bool test(long v1, long v2);
	}

	public interface LongPairSet_LongPairFunction<T>
	{

		/// <summary>
		/// Applies this function to the given arguments.
		/// </summary>
		/// <param name="item1">
		///            the first function argument </param>
		/// <param name="item2">
		///            the second function argument </param>
		/// <returns> the function result </returns>
		T apply(long item1, long item2);
	}

}