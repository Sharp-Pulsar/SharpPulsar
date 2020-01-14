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
namespace org.apache.pulsar.client.impl
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
		internal virtual bool withinTenPercentAndDecrementTimer(Backoff backoff, long t2)
		{
			long t1 = backoff.next();
			return (t1 >= t2 * 0.9 && t1 <= t2);
		}

		internal virtual bool checkExactAndDecrementTimer(Backoff backoff, long t2)
		{
			long t1 = backoff.next();
			return t1 == t2;
		}
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBackoffTest()
		public virtual void shouldBackoffTest()
		{
			// gives false
			assertFalse(Backoff.shouldBackoff(0L, TimeUnit.NANOSECONDS, 0));
			long currentTimestamp = System.nanoTime();
			// gives true
			assertTrue(Backoff.shouldBackoff(currentTimestamp, TimeUnit.NANOSECONDS, 100));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mandatoryStopTestNegativeTest()
		public virtual void mandatoryStopTestNegativeTest()
		{
			Backoff backoff = new Backoff(100, TimeUnit.MILLISECONDS, 60, TimeUnit.SECONDS, 1900, TimeUnit.MILLISECONDS);
			assertEquals(backoff.next(), 100);
			backoff.next(); // 200
			backoff.next(); // 400
			backoff.next(); // 800
			assertFalse(withinTenPercentAndDecrementTimer(backoff, 400));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void firstBackoffTimerTest()
		public virtual void firstBackoffTimerTest()
		{
			Clock mockClock = Mockito.mock(typeof(Clock));
			Mockito.when(mockClock.millis()).thenReturn(0L).thenReturn(300L);

			Backoff backoff = new Backoff(100, TimeUnit.MILLISECONDS, 60, TimeUnit.SECONDS, 1900, TimeUnit.MILLISECONDS, mockClock);

			assertEquals(backoff.next(), 100);

			long firstBackOffTime = backoff.FirstBackoffTimeInMillis;
			backoff.reset();
			assertEquals(backoff.next(), 100);
			long diffBackOffTime = backoff.FirstBackoffTimeInMillis - firstBackOffTime;
			assertEquals(diffBackOffTime, 300);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void basicTest()
		public virtual void basicTest()
		{
			Clock mockClock = Clock.@fixed(Instant.EPOCH, ZoneId.systemDefault());
			Backoff backoff = new Backoff(5, TimeUnit.MILLISECONDS, 60, TimeUnit.SECONDS, 60, TimeUnit.SECONDS, mockClock);
			assertTrue(checkExactAndDecrementTimer(backoff, 5));
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 10));
			backoff.reset();
			assertTrue(checkExactAndDecrementTimer(backoff, 5));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void maxTest()
		public virtual void maxTest()
		{
			Clock mockClock = Mockito.mock(typeof(Clock));
			Mockito.when(mockClock.millis()).thenReturn(0L).thenReturn(10L).thenReturn(20L).thenReturn(40L);

			Backoff backoff = new Backoff(5, TimeUnit.MILLISECONDS, 20, TimeUnit.MILLISECONDS, 20, TimeUnit.MILLISECONDS, mockClock);

			assertTrue(checkExactAndDecrementTimer(backoff, 5));
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 10));
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 5));
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 20));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mandatoryStopTest()
		public virtual void mandatoryStopTest()
		{
			Clock mockClock = Mockito.mock(typeof(Clock));

			Backoff backoff = new Backoff(100, TimeUnit.MILLISECONDS, 60, TimeUnit.SECONDS, 1900, TimeUnit.MILLISECONDS, mockClock);

			Mockito.when(mockClock.millis()).thenReturn(0L);
			assertTrue(checkExactAndDecrementTimer(backoff, 100));
			Mockito.when(mockClock.millis()).thenReturn(100L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 200));
			Mockito.when(mockClock.millis()).thenReturn(300L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 400));
			Mockito.when(mockClock.millis()).thenReturn(700L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 800));
			Mockito.when(mockClock.millis()).thenReturn(1500L);

			// would have been 1600 w/o the mandatory stop
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 400));
			Mockito.when(mockClock.millis()).thenReturn(1900L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 3200));
			Mockito.when(mockClock.millis()).thenReturn(3200L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 6400));
			Mockito.when(mockClock.millis()).thenReturn(3200L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 12800));
			Mockito.when(mockClock.millis()).thenReturn(6400L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 25600));
			Mockito.when(mockClock.millis()).thenReturn(12800L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 51200));
			Mockito.when(mockClock.millis()).thenReturn(25600L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 60000));
			Mockito.when(mockClock.millis()).thenReturn(51200L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 60000));
			Mockito.when(mockClock.millis()).thenReturn(60000L);

			backoff.reset();
			Mockito.when(mockClock.millis()).thenReturn(0L);
			assertTrue(checkExactAndDecrementTimer(backoff, 100));
			Mockito.when(mockClock.millis()).thenReturn(100L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 200));
			Mockito.when(mockClock.millis()).thenReturn(300L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 400));
			Mockito.when(mockClock.millis()).thenReturn(700L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 800));
			Mockito.when(mockClock.millis()).thenReturn(1500L);
			// would have been 1600 w/o the mandatory stop
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 400));

			backoff.reset();
			Mockito.when(mockClock.millis()).thenReturn(0L);
			assertTrue(checkExactAndDecrementTimer(backoff, 100));
			Mockito.when(mockClock.millis()).thenReturn(100L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 200));
			Mockito.when(mockClock.millis()).thenReturn(300L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 400));
			Mockito.when(mockClock.millis()).thenReturn(700L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 800));

			backoff.reset();
			Mockito.when(mockClock.millis()).thenReturn(0L);
			assertTrue(checkExactAndDecrementTimer(backoff, 100));
			Mockito.when(mockClock.millis()).thenReturn(100L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 200));
			Mockito.when(mockClock.millis()).thenReturn(300L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 400));
			Mockito.when(mockClock.millis()).thenReturn(700L);
			assertTrue(withinTenPercentAndDecrementTimer(backoff, 800));
		}

	}

}