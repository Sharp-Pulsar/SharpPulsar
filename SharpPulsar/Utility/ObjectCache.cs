using System;

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
namespace SharpPulsar.Utility
{

	public class ObjectCache
	{

		private readonly Func supplier;
		private T cachedInstance;

		private readonly long cacheDurationMillis;
		private long lastRefreshTimestamp;
		private readonly DateTimeOffset clock;

		public ObjectCache(Func supplier, long cacheDuration, BAMCIS.Util.Concurrent.TimeUnit unit) : this(supplier, cacheDuration, unit, DateTime.UtcNow)
		{
		}

		internal ObjectCache(Func supplier, long cacheDuration, BAMCIS.Util.Concurrent.TimeUnit unit, DateTime clock)
		{
			this.supplier = supplier;
			this.cachedInstance = default(T);
			this.cacheDurationMillis = unit.ToMillis(cacheDuration);
			this.clock = clock;
		}

		public virtual T Get()
		{
			lock (this)
			{
				long now = clock.ToUnixTimeSeconds();
				if (cachedInstance == null || (now - lastRefreshTimestamp) >= cacheDurationMillis)
				{
					cachedInstance = supplier.Invoke();
					lastRefreshTimestamp = now;
				}
        
				return cachedInstance;
			}
		}
	}

}