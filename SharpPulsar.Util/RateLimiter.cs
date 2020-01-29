using System;
using System.Threading;

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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	using MoreObjects = com.google.common.@base.MoreObjects;

	/// <summary>
	/// A Rate Limiter that distributes permits at a configurable rate. Each <seealso cref="acquire()"/> blocks if necessary until a
	/// permit is available, and then takes it. Each <seealso cref="tryAcquire()"/> tries to acquire permits from available permits,
	/// it returns true if it succeed else returns false. Rate limiter release configured permits at every configured rate
	/// time, so, on next ticket new fresh permits will be available.
	/// 
	/// <para>For example: if RateLimiter is configured to release 10 permits at every 1 second then RateLimiter will allow to
	/// acquire 10 permits at any time with in that 1 second.
	/// 
	/// </para>
	/// <para>Comparison with other RateLimiter such as <seealso cref="com.google.common.util.concurrent.RateLimiter"/>
	/// <ul>
	/// <li><b>Per second rate-limiting:</b> Per second rate-limiting not satisfied by Guava-RateLimiter</li>
	/// <li><b>Guava RateLimiter:</b> For X permits: it releases X/1000 permits every msec. therefore,
	/// for permits=2/sec =&gt; it release 1st permit on first 500msec and 2nd permit on next 500ms. therefore,
	/// if 2 request comes with in 500msec duration then 2nd request fails to acquire permit
	/// though we have configured 2 permits/second.</li>
	/// <li><b>RateLimiter:</b> it releases X permits every second. so, in above usecase:
	/// if 2 requests comes at the same time then both will acquire the permit.</li>
	/// <li><b>Faster: </b>RateLimiter is light-weight and faster than Guava-RateLimiter</li>
	/// </ul>
	/// </para>
	/// </summary>
	public class RateLimiter : AutoCloseable
	{

		private readonly ScheduledExecutorService executorService;
		private long rateTime;
		private TimeUnit timeUnit;
		private readonly bool externalExecutor;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.concurrent.ScheduledFuture<?> renewTask;
		private ScheduledFuture<object> renewTask;
		private long permits;
		private long acquiredPermits;
		private bool isClosed;
		// permitUpdate helps to update permit-rate at runtime
		private System.Func<long> permitUpdater;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public RateLimiter(final long permits, final long rateTime, final java.util.concurrent.TimeUnit timeUnit)
		public RateLimiter(long permits, long rateTime, TimeUnit timeUnit) : this(null, permits, rateTime, timeUnit, null)
		{
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public RateLimiter(final java.util.concurrent.ScheduledExecutorService service, final long permits, final long rateTime, final java.util.concurrent.TimeUnit timeUnit, java.util.function.Supplier<long> permitUpdater)
		public RateLimiter(ScheduledExecutorService service, long permits, long rateTime, TimeUnit timeUnit, System.Func<long> permitUpdater)
		{
			checkArgument(permits > 0, "rate must be > 0");
			checkArgument(rateTime > 0, "Renew permit time must be > 0");

			this.rateTime = rateTime;
			this.timeUnit = timeUnit;
			this.permits = permits;
			this.permitUpdater = permitUpdater;

			if (service != null)
			{
				this.executorService = service;
				this.externalExecutor = true;
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.ScheduledThreadPoolExecutor executor = new java.util.concurrent.ScheduledThreadPoolExecutor(1);
				ScheduledThreadPoolExecutor executor = new ScheduledThreadPoolExecutor(1);
				executor.ContinueExistingPeriodicTasksAfterShutdownPolicy = false;
				executor.ExecuteExistingDelayedTasksAfterShutdownPolicy = false;
				this.executorService = executor;
				this.externalExecutor = false;
			}

		}

		public override void close()
		{
			lock (this)
			{
				if (!isClosed)
				{
					if (!externalExecutor)
					{
						executorService.shutdownNow();
					}
					if (renewTask != null)
					{
						renewTask.cancel(false);
					}
					isClosed = true;
				}
			}
		}

		public virtual bool Closed
		{
			get
			{
				lock (this)
				{
					return isClosed;
				}
			}
		}

		/// <summary>
		/// Acquires the given number of permits from this {@code RateLimiter}, blocking until the request be granted.
		/// 
		/// <para>This method is equivalent to {@code acquire(1)}.
		/// </para>
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void acquire() throws InterruptedException
		public virtual void acquire()
		{
			lock (this)
			{
				acquire(1);
			}
		}

		/// <summary>
		/// Acquires the given number of permits from this {@code RateLimiter}, blocking until the request be granted.
		/// </summary>
		/// <param name="acquirePermit">
		///            the number of permits to acquire </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void acquire(long acquirePermit) throws InterruptedException
		public virtual void acquire(long acquirePermit)
		{
			lock (this)
			{
				checkArgument(!Closed, "Rate limiter is already shutdown");
				checkArgument(acquirePermit <= this.permits, "acquiring permits must be less or equal than initialized rate =" + this.permits);
        
				// lazy init and start task only once application start using it
				if (renewTask == null)
				{
					renewTask = createTask();
				}
        
				bool canAcquire = false;
				do
				{
					canAcquire = acquirePermit < 0 || acquiredPermits < this.permits;
					if (!canAcquire)
					{
						Monitor.Wait(this);
					}
					else
					{
						acquiredPermits += acquirePermit;
					}
				} while (!canAcquire);
			}
		}

		/// <summary>
		/// Acquires permits from this <seealso cref="RateLimiter"/> if it can be acquired immediately without delay.
		/// 
		/// <para>This method is equivalent to {@code tryAcquire(1)}.
		/// 
		/// </para>
		/// </summary>
		/// <returns> {@code true} if the permits were acquired, {@code false} otherwise </returns>
		public virtual bool tryAcquire()
		{
			lock (this)
			{
				return tryAcquire(1);
			}
		}

		/// <summary>
		/// Acquires permits from this <seealso cref="RateLimiter"/> if it can be acquired immediately without delay.
		/// </summary>
		/// <param name="acquirePermit">
		///            the number of permits to acquire </param>
		/// <returns> {@code true} if the permits were acquired, {@code false} otherwise </returns>
		public virtual bool tryAcquire(long acquirePermit)
		{
			lock (this)
			{
				checkArgument(!Closed, "Rate limiter is already shutdown");
				// lazy init and start task only once application start using it
				if (renewTask == null)
				{
					renewTask = createTask();
				}
        
				// acquired-permits can't be larger than the rate
				if (acquirePermit > this.permits)
				{
					acquiredPermits = this.permits;
					return false;
				}
				bool canAcquire = acquirePermit < 0 || acquiredPermits < this.permits;
				if (canAcquire)
				{
					acquiredPermits += acquirePermit;
				}
				return canAcquire;
			}
		}

		/// <summary>
		/// Return available permits for this <seealso cref="RateLimiter"/>.
		/// </summary>
		/// <returns> returns 0 if permits is not available </returns>
		public virtual long AvailablePermits
		{
			get
			{
				lock (this)
				{
					return Math.Max(0, this.permits - this.acquiredPermits);
				}
			}
		}

		/// <summary>
		/// Resets new rate by configuring new value for permits per configured rate-period.
		/// </summary>
		/// <param name="permits"> </param>
		public virtual long Rate
		{
			set
			{
				lock (this)
				{
					this.permits = value;
				}
			}
			get
			{
				lock (this)
				{
					return this.permits;
				}
			}
		}

		/// <summary>
		/// Resets new rate with new permits and rate-time.
		/// </summary>
		/// <param name="permits"> </param>
		/// <param name="rateTime"> </param>
		/// <param name="timeUnit"> </param>
		/// <param name="permitUpdaterByte"> </param>
		public virtual void setRate(long permits, long rateTime, TimeUnit timeUnit, System.Func<long> permitUpdaterByte)
		{
			lock (this)
			{
				if (renewTask != null)
				{
					renewTask.cancel(false);
				}
				this.permits = permits;
				this.rateTime = rateTime;
				this.timeUnit = timeUnit;
				this.permitUpdater = permitUpdaterByte;
				this.renewTask = createTask();
			}
		}


		public virtual long RateTime
		{
			get
			{
				lock (this)
				{
					return this.rateTime;
				}
			}
		}

		public virtual TimeUnit RateTimeUnit
		{
			get
			{
				lock (this)
				{
					return this.timeUnit;
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected java.util.concurrent.ScheduledFuture<?> createTask()
		protected internal virtual ScheduledFuture<object> createTask()
		{
			return executorService.scheduleAtFixedRate(this.renew, this.rateTime, this.rateTime, this.timeUnit);
		}

		internal virtual void renew()
		{
			lock (this)
			{
				acquiredPermits = 0;
				if (permitUpdater != null)
				{
					long newPermitRate = permitUpdater.get();
					if (newPermitRate > 0)
					{
						Rate = newPermitRate;
					}
				}
				Monitor.PulseAll(this);
			}
		}

		public override string ToString()
		{
			return MoreObjects.toStringHelper(this).add("rateTime", rateTime).add("permits", permits).add("acquiredPermits", acquiredPermits).ToString();
		}

	}

}