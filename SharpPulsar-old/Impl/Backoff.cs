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


	// All variables are in TimeUnit millis by default
//ORIGINAL LINE: @Data public class Backoff
	public class Backoff
	{
		public static readonly long DEFAULT_INTERVAL_IN_NANOSECONDS = TimeUnit.MILLISECONDS.toNanos(100);
		public static readonly long MAX_BACKOFF_INTERVAL_NANOSECONDS = TimeUnit.SECONDS.toNanos(30);
		private readonly long initial;
		private readonly long max;
		private readonly Clock clock;
//Fields cannot have the same name as methods:
		private long next_Conflict;
		private long mandatoryStop;

		private long firstBackoffTimeInMillis;
		private bool mandatoryStopMade = false;

		private static readonly Random random = new Random();
		internal Backoff(long initial, TimeUnit unitInitial, long max, TimeUnit unitMax, long mandatoryStop, TimeUnit unitMandatoryStop, Clock clock)
		{
			this.initial = unitInitial.toMillis(initial);
			this.max = unitMax.toMillis(max);
			this.next_Conflict = this.initial;
			this.mandatoryStop = unitMandatoryStop.toMillis(mandatoryStop);
			this.clock = clock;
		}

		public Backoff(long initial, TimeUnit unitInitial, long max, TimeUnit unitMax, long mandatoryStop, TimeUnit unitMandatoryStop) : this(initial, unitInitial, max, unitMax, mandatoryStop, unitMandatoryStop, Clock.systemDefaultZone())
		{
		}

		public virtual long Next()
		{
			long current = this.next_Conflict;
			if (current < max)
			{
				this.next_Conflict = Math.Min(this.next_Conflict * 2, this.max);
			}

			// Check for mandatory stop
			if (!mandatoryStopMade)
			{
				long now = clock.millis();
				long timeElapsedSinceFirstBackoff = 0;
				if (initial == current)
				{
					firstBackoffTimeInMillis = now;
				}
				else
				{
					timeElapsedSinceFirstBackoff = now - firstBackoffTimeInMillis;
				}

				if (timeElapsedSinceFirstBackoff + current > mandatoryStop)
				{
					current = Math.Max(initial, mandatoryStop - timeElapsedSinceFirstBackoff);
					mandatoryStopMade = true;
				}
			}

			// Randomly decrease the timeout up to 10% to avoid simultaneous retries
			// If current < 10 then current/10 < 1 and we get an exception from Random saying "Bound must be positive"
			if (current > 10)
			{
				current -= random.Next((int) current / 10);
			}
			return Math.Max(initial, current);
		}

		public virtual void ReduceToHalf()
		{
			if (next_Conflict > initial)
			{
				this.next_Conflict = Math.Max(this.next_Conflict / 2, this.initial);
			}
		}

		public virtual void Reset()
		{
			this.next_Conflict = this.initial;
			this.mandatoryStopMade = false;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting long getFirstBackoffTimeInMillis()
		internal virtual long FirstBackoffTimeInMillis
		{
			get
			{
				return firstBackoffTimeInMillis;
			}
		}

		public static bool ShouldBackoff(long initialTimestamp, TimeUnit unitInitial, int failedAttempts, long defaultInterval, long maxBackoffInterval)
		{
			long initialTimestampInNano = unitInitial.toNanos(initialTimestamp);
			long currentTime = System.nanoTime();
			long interval = defaultInterval;
			for (int i = 1; i < failedAttempts; i++)
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

		public static bool ShouldBackoff(long initialTimestamp, TimeUnit unitInitial, int failedAttempts)
		{
			return Backoff.shouldBackoff(initialTimestamp, unitInitial, failedAttempts, DEFAULT_INTERVAL_IN_NANOSECONDS, MAX_BACKOFF_INTERVAL_NANOSECONDS);
		}
	}

}