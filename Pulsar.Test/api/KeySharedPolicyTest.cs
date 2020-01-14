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

	public class KeySharedPolicyTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAutoSplit()
		public virtual void testAutoSplit()
		{

			KeySharedPolicy policy = KeySharedPolicy.autoSplitHashRange();
			Assert.assertEquals(2 << 15, policy.HashRangeTotal);

			policy.validate();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExclusiveHashRange()
		public virtual void testExclusiveHashRange()
		{

			KeySharedPolicy.KeySharedPolicySticky policy = KeySharedPolicy.stickyHashRange();
			Assert.assertEquals(2 << 15, policy.HashRangeTotal);

			policy.ranges(Range.of(0, 1), Range.of(1, 2));
			Assert.assertEquals(policy.Ranges.size(), 2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExclusiveHashRangeInvalid()
		public virtual void testExclusiveHashRangeInvalid()
		{

			KeySharedPolicy.KeySharedPolicySticky policy = KeySharedPolicy.stickyHashRange();
			try
			{
				policy.validate();
				Assert.fail("should be failed");
			}
			catch (System.ArgumentException)
			{
			}

			policy.ranges(Range.of(0, 9), Range.of(0, 5));
			try
			{
				policy.validate();
				Assert.fail("should be failed");
			}
			catch (System.ArgumentException)
			{
			}
		}
	}

}