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
namespace Org.Apache.Pulsar.Client.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.testng.Assert.assertTrue;

	using Mockito = org.mockito.Mockito;
	using Test = org.testng.annotations.Test;

	public class BackoffTest
	{
		public virtual bool WithinTenPercentAndDecrementTimer(Backoff Backoff, long T2)
		{
			long T1 = Backoff.next();
			return (T1 >= T2 * 0.9 && T1 <= T2);
		}

		public virtual bool CheckExactAndDecrementTimer(Backoff Backoff, long T2)
		{
			long T1 = Backoff.next();
			return T1 == T2;
		}
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBackoffTest()
		public virtual void ShouldBackoffTest()
		{
			// gives false
			assertFalse(Backoff.ShouldBackoff(0L, TimeUnit.NANOSECONDS, 0));
			long CurrentTimestamp = System.nanoTime();
			// gives true
			assertTrue(Backoff.ShouldBackoff(CurrentTimestamp, TimeUnit.NANOSECONDS, 100));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mandatoryStopTestNegativeTest()
		public virtual void MandatoryStopTestNegativeTest()
		{
			Backoff Backoff = new Backoff(100, TimeUnit.MILLISECONDS, 60, TimeUnit.SECONDS, 1900, TimeUnit.MILLISECONDS);
			assertEquals(Backoff.next(), 100);
			Backoff.next(); // 200
			Backoff.next(); // 400
			Backoff.next(); // 800
			assertFalse(WithinTenPercentAndDecrementTimer(Backoff, 400));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void firstBackoffTimerTest()
		public virtual void FirstBackoffTimerTest()
		{
			Clock MockClock = Mockito.mock(typeof(Clock));
			Mockito.when(MockClock.millis()).thenReturn(0L).thenReturn(300L);

			Backoff Backoff = new Backoff(100, TimeUnit.MILLISECONDS, 60, TimeUnit.SECONDS, 1900, TimeUnit.MILLISECONDS, MockClock);

			assertEquals(Backoff.next(), 100);

			long FirstBackOffTime = Backoff.FirstBackoffTimeInMillis;
			Backoff.reset();
			assertEquals(Backoff.next(), 100);
			long DiffBackOffTime = Backoff.FirstBackoffTimeInMillis - FirstBackOffTime;
			assertEquals(DiffBackOffTime, 300);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void basicTest()
		public virtual void BasicTest()
		{
			Clock MockClock = Clock.@fixed(Instant.EPOCH, ZoneId.systemDefault());
			Backoff Backoff = new Backoff(5, TimeUnit.MILLISECONDS, 60, TimeUnit.SECONDS, 60, TimeUnit.SECONDS, MockClock);
			assertTrue(CheckExactAndDecrementTimer(Backoff, 5));
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 10));
			Backoff.reset();
			assertTrue(CheckExactAndDecrementTimer(Backoff, 5));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void maxTest()
		public virtual void MaxTest()
		{
			Clock MockClock = Mockito.mock(typeof(Clock));
			Mockito.when(MockClock.millis()).thenReturn(0L).thenReturn(10L).thenReturn(20L).thenReturn(40L);

			Backoff Backoff = new Backoff(5, TimeUnit.MILLISECONDS, 20, TimeUnit.MILLISECONDS, 20, TimeUnit.MILLISECONDS, MockClock);

			assertTrue(CheckExactAndDecrementTimer(Backoff, 5));
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 10));
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 5));
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 20));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mandatoryStopTest()
		public virtual void MandatoryStopTest()
		{
			Clock MockClock = Mockito.mock(typeof(Clock));

			Backoff Backoff = new Backoff(100, TimeUnit.MILLISECONDS, 60, TimeUnit.SECONDS, 1900, TimeUnit.MILLISECONDS, MockClock);

			Mockito.when(MockClock.millis()).thenReturn(0L);
			assertTrue(CheckExactAndDecrementTimer(Backoff, 100));
			Mockito.when(MockClock.millis()).thenReturn(100L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 200));
			Mockito.when(MockClock.millis()).thenReturn(300L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 400));
			Mockito.when(MockClock.millis()).thenReturn(700L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 800));
			Mockito.when(MockClock.millis()).thenReturn(1500L);

			// would have been 1600 w/o the mandatory stop
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 400));
			Mockito.when(MockClock.millis()).thenReturn(1900L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 3200));
			Mockito.when(MockClock.millis()).thenReturn(3200L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 6400));
			Mockito.when(MockClock.millis()).thenReturn(3200L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 12800));
			Mockito.when(MockClock.millis()).thenReturn(6400L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 25600));
			Mockito.when(MockClock.millis()).thenReturn(12800L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 51200));
			Mockito.when(MockClock.millis()).thenReturn(25600L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 60000));
			Mockito.when(MockClock.millis()).thenReturn(51200L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 60000));
			Mockito.when(MockClock.millis()).thenReturn(60000L);

			Backoff.reset();
			Mockito.when(MockClock.millis()).thenReturn(0L);
			assertTrue(CheckExactAndDecrementTimer(Backoff, 100));
			Mockito.when(MockClock.millis()).thenReturn(100L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 200));
			Mockito.when(MockClock.millis()).thenReturn(300L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 400));
			Mockito.when(MockClock.millis()).thenReturn(700L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 800));
			Mockito.when(MockClock.millis()).thenReturn(1500L);
			// would have been 1600 w/o the mandatory stop
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 400));

			Backoff.reset();
			Mockito.when(MockClock.millis()).thenReturn(0L);
			assertTrue(CheckExactAndDecrementTimer(Backoff, 100));
			Mockito.when(MockClock.millis()).thenReturn(100L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 200));
			Mockito.when(MockClock.millis()).thenReturn(300L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 400));
			Mockito.when(MockClock.millis()).thenReturn(700L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 800));

			Backoff.reset();
			Mockito.when(MockClock.millis()).thenReturn(0L);
			assertTrue(CheckExactAndDecrementTimer(Backoff, 100));
			Mockito.when(MockClock.millis()).thenReturn(100L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 200));
			Mockito.when(MockClock.millis()).thenReturn(300L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 400));
			Mockito.when(MockClock.millis()).thenReturn(700L);
			assertTrue(WithinTenPercentAndDecrementTimer(Backoff, 800));
		}

	}

}