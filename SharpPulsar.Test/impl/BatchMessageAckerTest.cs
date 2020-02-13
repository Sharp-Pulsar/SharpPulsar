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

	using BeforeMethod = org.testng.annotations.BeforeMethod;
	using Test = org.testng.annotations.Test;

	public class BatchMessageAckerTest
	{

		private const int BatchSize = 10;

		private BatchMessageAcker acker;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void setup()
		public virtual void Setup()
		{
			acker = BatchMessageAcker.NewAcker(10);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAckers()
		public virtual void TestAckers()
		{
			assertEquals(BatchSize, acker.OutstandingAcks);
			assertEquals(BatchSize, acker.BatchSize);

			assertFalse(acker.AckIndividual(4));
			for (int I = 0; I < BatchSize; I++)
			{
				if (4 == I)
				{
					assertFalse(acker.BitSet.Get(I));
				}
				else
				{
					assertTrue(acker.BitSet.Get(i));
				}
			}

			assertFalse(acker.AckCumulative(6));
			for (int I = 0; I < BatchSize; I++)
			{
				if (I <= 6)
				{
					assertFalse(acker.BitSet.Get(I));
				}
				else
				{
					assertTrue(acker.BitSet.Get(i));
				}
			}

			for (int I = BatchSize - 1; I >= 8; I--)
			{
				assertFalse(acker.AckIndividual(I));
				assertFalse(acker.BitSet.Get(I));
			}

			assertTrue(acker.AckIndividual(7));
			assertEquals(0, acker.OutstandingAcks);
		}

	}

}