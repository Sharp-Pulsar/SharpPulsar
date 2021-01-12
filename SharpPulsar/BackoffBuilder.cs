using BAMCIS.Util.Concurrent;
using System;

namespace SharpPulsar
{
	public class BackoffBuilder
	{
		private long _backoffIntervalNanos;
		private long _maxBackoffIntervalNanos;
		private long _initial;
		private TimeUnit _unitInitial;
		private long _max;
		private TimeUnit _unitMax;
		private DateTimeOffset _clock;
		private long _mandatoryStop;
		private TimeUnit _unitMandatoryStop;
		internal BackoffBuilder()
		{
			_initial = 0;
			_max = 0;
			_mandatoryStop = 0;
			_clock = DateTimeOffset.Now;
			_backoffIntervalNanos = 0;
			_maxBackoffIntervalNanos = 0;
		}

		public virtual BackoffBuilder SetInitialTime(long initial, TimeUnit unitInitial)
		{
			_unitInitial = unitInitial;
			_initial = initial;
			return this;
		}

		public virtual BackoffBuilder SetMax(long max, TimeUnit unitMax)
		{
			_unitMax = unitMax;
			_max = max;
			return this;
		}

		public virtual BackoffBuilder SetMandatoryStop(long mandatoryStop, TimeUnit unitMandatoryStop)
		{
			_mandatoryStop = mandatoryStop;
			_unitMandatoryStop = unitMandatoryStop;
			return this;
		}


		public virtual Backoff Create()
		{
			return new Backoff(_initial, _unitInitial, _max, _unitMax, _mandatoryStop, _unitMandatoryStop, _clock);
		}
	}

}
