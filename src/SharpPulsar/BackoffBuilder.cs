using System;

namespace SharpPulsar
{
	public class BackoffBuilder
	{
		//private readonly long _backoffIntervalNanos;
        //private readonly long _maxBackoffIntervalNanos;
        private TimeSpan _initial;
		private TimeSpan _max;
		private readonly DateTimeOffset _clock;
		private TimeSpan _mandatoryStop;
		internal BackoffBuilder()
		{
			_initial = TimeSpan.Zero;
			_max = TimeSpan.Zero;
			_mandatoryStop = TimeSpan.Zero;
			_clock = DateTimeOffset.Now;
			//_backoffIntervalNanos = 0;
			//_maxBackoffIntervalNanos = 0;
		}

		public virtual BackoffBuilder SetInitialTime(TimeSpan initial)
		{
			_initial = initial;
			return this;
		}

		public virtual BackoffBuilder SetMax(TimeSpan max)
		{
			_max = max;
			return this;
		}

		public virtual BackoffBuilder SetMandatoryStop(TimeSpan mandatoryStop)
		{
			_mandatoryStop = mandatoryStop;
			return this;
		}


		public virtual Backoff Create()
		{
			return new Backoff(_initial, _max, _mandatoryStop, _clock);
		}
	}

}
