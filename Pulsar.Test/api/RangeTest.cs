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
namespace org.apache.pulsar.client.api
{
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;

	public class RangeTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testOf()
		public virtual void testOf()
		{
			Range range = Range.of(0, 3);
			Assert.assertEquals(0, range.Start);
			Assert.assertEquals(3, range.End);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIntersect()
		public virtual void testIntersect()
		{
			Range range1 = Range.of(0, 9);
			Range range2 = Range.of(0, 2);
			Range intersectRange = range1.intersect(range2);
			Assert.assertEquals(0, intersectRange.Start);
			Assert.assertEquals(2, intersectRange.End);

			range2 = Range.of(10, 20);
			intersectRange = range1.intersect(range2);
			Assert.assertNull(intersectRange);

			range2 = Range.of(-10, -1);
			intersectRange = range1.intersect(range2);
			Assert.assertNull(intersectRange);

			range2 = Range.of(-5, 5);
			intersectRange = range1.intersect(range2);
			Assert.assertEquals(0, intersectRange.Start);
			Assert.assertEquals(5, intersectRange.End);

			range2 = Range.of(5, 15);
			intersectRange = range1.intersect(range2);
			Assert.assertEquals(5, intersectRange.Start);
			Assert.assertEquals(9, intersectRange.End);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testInvalid()
		public virtual void testInvalid()
		{
			Range.of(0, -5);
		}
	}

}