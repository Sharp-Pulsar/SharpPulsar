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
		private readonly long initial;
		private readonly long max;
		private readonly DateTime clock;
		private long next;
		private long mandatoryStop;

		internal  long firstBackoffTimeInMillis;
		private bool mandatoryStopMade = false;

		private static readonly Random random = new Random();
		public Backoff(long initial, BAMCIS.Util.Concurrent.TimeUnit UnitInitial, long max, BAMCIS.Util.Concurrent.TimeUnit unitMax, long mandatoryStop, BAMCIS.Util.Concurrent.TimeUnit unitMandatoryStop, DateTime clock)
		{
			this.initial = UnitInitial.ToMillis(initial);
			this.max = unitMax.ToMillis(max);
			this.next = this.initial;
			this.mandatoryStop = unitMandatoryStop.ToMillis(mandatoryStop);
			this.clock = clock;
		}

		public Backoff(long Initial, BAMCIS.Util.Concurrent.TimeUnit UnitInitial, long Max, BAMCIS.Util.Concurrent.TimeUnit UnitMax, long MandatoryStop, BAMCIS.Util.Concurrent.TimeUnit UnitMandatoryStop) : this(Initial, UnitInitial, Max, UnitMax, MandatoryStop, UnitMandatoryStop, DateTime.Now)
		{
		}

		public virtual long Next()
		{
			long Current = this.next;
			if (Current < max)
			{
				this.next = Math.Min(this.next * 2, this.max);
			}

			// Check for mandatory stop
			if (!mandatoryStopMade)
			{
				long now = clock.Millisecond;
				long timeElapsedSinceFirstBackoff = 0;
				if (initial == Current)
				{
					firstBackoffTimeInMillis = now;
				}
				else
				{
					timeElapsedSinceFirstBackoff = now - firstBackoffTimeInMillis;
				}

				if (timeElapsedSinceFirstBackoff + Current > mandatoryStop)
				{
					Current = Math.Max(initial, mandatoryStop - timeElapsedSinceFirstBackoff);
					mandatoryStopMade = true;
				}
			}

			// Randomly decrease the timeout up to 10% to avoid simultaneous retries
			// If current < 10 then current/10 < 1 and we get an exception from Random saying "Bound must be positive"
			if (Current > 10)
			{
				Current -= random.Next((int) Current / 10);
			}
			return Math.Max(initial, Current);
		}

		public virtual void ReduceToHalf()
		{
			if (next > initial)
			{
				this.next = Math.Max(this.next / 2, this.initial);
			}
		}

		public virtual void Reset()
		{
			this.next = this.initial;
			this.mandatoryStopMade = false;
		}


		public static bool ShouldBackoff(long initialTimestamp, BAMCIS.Util.Concurrent.TimeUnit unitInitial, int failedAttempts, long defaultInterval, long maxBackoffInterval)
		{
			long initialTimestampInNano = unitInitial.ToNanos(initialTimestamp);
			long currentTime = DateTime.Now.Millisecond;
			long Interval = defaultInterval;
			for (int I = 1; I < failedAttempts; I++)
			{
				Interval = Interval * 2;
				if (Interval > maxBackoffInterval)
				{
					Interval = maxBackoffInterval;
					break;
				}
			}

			// if the current time is less than the time at which next retry should occur, we should backoff
			return currentTime < (initialTimestampInNano + Interval);
		}

		public static bool ShouldBackoff(long InitialTimestamp, BAMCIS.Util.Concurrent.TimeUnit UnitInitial, int FailedAttempts)
		{
			return ShouldBackoff(InitialTimestamp, UnitInitial, FailedAttempts, DefaultIntervalInNanoseconds, MaxBackoffIntervalNanoseconds);
		}
	}

}