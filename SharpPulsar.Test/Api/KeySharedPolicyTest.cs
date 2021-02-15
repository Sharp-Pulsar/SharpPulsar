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
/// 
using SharpPulsar.Common;
using Xunit;

namespace SharpPulsar.Test.Api
{
    public class KeySharedPolicyTest
	{
		[Fact]
		public void TestAutoSplit()
		{

			KeySharedPolicy policy = KeySharedPolicy.AutoSplitHashRange();
			Assert.Equal(2 << 15, policy.HashRangeTotal);

			policy.Validate();
		}
		[Fact]
		public void TestExclusiveHashRange()
		{

			KeySharedPolicy.KeySharedPolicySticky policy = KeySharedPolicy.StickyHashRange();
			Assert.Equal(2 << 15, policy.HashRangeTotal);

			policy.GetRanges(Range.Of(0, 1), Range.Of(1, 2));
			Assert.Equal(2, policy.Ranges.Count);
		}
		[Fact]
		public void TestExclusiveHashRangeInvalid()
		{

			KeySharedPolicy.KeySharedPolicySticky policy = KeySharedPolicy.StickyHashRange();
            Assert.Throws<System.ArgumentException>(() => policy.Validate());
			policy.GetRanges(Range.Of(0, 9), Range.Of(0, 5));
            Assert.Throws<System.ArgumentException>(() => policy.Validate());
		}
	}

}