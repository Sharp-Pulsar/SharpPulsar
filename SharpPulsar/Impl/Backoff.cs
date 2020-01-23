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
	using VisibleForTesting = com.google.common.annotations.VisibleForTesting;
	using Data = lombok.Data;
	using Slf4j = lombok.@extern.slf4j.Slf4j;


	// All variables are in BAMCIS.Util.Concurrent.TimeUnit millis by default
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data public class Backoff
	public class Backoff
	{
		public static readonly long DefaultIntervalInNanoseconds = BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.toNanos(100);
		public static readonly long MaxBackoffIntervalNanoseconds = BAMCIS.Util.Concurrent.TimeUnit.SECONDS.toNanos(30);
		private readonly long initial;
		private readonly long max;
		private readonly Clock clock;
		private long next;
		private long mandatoryStop;

		internal virtual FirstBackoffTimeInMillis {get;}
		private bool mandatoryStopMade = false;

		private static readonly Random random = new Random();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting Backoff(long initial, java.util.concurrent.BAMCIS.Util.Concurrent.TimeUnit unitInitial, long max, java.util.concurrent.BAMCIS.Util.Concurrent.TimeUnit unitMax, long mandatoryStop, java.util.concurrent.BAMCIS.Util.Concurrent.TimeUnit unitMandatoryStop, java.time.Clock clock)
		public Backoff(long Initial, BAMCIS.Util.Concurrent.TimeUnit UnitInitial, long Max, BAMCIS.Util.Concurrent.TimeUnit UnitMax, long MandatoryStop, BAMCIS.Util.Concurrent.TimeUnit UnitMandatoryStop, Clock Clock)
		{
			this.initial = UnitInitial.toMillis(Initial);
			this.max = UnitMax.toMillis(Max);
			this.next = this.initial;
			this.mandatoryStop = UnitMandatoryStop.toMillis(MandatoryStop);
			this.clock = Clock;
		}

		public Backoff(long Initial, BAMCIS.Util.Concurrent.TimeUnit UnitInitial, long Max, BAMCIS.Util.Concurrent.TimeUnit UnitMax, long MandatoryStop, BAMCIS.Util.Concurrent.TimeUnit UnitMandatoryStop) : this(Initial, UnitInitial, Max, UnitMax, MandatoryStop, UnitMandatoryStop, Clock.systemDefaultZone())
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
				long Now = clock.millis();
				long TimeElapsedSinceFirstBackoff = 0;
				if (initial == Current)
				{
					FirstBackoffTimeInMillis = Now;
				}
				else
				{
					TimeElapsedSinceFirstBackoff = Now - FirstBackoffTimeInMillis;
				}

				if (TimeElapsedSinceFirstBackoff + Current > mandatoryStop)
				{
					Current = Math.Max(initial, mandatoryStop - TimeElapsedSinceFirstBackoff);
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting long getFirstBackoffTimeInMillis()

		public static bool ShouldBackoff(long InitialTimestamp, BAMCIS.Util.Concurrent.TimeUnit UnitInitial, int FailedAttempts, long DefaultInterval, long MaxBackoffInterval)
		{
			long InitialTimestampInNano = UnitInitial.toNanos(InitialTimestamp);
			long CurrentTime = System.nanoTime();
			long Interval = DefaultInterval;
			for (int I = 1; I < FailedAttempts; I++)
			{
				Interval = Interval * 2;
				if (Interval > MaxBackoffInterval)
				{
					Interval = MaxBackoffInterval;
					break;
				}
			}

			// if the current time is less than the time at which next retry should occur, we should backoff
			return CurrentTime < (InitialTimestampInNano + Interval);
		}

		public static bool ShouldBackoff(long InitialTimestamp, BAMCIS.Util.Concurrent.TimeUnit UnitInitial, int FailedAttempts)
		{
			return Backoff.ShouldBackoff(InitialTimestamp, UnitInitial, FailedAttempts, DefaultIntervalInNanoseconds, MaxBackoffIntervalNanoseconds);
		}
	}

}