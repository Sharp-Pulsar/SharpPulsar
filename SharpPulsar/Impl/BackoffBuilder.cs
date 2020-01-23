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

	public class BackoffBuilder
	{
		private long backoffIntervalNanos;
		private long maxBackoffIntervalNanos;
		private long initial;
		private BAMCIS.Util.Concurrent.TimeUnit unitInitial;
		private long max;
		private BAMCIS.Util.Concurrent.TimeUnit unitMax;
		private Clock clock;
		private long mandatoryStop;
		private BAMCIS.Util.Concurrent.TimeUnit unitMandatoryStop;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting BackoffBuilder()
		public BackoffBuilder()
		{
			this.initial = 0;
			this.max = 0;
			this.mandatoryStop = 0;
			this.clock = Clock.systemDefaultZone();
			this.backoffIntervalNanos = 0;
			this.maxBackoffIntervalNanos = 0;
		}

		public virtual BackoffBuilder SetInitialTime(long Initial, BAMCIS.Util.Concurrent.TimeUnit UnitInitial)
		{
			this.unitInitial = UnitInitial;
			this.initial = Initial;
			return this;
		}

		public virtual BackoffBuilder SetMax(long Max, BAMCIS.Util.Concurrent.TimeUnit UnitMax)
		{
			this.unitMax = UnitMax;
			this.max = Max;
			return this;
		}

		public virtual BackoffBuilder SetMandatoryStop(long MandatoryStop, BAMCIS.Util.Concurrent.TimeUnit UnitMandatoryStop)
		{
			this.mandatoryStop = MandatoryStop;
			this.unitMandatoryStop = UnitMandatoryStop;
			return this;
		}


		public virtual Backoff Create()
		{
			return new Backoff(initial, unitInitial, max, unitMax, mandatoryStop, unitMandatoryStop, clock);
		}
	}

}