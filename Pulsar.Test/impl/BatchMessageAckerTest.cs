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

	using BeforeMethod = org.testng.annotations.BeforeMethod;
	using Test = org.testng.annotations.Test;

	public class BatchMessageAckerTest
	{

		private const int BATCH_SIZE = 10;

		private BatchMessageAcker acker;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void setup()
		public virtual void setup()
		{
			acker = BatchMessageAcker.newAcker(10);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAckers()
		public virtual void testAckers()
		{
			assertEquals(BATCH_SIZE, acker.OutstandingAcks);
			assertEquals(BATCH_SIZE, acker.BatchSize);

			assertFalse(acker.ackIndividual(4));
			for (int i = 0; i < BATCH_SIZE; i++)
			{
				if (4 == i)
				{
					assertFalse(acker.BitSet.Get(i));
				}
				else
				{
					assertTrue(acker.BitSet.Get(i));
				}
			}

			assertFalse(acker.ackCumulative(6));
			for (int i = 0; i < BATCH_SIZE; i++)
			{
				if (i <= 6)
				{
					assertFalse(acker.BitSet.Get(i));
				}
				else
				{
					assertTrue(acker.BitSet.Get(i));
				}
			}

			for (int i = BATCH_SIZE - 1; i >= 8; i--)
			{
				assertFalse(acker.ackIndividual(i));
				assertFalse(acker.BitSet.Get(i));
			}

			assertTrue(acker.ackIndividual(7));
			assertEquals(0, acker.OutstandingAcks);
		}

	}

}