using SharpPulsar.Util;
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
namespace SharpPulsar.Impl
{

	public class Backoff
	{
		public static readonly long DefaultIntervalInNanoseconds = BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.ToNanos(100);
		public static readonly long MaxBackoffIntervalNanoseconds = BAMCIS.Util.Concurrent.TimeUnit.SECONDS.ToNanos(30);
		private readonly long _initial;
		private readonly long _max;
		private readonly DateTime _clock;
		private long _next;
		private long _mandatoryStop;

		internal  long FirstBackoffTimeInMillis;
		private bool _mandatoryStopMade = false;

		private static readonly Random Random = new Random();
		public Backoff(long initial, BAMCIS.Util.Concurrent.TimeUnit unitInitial, long max, BAMCIS.Util.Concurrent.TimeUnit unitMax, long mandatoryStop, BAMCIS.Util.Concurrent.TimeUnit unitMandatoryStop, DateTime clock)
		{
			this._initial = unitInitial.ToMillis(initial);
			this._max = unitMax.ToMillis(max);
			this._next = this._initial;
			this._mandatoryStop = unitMandatoryStop.ToMillis(mandatoryStop);
			this._clock = clock;
		}

		public Backoff(long initial, BAMCIS.Util.Concurrent.TimeUnit unitInitial, long max, BAMCIS.Util.Concurrent.TimeUnit unitMax, long mandatoryStop, BAMCIS.Util.Concurrent.TimeUnit unitMandatoryStop) : this(initial, unitInitial, max, unitMax, mandatoryStop, unitMandatoryStop, DateTime.Now)
		{
		}

		public virtual long Next()
		{
			var current = this._next;
			if (current < _max)
			{
				this._next = Math.Min(this._next * 2, this._max);
			}

			// Check for mandatory stop
			if (!_mandatoryStopMade)
			{
				long now = _clock.Millisecond;
				long timeElapsedSinceFirstBackoff = 0;
				if (_initial == current)
				{
					FirstBackoffTimeInMillis = now;
				}
				else
				{
					timeElapsedSinceFirstBackoff = now - FirstBackoffTimeInMillis;
				}

				if (timeElapsedSinceFirstBackoff + current > _mandatoryStop)
				{
					current = Math.Max(_initial, _mandatoryStop - timeElapsedSinceFirstBackoff);
					_mandatoryStopMade = true;
				}
			}

			// Randomly decrease the timeout up to 10% to avoid simultaneous retries
			// If current < 10 then current/10 < 1 and we get an exception from Random saying "Bound must be positive"
			if (current > 10)
			{
				current -= Random.Next((int) current / 10);
			}
			return Math.Max(_initial, current);
		}

		public virtual void ReduceToHalf()
		{
			if (_next > _initial)
			{
				this._next = Math.Max(this._next / 2, this._initial);
			}
		}

		public virtual void Reset()
		{
			this._next = this._initial;
			this._mandatoryStopMade = false;
		}


		public static bool ShouldBackoff(long initialTimestamp, BAMCIS.Util.Concurrent.TimeUnit unitInitial, int failedAttempts, long defaultInterval, long maxBackoffInterval)
		{
			var initialTimestampInNano = unitInitial.ToNanos(initialTimestamp);
			long currentTime = DateTime.Now.Millisecond;
			var interval = defaultInterval;
			for (var i = 1; i < failedAttempts; i++)
			{
				interval = interval * 2;
				if (interval > maxBackoffInterval)
				{
					interval = maxBackoffInterval;
					break;
				}
			}

			// if the current time is less than the time at which next retry should occur, we should backoff
			return currentTime < (initialTimestampInNano + interval);
		}

		public static bool ShouldBackoff(long initialTimestamp, BAMCIS.Util.Concurrent.TimeUnit unitInitial, int failedAttempts)
		{
			return ShouldBackoff(initialTimestamp, unitInitial, failedAttempts, DefaultIntervalInNanoseconds, MaxBackoffIntervalNanoseconds);
		}
	}

}