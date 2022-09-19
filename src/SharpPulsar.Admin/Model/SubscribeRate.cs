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
namespace SharpPulsar.Admin.Model
{
	using ToString = lombok.ToString;

	/// <summary>
	/// Information about subscription rate.
	/// </summary>
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @ToString public class SubscribeRate
	public class SubscribeRate
	{

		public int SubscribeThrottlingRatePerConsumer = -1;
		public int RatePeriodInSecond = 30;

		public SubscribeRate()
		{
		}

		public SubscribeRate(int subscribeThrottlingRatePerConsumer, int ratePeriodInSecond)
		{
			this.SubscribeThrottlingRatePerConsumer = subscribeThrottlingRatePerConsumer;
			this.RatePeriodInSecond = ratePeriodInSecond;
		}

		public static SubscribeRate Normalize(SubscribeRate SubscribeRate)
		{
			if (SubscribeRate != null && SubscribeRate.SubscribeThrottlingRatePerConsumer > 0 && SubscribeRate.RatePeriodInSecond > 0)
			{
				return SubscribeRate;
			}
			else
			{
				return null;
			}
		}

		public override int GetHashCode()
		{
			return Objects.hash(SubscribeThrottlingRatePerConsumer, RatePeriodInSecond);
		}

		public override bool Equals(object Obj)
		{
			if (Obj is SubscribeRate)
			{
				SubscribeRate Rate = (SubscribeRate) Obj;
				return Objects.equals(SubscribeThrottlingRatePerConsumer, Rate.SubscribeThrottlingRatePerConsumer) && Objects.equals(RatePeriodInSecond, Rate.RatePeriodInSecond);
			}
			return false;
		}

	}

}