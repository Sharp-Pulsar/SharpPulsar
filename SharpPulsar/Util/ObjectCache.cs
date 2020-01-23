using BAMCIS.Util.Concurrent;
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
namespace SharpPulsar.Util
{

	public class ObjectCache<T> 
	{

		private readonly Func<T> _supplier;
		private T _cachedInstance;

		private readonly long _cacheDurationMillis;
		private long _lastRefreshTimestamp;
		private readonly DateTime _clock;

		public ObjectCache(Func<T> Supplier, long CacheDuration, BAMCIS.Util.Concurrent.TimeUnit Unit) : this(Supplier, CacheDuration, Unit, DateTime.UtcNow)
		{
		}

		public ObjectCache(System.Func<T> Supplier, long CacheDuration, BAMCIS.Util.Concurrent.TimeUnit Unit, DateTime Clock)
		{
			_supplier = Supplier;
			_cachedInstance = default(T);
			_cacheDurationMillis = Unit.ToMillis(CacheDuration);
			_clock = Clock;
		}

		public virtual T Get()
		{
			lock (this)
			{
				long Now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
				if (_cachedInstance == null || (Now - _lastRefreshTimestamp) >= _cacheDurationMillis)
				{
					_cachedInstance = _supplier.Invoke();
					_lastRefreshTimestamp = Now;
				}
        
				return _cachedInstance;
			}
		}
	}

}