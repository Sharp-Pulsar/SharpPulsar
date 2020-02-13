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

	public class KeySharedPolicyTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAutoSplit()
		public virtual void TestAutoSplit()
		{

			KeySharedPolicy Policy = KeySharedPolicy.AutoSplitHashRange();
			Assert.assertEquals(2 << 15, Policy.HashRangeTotal);

			Policy.validate();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExclusiveHashRange()
		public virtual void TestExclusiveHashRange()
		{

			KeySharedPolicy.KeySharedPolicySticky Policy = KeySharedPolicy.StickyHashRange();
			Assert.assertEquals(2 << 15, Policy.HashRangeTotal);

			Policy.ranges(Range.Of(0, 1), Range.Of(1, 2));
			Assert.assertEquals(Policy.Ranges.Count, 2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExclusiveHashRangeInvalid()
		public virtual void TestExclusiveHashRangeInvalid()
		{

			KeySharedPolicy.KeySharedPolicySticky Policy = KeySharedPolicy.StickyHashRange();
			try
			{
				Policy.validate();
				Assert.fail("should be failed");
			}
			catch (System.ArgumentException)
			{
			}

			Policy.ranges(Range.Of(0, 9), Range.Of(0, 5));
			try
			{
				Policy.validate();
				Assert.fail("should be failed");
			}
			catch (System.ArgumentException)
			{
			}
		}
	}

}