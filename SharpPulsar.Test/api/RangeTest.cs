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
namespace Org.Apache.Pulsar.Client.Api
{
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;

	public class RangeTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testOf()
		public virtual void TestOf()
		{
			Range Range = Range.Of(0, 3);
			Assert.assertEquals(0, Range.Start);
			Assert.assertEquals(3, Range.End);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIntersect()
		public virtual void TestIntersect()
		{
			Range Range1 = Range.Of(0, 9);
			Range Range2 = Range.Of(0, 2);
			Range IntersectRange = Range1.intersect(Range2);
			Assert.assertEquals(0, IntersectRange.Start);
			Assert.assertEquals(2, IntersectRange.End);

			Range2 = Range.Of(10, 20);
			IntersectRange = Range1.intersect(Range2);
			Assert.assertNull(IntersectRange);

			Range2 = Range.Of(-10, -1);
			IntersectRange = Range1.intersect(Range2);
			Assert.assertNull(IntersectRange);

			Range2 = Range.Of(-5, 5);
			IntersectRange = Range1.intersect(Range2);
			Assert.assertEquals(0, IntersectRange.Start);
			Assert.assertEquals(5, IntersectRange.End);

			Range2 = Range.Of(5, 15);
			IntersectRange = Range1.intersect(Range2);
			Assert.assertEquals(5, IntersectRange.Start);
			Assert.assertEquals(9, IntersectRange.End);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expectedExceptions = IllegalArgumentException.class) public void testInvalid()
		public virtual void TestInvalid()
		{
			Range.Of(0, -5);
		}
	}

}