using System.Collections.Generic;
using System;
using SharpPulsar.Util.Atomic;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

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

	public class ExecutorProvider
	{
		private readonly int numThreads;
		private readonly IList<Task> _executors;
		private readonly AtomicInt _currentThread = new AtomicInt(0);
		private readonly CancellationTokenSource _cancellationToken;

		public ExecutorProvider(int NumThreads, Action taskFactory)
		{
			_cancellationToken = new CancellationTokenSource();
			if (NumThreads < 1)
				throw new ArgumentException("Number of threads most be greater than 0");
			this.numThreads = NumThreads;
			if (taskFactory is null)
				throw new NullReferenceException("ActionFactory cannot be null");
			_executors = new List<Task>(NumThreads);
			for (int I = 0; I < NumThreads; I++)
			{
				_executors.Add(Task.Run(taskFactory, _cancellationToken.Token));
			}
		}

		public virtual Task Executor
		{
			get
			{
				return _executors[(_currentThread.Increment() & int.MaxValue) % numThreads];
			}
		}

		public virtual void ShutdownNow()
		{
			_cancellationToken.Cancel();
			_executors.ToList().ForEach(executor => { 
				if(executor.IsCanceled)
				{
					//log for example
				}
			});
		}
	}

}