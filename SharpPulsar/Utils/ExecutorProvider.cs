using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Util.Atomic;

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
namespace SharpPulsar.Utils
{

	public class ExecutorProvider
	{
		private readonly int _numThreads;
		private readonly IList<Task> _executors;
		private readonly AtomicInt _currentThread = new AtomicInt(0);
		private readonly CancellationTokenSource _cancellationToken;

		public ExecutorProvider(int numThreads, Action taskFactory)
		{
			_cancellationToken = new CancellationTokenSource();
			if (numThreads < 1)
				throw new ArgumentException("Number of threads most be greater than 0");
			this._numThreads = numThreads;
			if (taskFactory is null)
				throw new NullReferenceException("ActionFactory cannot be null");
			_executors = new List<Task>(numThreads);
			for (var i = 0; i < numThreads; i++)
			{
				_executors.Add(Task.Run(taskFactory, _cancellationToken.Token));
			}
		}

		public virtual Task Executor => _executors[(_currentThread.Increment() & int.MaxValue) % _numThreads];

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