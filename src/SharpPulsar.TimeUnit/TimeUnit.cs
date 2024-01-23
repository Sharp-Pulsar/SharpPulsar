// https://github.com/bamcis-io/TimeUnit/blob/main/TimeUnit/TimeUnit.cs

namespace SharpPulsar.TimeUnit
{
    /// <summary>
    /// A TimeUnit represents time durations at a given unit of granularity
    /// and provides utility methods to convert across units, and to perform
    /// timing and delay operations in these units. A TimeUnit does not maintain
    /// time information, but only helps organize and use time representations
    /// that may be maintained separately across various contexts. 
    /// 
    /// TimeUnit is mainly used to inform time-based methods how a given timing 
    /// parameter should be interpreted.
    /// </summary>
    public sealed class TimeUnit
    {
        #region Private Fields

        private string Value;

        private const string _DAYS = "DAYS";
        private const string _HOURS = "HOURS";
        private const string _MICROSECONDS = "MICROSECONDS";
        private const string _MILLISECONDS = "MILLISECONDS";
        private const string _MINUTES = "MINUTES";
        private const string _NANOSECONDS = "NANOSECONDS";
        private const string _SECONDS = "SECONDS";
        private const string _TICKS = "TICKS";

        private static readonly long C0 = 1L;
        private static readonly long NanosecondsPerTick = C0 * 100;
        private static readonly long NanosecondsPerMicrosecond = NanosecondsPerTick * 10;
        private static readonly long NanosecondsPerMillisecond = NanosecondsPerMicrosecond * 1000L;
        private static readonly long NanosecondsPerSecond = NanosecondsPerMillisecond * 1000L;
        private static readonly long NanosecondsPerMinute = NanosecondsPerSecond * 60L;
        private static readonly long NanosecondsPerHour = NanosecondsPerMinute * 60L;
        private static readonly long NanosecondsPerDay = NanosecondsPerHour * 24L;

        private Func<long, long> _ToNanos;
        private Func<long, long> _ToTicks;
        private Func<long, long> _ToMicros;
        private Func<long, long> _ToMillis;
        private Func<long, long> _ToSeconds;
        private Func<long, long> _ToMinutes;
        private Func<long, long> _ToHours;
        private Func<long, long> _ToDays;
        private Func<long, long, int> _ExcessNanos;

        #endregion

        #region Public Constants

        public static readonly TimeUnit NANOSECONDS = new TimeUnit(
            _NANOSECONDS,
            (long duration) => { return duration; }, // Nanos
            (long duration) => { return duration / NanosecondsPerTick; }, // Ticks
            (long duration) => { return duration / NanosecondsPerMicrosecond; }, // Micros
            (long duration) => { return duration / NanosecondsPerMillisecond; }, // Millis
            (long duration) => { return duration / NanosecondsPerSecond; }, // Seconds
            (long duration) => { return duration / NanosecondsPerMinute; }, // Minutes
            (long duration) => { return duration / NanosecondsPerHour; }, // Hours
            (long duration) => { return duration / NanosecondsPerDay; },  // Days
            (long x, long y) => { return (int)(x - (y * NanosecondsPerMillisecond)); } // ExcessNanos
        );

        public static readonly TimeUnit TICKS = new TimeUnit(
            _TICKS,
            (long duration) => { return scale(duration, NanosecondsPerTick, long.MaxValue / NanosecondsPerTick); }, // Nanos
            (long duration) => { return duration; }, // Ticks
            (long duration) => { return duration / (NanosecondsPerMicrosecond / NanosecondsPerTick); }, // Micros
            (long duration) => { return duration / (NanosecondsPerMillisecond / NanosecondsPerTick); }, // Millis
            (long duration) => { return duration / (NanosecondsPerSecond / NanosecondsPerTick); }, // Seconds
            (long duration) => { return duration / (NanosecondsPerMinute / NanosecondsPerTick); }, // Minutes
            (long duration) => { return duration / (NanosecondsPerHour / NanosecondsPerTick); }, // Hours
            (long duration) => { return duration / (NanosecondsPerDay / NanosecondsPerTick); },  // Days
            (long x, long y) => { return (int)((x * NanosecondsPerTick) - (y * NanosecondsPerMillisecond)); } // ExcessNanos
        );

        public static readonly TimeUnit MICROSECONDS = new TimeUnit(
            _MICROSECONDS,
            (long duration) => { return scale(duration, NanosecondsPerMicrosecond, long.MaxValue / NanosecondsPerMicrosecond); }, // Nanos
            (long duration) => { return scale(duration, NanosecondsPerMicrosecond / NanosecondsPerTick, long.MaxValue / (NanosecondsPerMicrosecond / NanosecondsPerTick)); }, // Ticks
            (long duration) => { return duration; }, // Micros
            (long duration) => { return duration / (NanosecondsPerMillisecond / NanosecondsPerMicrosecond); }, // Millis
            (long duration) => { return duration / (NanosecondsPerSecond / NanosecondsPerMicrosecond); }, // Seconds
            (long duration) => { return duration / (NanosecondsPerMinute / NanosecondsPerMicrosecond); }, // Minutes
            (long duration) => { return duration / (NanosecondsPerHour / NanosecondsPerMicrosecond); }, // Hours
            (long duration) => { return duration / (NanosecondsPerDay / NanosecondsPerMicrosecond); },  // Days
            (long x, long y) => { return 0; } // ExcessNanos
        );

        public static readonly TimeUnit MILLISECONDS = new TimeUnit(
            _MILLISECONDS,
            (long duration) => { return scale(duration, NanosecondsPerMillisecond, long.MaxValue / NanosecondsPerMillisecond); }, // Nanos
            (long duration) => { return scale(duration, NanosecondsPerMillisecond / NanosecondsPerTick, long.MaxValue / (NanosecondsPerMillisecond / NanosecondsPerTick)); }, // Ticks
            (long duration) => { return scale(duration, NanosecondsPerMillisecond / NanosecondsPerMicrosecond, long.MaxValue / (NanosecondsPerMillisecond / NanosecondsPerMicrosecond)); }, // Micros
            (long duration) => { return duration; }, // Millis
            (long duration) => { return duration / (NanosecondsPerSecond / NanosecondsPerMillisecond); }, // Seconds
            (long duration) => { return duration / (NanosecondsPerMinute / NanosecondsPerMillisecond); }, // Minutes
            (long duration) => { return duration / (NanosecondsPerHour / NanosecondsPerMillisecond); }, // Hours
            (long duration) => { return duration / (NanosecondsPerDay / NanosecondsPerMillisecond); },  // Days
            (long x, long y) => { return 0; } // ExcessNanos
        );

        public static readonly TimeUnit SECONDS = new TimeUnit(
            _SECONDS,
            (long duration) => { return scale(duration, NanosecondsPerSecond, long.MaxValue / NanosecondsPerSecond); }, // Nanos
            (long duration) => { return scale(duration, NanosecondsPerSecond / NanosecondsPerTick, long.MaxValue / (NanosecondsPerSecond / NanosecondsPerTick)); }, // Ticks
            (long duration) => { return scale(duration, NanosecondsPerSecond / NanosecondsPerMicrosecond, long.MaxValue / (NanosecondsPerSecond / NanosecondsPerMicrosecond)); }, // Micros
            (long duration) => { return scale(duration, NanosecondsPerSecond / NanosecondsPerMillisecond, long.MaxValue / (NanosecondsPerSecond / NanosecondsPerMillisecond)); }, // Millis
            (long duration) => { return duration; }, // Seconds
            (long duration) => { return duration / (NanosecondsPerMinute / NanosecondsPerSecond); }, // Minutes
            (long duration) => { return duration / (NanosecondsPerHour / NanosecondsPerSecond); }, // Hours
            (long duration) => { return duration / (NanosecondsPerDay / NanosecondsPerSecond); },  // Days
            (long x, long y) => { return 0; } // ExcessNanos
        );

        public static readonly TimeUnit MINUTES = new TimeUnit(
            _MINUTES,
            (long duration) => { return scale(duration, NanosecondsPerMinute, long.MaxValue / NanosecondsPerMinute); }, // Nanos
            (long duration) => { return scale(duration, NanosecondsPerMinute / NanosecondsPerTick, long.MaxValue / (NanosecondsPerMinute / NanosecondsPerTick)); }, // Ticks
            (long duration) => { return scale(duration, NanosecondsPerMinute / NanosecondsPerMicrosecond, long.MaxValue / (NanosecondsPerMinute / NanosecondsPerMicrosecond)); }, // Micros
            (long duration) => { return scale(duration, NanosecondsPerMinute / NanosecondsPerMillisecond, long.MaxValue / (NanosecondsPerMinute / NanosecondsPerMillisecond)); }, // Millis
            (long duration) => { return scale(duration, NanosecondsPerMinute / NanosecondsPerSecond, long.MaxValue / (NanosecondsPerMinute / NanosecondsPerSecond)); }, // Seconds
            (long duration) => { return duration; }, // Minutes
            (long duration) => { return duration / (NanosecondsPerHour / NanosecondsPerMinute); }, // Hours
            (long duration) => { return duration / (NanosecondsPerDay / NanosecondsPerMinute); },  // Days
            (long x, long y) => { return 0; } // ExcessNanos
        );

        public static readonly TimeUnit HOURS = new TimeUnit(
            _HOURS,
            (long duration) => { return scale(duration, NanosecondsPerHour, long.MaxValue / NanosecondsPerHour); }, // Nanos
            (long duration) => { return scale(duration, NanosecondsPerHour / NanosecondsPerTick, long.MaxValue / (NanosecondsPerHour / NanosecondsPerTick)); }, // Ticks
            (long duration) => { return scale(duration, NanosecondsPerHour / NanosecondsPerMicrosecond, long.MaxValue / (NanosecondsPerHour / NanosecondsPerMicrosecond)); }, // Micros
            (long duration) => { return scale(duration, NanosecondsPerHour / NanosecondsPerMillisecond, long.MaxValue / (NanosecondsPerHour / NanosecondsPerMillisecond)); }, // Millis
            (long duration) => { return scale(duration, NanosecondsPerHour / NanosecondsPerSecond, long.MaxValue / (NanosecondsPerHour / NanosecondsPerSecond)); }, // Seconds
            (long duration) => { return scale(duration, NanosecondsPerHour / NanosecondsPerMinute, long.MaxValue / (NanosecondsPerHour / NanosecondsPerMinute)); }, // Minutes
            (long duration) => { return duration; }, // Hours
            (long duration) => { return duration / (NanosecondsPerDay / NanosecondsPerHour); },  // Days
            (long x, long y) => { return 0; } // ExcessNanos
        );

        public static readonly TimeUnit DAYS = new TimeUnit(
            _DAYS,
            (long duration) => { return scale(duration, NanosecondsPerDay, long.MaxValue / NanosecondsPerDay); }, // Nanos
            (long duration) => { return scale(duration, NanosecondsPerDay / NanosecondsPerTick, long.MaxValue / (NanosecondsPerDay / NanosecondsPerTick)); }, // Ticks
            (long duration) => { return scale(duration, NanosecondsPerDay / NanosecondsPerMicrosecond, long.MaxValue / (NanosecondsPerDay / NanosecondsPerMicrosecond)); }, // Micros
            (long duration) => { return scale(duration, NanosecondsPerDay / NanosecondsPerMillisecond, long.MaxValue / (NanosecondsPerDay / NanosecondsPerMillisecond)); }, // Millis
            (long duration) => { return scale(duration, NanosecondsPerDay / NanosecondsPerSecond, long.MaxValue / (NanosecondsPerDay / NanosecondsPerSecond)); }, // Seconds
            (long duration) => { return scale(duration, NanosecondsPerDay / NanosecondsPerMinute, long.MaxValue / (NanosecondsPerDay / NanosecondsPerMinute)); }, // Minutes
            (long duration) => { return scale(duration, NanosecondsPerDay / NanosecondsPerHour, long.MaxValue / (NanosecondsPerDay / NanosecondsPerHour)); }, // Hours
            (long duration) => { return duration; },  // Days
            (long x, long y) => { return 0; } // ExcessNanos
        );

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new time unit
        /// </summary>
        /// <param name="value"></param>
        /// <param name="toNanos"></param>
        /// <param name="toTicks"></param>
        /// <param name="toMicros"></param>
        /// <param name="toMillis"></param>
        /// <param name="toSeconds"></param>
        /// <param name="toMinutes"></param>
        /// <param name="toHours"></param>
        /// <param name="toDays"></param>
        /// <param name="excessNanos"></param>
        private TimeUnit(string value,
            Func<long, long> toNanos,
            Func<long, long> toTicks,
            Func<long, long> toMicros,
            Func<long, long> toMillis,
            Func<long, long> toSeconds,
            Func<long, long> toMinutes,
            Func<long, long> toHours,
            Func<long, long> toDays,
            Func<long, long, int> excessNanos
        )
        {
            this.Value = value;
            this._ToNanos = toNanos;
            this._ToTicks = toTicks;
            this._ToMicros = toMicros;
            this._ToMillis = toMillis;
            this._ToSeconds = toSeconds;
            this._ToMinutes = toMinutes;
            this._ToHours = toHours;
            this._ToDays = toDays;
            this._ExcessNanos = excessNanos;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs a Thread.Sleep() using this unit. This is a convenience method that
        /// converts time arguments into milliseconds used by Thread.Sleep().
        /// </summary>
        /// <param name="timeout">The minimum time to sleep, if less than or equal to zero, it will not sleep at all</param>
        public void Sleep(long timeout)
        {
            if (timeout > 0)
            {
                long Milliseconds = this.ToMilliseconds(timeout);
                Thread.Sleep((int)Milliseconds);
            }
        }

        /// <summary>
        /// Performs a timed Thread.Join() using this time unit. This is a convenience method that converts
        /// time arguments into milliseconds used by Join().
        /// </summary>
        /// <param name="thread">The thread to wait for</param>
        /// <param name="timeout">The maximum time to wait, if less than or equal to zero, it will not sleep at all</param>
        public void TimedJoin(Thread thread, long timeout)
        {
            if (timeout > 0)
            {
                long Milliseconds = this.ToMilliseconds(timeout);
                thread.Join((int)Milliseconds);
            }
        }

        /// <summary>
        /// Performs a timed Monitor.Wait() using this time unit. This is a convenience methods that
        /// converts timeout arguments into milliseconds using by Wait(). For example,
        /// 
        /// public Object poll(long timeout, TimeUnit unit) 
        /// {
        ///     while (true) {
        ///         unit.TimedWait(this, timeout);
        ///         ...
        ///     }
        /// }
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="timeout"></param>
        public void TimedWait(Object obj, long timeout)
        {
            if (timeout > 0)
            {
                long Milliseconds = this.ToMilliseconds(timeout);
                Monitor.Wait(obj, (int)Milliseconds);
            }
        }

        public long ToNanoseconds(long duration)
        {
            return this._ToNanos(duration);
        }

        public long ToTicks(long duration)
        {
            return this._ToTicks(duration);
        }

        public long ToMicroseconds(long duration)
        {
            return this._ToMicros(duration);
        }

        public long ToMilliseconds(long duration)
        {
            return this._ToMillis(duration);
        }

        public long ToSeconds(long duration)
        {
            return this._ToSeconds(duration);
        }

        public long ToMinutes(long duration)
        {
            return this._ToMinutes(duration);
        }

        public long ToHours(long duration)
        {
            return this._ToHours(duration);
        }

        public long ToDays(long duration)
        {
            return this._ToDays(duration);
        }

        /// <summary>
        /// Converts the give time duration in the given unit to this unit. Conversions from finer
        /// to coarser granularities truncate, and thus lose precision. For example, converting from 999
        /// milliseconds to seconds results in 0. Conversions from coarser to finer granularities with arguments
        /// that would numerically overflow saturate to Int64.MaxValue if positive or Int64.MinValue if negative.
        /// 
        /// For example, to convert 10 minutes to milliseconds, use:
        /// TimeUnit.MILLISECONDS.Convert(10, TimeUnit.MINUTES);
        /// </summary>
        /// <param name="sourceDuration">The time duration in the given source unit</param>
        /// <param name="sourceUnit">The unit of the sourceDuration argument</param>
        /// <returns>The converted duration of this unit or Int64.MinValue if conversion would negatively overflow or Int64.MaxValue if it would
        /// positively overflow</returns>
        public long Convert(long sourceDuration, TimeUnit sourceUnit)
        {
            switch (this.Value)
            {
                case _DAYS:
                    {
                        return sourceUnit._ToDays(sourceDuration);
                    }
                case _HOURS:
                    {
                        return sourceUnit.ToHours(sourceDuration);
                    }
                case _MINUTES:
                    {
                        return sourceUnit.ToMinutes(sourceDuration);
                    }
                case _SECONDS:
                    {
                        return sourceUnit.ToSeconds(sourceDuration);
                    }
                case _MILLISECONDS:
                    {
                        return sourceUnit.ToMilliseconds(sourceDuration);
                    }
                case _MICROSECONDS:
                    {
                        return sourceUnit.ToMicroseconds(sourceDuration);
                    }
                case _TICKS:
                    {
                        return sourceUnit.ToTicks(sourceDuration);
                    }
                case _NANOSECONDS:
                    {
                        return sourceUnit.ToNanoseconds(sourceDuration);
                    }
                default:
                    {
                        throw new ArgumentException($"The time unit {this.Value} is unrecognized.");
                    }
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            TimeUnit Other = (TimeUnit)obj;

            return this.Value == Other.Value;
        }

        public override string ToString()
        {
            return this.Value;
        }

        public override int GetHashCode()
        {
            return Hashing.Hash(this.Value);
        }

        public static bool operator ==(TimeUnit left, TimeUnit right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (right is null || left is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(TimeUnit left, TimeUnit right)
        {
            return !(left == right);
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Utility to compute the excess nanosecond argument to wait
        /// </summary>
        /// <param name="duration">The duration of this time unit</param>
        /// <param name="milliseconds">The number of milliseconds to wait</param>
        /// <returns>The number of excess nanoseconds</returns>
        private int ExcessNanos(long duration, long milliseconds)
        {
            return this._ExcessNanos(duration, milliseconds);
        }

        /// <summary>
        /// Scale x by y, checking for overflow
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="over"></param>
        /// <returns></returns>
        private static long scale(long x, long y, long over)
        {
            if (x > over)
            {
                return long.MaxValue;
            }

            if (x < -over)
            {
                return long.MinValue;
            }

            return x * y;

        }

        #endregion
    }
}
