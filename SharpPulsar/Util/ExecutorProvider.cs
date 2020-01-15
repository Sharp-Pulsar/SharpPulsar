﻿using System.Collections.Generic;

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
namespace SharpPulsar.Util
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;


	

	public class ExecutorProvider
	{
		private readonly int numThreads;
		private readonly IList<ExecutorService> executors;
		private readonly AtomicInteger currentThread = new AtomicInteger(0);

		public ExecutorProvider(int numThreads, ThreadFactory threadFactory)
		{
			checkArgument(numThreads > 0);
			this.numThreads = numThreads;
			checkNotNull(threadFactory);
			executors = Lists.newArrayListWithCapacity(numThreads);
			for (int i = 0; i < numThreads; i++)
			{
				executors.Add(Executors.newSingleThreadScheduledExecutor(threadFactory));
			}
		}

		public virtual ExecutorService Executor
		{
			get
			{
				return executors[(currentThread.AndIncrement & int.MaxValue) % numThreads];
			}
		}

		public virtual void shutdownNow()
		{
			executors.ForEach(executor => executor.shutdownNow());
		}
	}

}