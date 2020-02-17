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

using Xunit;

namespace SharpPulsar.Test.Api
{
    public class RangeTest
	{
		[Fact]
		public  void TestOf()
		{
			var range = SharpPulsar.Api.Range.Of(0, 3);
			Assert.Equal(0, range.Start);
			Assert.Equal(3, range.End);
		}

		[Fact]
		public void TestIntersect()
		{
            var range1 = SharpPulsar.Api.Range.Of(0, 9);
            var range2 = SharpPulsar.Api.Range.Of(0, 2);
            var intersectRange = range1.Intersect(range2);
			Assert.Equal(0, intersectRange.Start);
			Assert.Equal(2, intersectRange.End);

			range2 = SharpPulsar.Api.Range.Of(10, 20);
			intersectRange = range1.Intersect(range2);
			Assert.Null(intersectRange);

			range2 = SharpPulsar.Api.Range.Of(-10, -1);
			intersectRange = range1.Intersect(range2);
			Assert.Null(intersectRange);

			range2 = SharpPulsar.Api.Range.Of(-5, 5);
			intersectRange = range1.Intersect(range2);
			Assert.Equal(0, intersectRange.Start);
			Assert.Equal(5, intersectRange.End);

			range2 = SharpPulsar.Api.Range.Of(5, 15);
			intersectRange = range1.Intersect(range2);
			Assert.Equal(5, intersectRange.Start);
			Assert.Equal(9, intersectRange.End);
		}
		[Fact]
		public  void TestInvalid()
		{
            SharpPulsar.Api.Range.Of(0, -5);
		}
	}

}