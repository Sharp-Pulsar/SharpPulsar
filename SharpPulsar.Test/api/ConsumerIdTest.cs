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

using System;
using SharpPulsar.Impl;
using Xunit;

namespace SharpPulsar.Test.Api
{

	public class ConsumerIdTest
	{
		private const string TopicTestConflict = "my-topic-1";
		private const string TopicTest2 = "my-topic-2";
		private const string SubcribtionTest = "my-sub-1";

		[Fact]
		public  void GetTopicTest()
		{
			var testConsumerId = new ConsumerId(TopicTestConflict, SubcribtionTest);
			Assert.Equal(TopicTestConflict, testConsumerId.Topic);
		}

		[Fact]
		public void GetSubscribtionTest()
		{
			var testConsumerId = new ConsumerId(TopicTestConflict, SubcribtionTest);
			Assert.Equal(SubcribtionTest, testConsumerId.Subscription);
		}

		[Fact]
		public  void HashCodeTest()
		{
			var testConsumerId = new ConsumerId(TopicTestConflict, SubcribtionTest);
            Assert.Equal(HashCode.Combine(TopicTestConflict, SubcribtionTest), testConsumerId.GetHashCode());
		}

		[Fact]
		public virtual void EqualTest()
		{
			var testConsumerId1 = new ConsumerId(TopicTestConflict, SubcribtionTest);
			var testConsumerId2 = new ConsumerId(TopicTestConflict, SubcribtionTest);
			var testConsumerId3 = new ConsumerId(TopicTest2, SubcribtionTest);

            Assert.Equal(testConsumerId2, testConsumerId1);

            Assert.Equal(testConsumerId3, testConsumerId1);

            Assert.Null(testConsumerId1);
		}
		[Fact]
		public void CompareToTest()
		{
			var testConsumerId1 = new ConsumerId(TopicTestConflict, SubcribtionTest);
			var testConsumerId2 = new ConsumerId(TopicTestConflict, SubcribtionTest);
			var testConsumerId3 = new ConsumerId(TopicTest2, SubcribtionTest);

            Assert.Equal(0, testConsumerId1.CompareTo(testConsumerId2));
			Assert.Equal(-1, testConsumerId1.CompareTo(testConsumerId3));

		}
	}

}