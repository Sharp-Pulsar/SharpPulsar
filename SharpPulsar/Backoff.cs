using System;
using System.Diagnostics;

namespace SharpPulsar
{
	public class Backoff
	{
		public static readonly long DefaultIntervalInMs =  100;
		public static readonly long MaxBackoffIntervalMs = 30;
		private readonly long _initial;
		private readonly long _max;
		private readonly DateTimeOffset _clock;
		private long _next;
		private readonly long _mandatoryStop;

		private long _firstBackoffTimeInMillis;
		private bool _mandatoryStopMade = false;

		private static readonly Random _random = new Random();
		internal Backoff(TimeSpan initial, TimeSpan max, TimeSpan mandatoryStop, DateTimeOffset clock)
		{
			_initial = (long)initial.TotalMilliseconds;
			_max = (long)max.TotalMilliseconds;
			_next = _initial;
			_mandatoryStop = (long)mandatoryStop.TotalMilliseconds;
			_clock = clock;
		}

		public Backoff(TimeSpan initial, TimeSpan max, TimeSpan mandatoryStop) : this(initial, max, mandatoryStop, DateTimeOffset.Now)
		{
		}

		public virtual long Next()
		{
			var current = _next;
			if (current < _max)
			{
				_next = Math.Min(_next * 2, _max);
			}

			// Check for mandatory stop
			if (!_mandatoryStopMade)
			{
				var now = _clock.ToUnixTimeSeconds();
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

		public static bool ShouldBackoff(long initialTimestamp, int failedAttempts, long defaultInterval, long maxBackoffInterval)
		{
			var initialTimestampInNano = initialTimestamp;
			var currentTime = NanoTime();
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

		public static bool ShouldBackoff(long initialTimestamp, int failedAttempts)
		{
			return ShouldBackoff(initialTimestamp, failedAttempts, DefaultIntervalInMs, MaxBackoffIntervalMs);
		}
		private static long NanoTime()
		{
			var nano = 10000L * Stopwatch.GetTimestamp();
			nano /= TimeSpan.TicksPerMillisecond;
			nano *= 100L;
			return nano;
		}
	}


}
