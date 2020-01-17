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
namespace org.apache.pulsar.common.util
{

	/// <summary>
	/// This class is aimed at simplifying work with {@code CompletableFuture}.
	/// </summary>
	public class FutureUtil
	{

		/// <summary>
		/// Return a future that represents the completion of the futures in the provided list.
		/// </summary>
		/// <param name="futures">
		/// @return </param>
		public static CompletableFuture<Void> waitForAll<T>(IList<CompletableFuture<T>> futures)
		{
			return CompletableFuture.allOf(((List<CompletableFuture<T>>)futures).ToArray());
		}

		public static CompletableFuture<T> failedFuture<T>(Exception t)
		{
			CompletableFuture<T> future = new CompletableFuture<T>();
			future.completeExceptionally(t);
			return future;
		}

		public static Exception unwrapCompletionException(Exception t)
		{
			if (t is CompletionException)
			{
				return unwrapCompletionException(t.InnerException);
			}
			else
			{
				return t;
			}
		}
	}

}