using System;
using BAMCIS.Util.Concurrent;
using System.Diagnostics;

namespace SharpPulsar
{
	public class Backoff
	{
		public static readonly long DefaultIntervalInNanoseconds =  TimeUnit.MILLISECONDS.ToNanoseconds(100);
		public static readonly long MaxBackoffIntervalNanoseconds = TimeUnit.SECONDS.ToNanoseconds(30);
		private readonly long _initial;
		private readonly long _max;
		private readonly DateTimeOffset _clock;
		private long _next;
		private long _mandatoryStop;

		private long _firstBackoffTimeInMillis;
		private bool _mandatoryStopMade = false;

		private static readonly Random _random = new Random();
		internal Backoff(long initial, TimeUnit unitInitial, long max, TimeUnit unitMax, long mandatoryStop, TimeUnit unitMandatoryStop, DateTimeOffset clock)
		{
			_initial = unitInitial.ToMilliseconds(initial);
			_max = unitMax.ToMilliseconds(max);
			_next = _initial;
			_mandatoryStop = unitMandatoryStop.ToMilliseconds(mandatoryStop);
			_clock = clock;
		}

		public Backoff(long initial, TimeUnit unitInitial, long max, TimeUnit unitMax, long mandatoryStop, TimeUnit unitMandatoryStop) : this(initial, unitInitial, max, unitMax, mandatoryStop, unitMandatoryStop, DateTimeOffset.Now)
		{
		}

		public virtual long Next()
		{
			long current = _next;
			if (current < _max)
			{
				_next = Math.Min(_next * 2, _max);
			}

			// Check for mandatory stop
			if (!_mandatoryStopMade)
			{
				long now = _clock.ToUnixTimeSeconds();
				long timeElapsedSinceFirstBackoff = 0;
				if (_initial == current)
				{
					_firstBackoffTimeInMillis = now;
				}
				else
				{
					timeElapsedSinceFirstBackoff = now - _firstBackoffTimeInMillis;
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
				current -= _random.Next((int)current / 10);
			}
			return Math.Max(_initial, current);
		}

		public virtual void ReduceToHalf()
		{
			if (_next > _initial)
			{
				_next = Math.Max(_next / 2, _initial);
			}
		}

		public virtual void Reset()
		{
			_next = _initial;
			_mandatoryStopMade = false;
		}

		internal virtual long FirstBackoffTimeInMillis
		{
			get
			{
				return _firstBackoffTimeInMillis;
			}
		}

		public static bool ShouldBackoff(long initialTimestamp, TimeUnit unitInitial, int failedAttempts, long defaultInterval, long maxBackoffInterval)
		{
			long initialTimestampInNano = unitInitial.ToNanoseconds(initialTimestamp);
			long currentTime = NanoTime();
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
			return Backoff.ShouldBackoff(initialTimestamp, unitInitial, failedAttempts, DefaultIntervalInNanoseconds, MaxBackoffIntervalNanoseconds);
		}
		private static long NanoTime()
		{
			long nano = 10000L * Stopwatch.GetTimestamp();
			nano /= TimeSpan.TicksPerMillisecond;
			nano *= 100L;
			return nano;
		}
	}


}
