using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
	public class TaskUtil
	{

		/// <summary>
		/// Return a future that represents the completion of the futures in the provided list.
		/// </summary>
		/// <param name="futures">
		/// @return </param>
		public static Task WaitForAll<T>(IList<Task<T>> tasks)
		{
			return Task.WhenAll(tasks.ToArray());
		}

		public static ValueTask<T> FailedTask<T>(Exception t)
		{
			return new ValueTask<T>(Task.FromException(t));
		}

		public static Exception UnwrapCompletionException(Exception t)
		{
			if (t is CompletionException)
			{
				return UnwrapCompletionException(t.InnerException);
			}
			else
			{
				return t;
			}
		}
	}

}