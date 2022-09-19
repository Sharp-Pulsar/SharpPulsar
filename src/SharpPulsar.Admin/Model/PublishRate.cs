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
	/// Publish-rate to manage publish throttling.
	/// </summary>
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @ToString public class PublishRate
	public class PublishRate
	{

		public int PublishThrottlingRateInMsg = -1;
		public long PublishThrottlingRateInByte = -1;

		public PublishRate() : base()
		{
			this.PublishThrottlingRateInMsg = -1;
			this.PublishThrottlingRateInByte = -1;
		}

		public PublishRate(int DispatchThrottlingRateInMsg, long DispatchThrottlingRateInByte) : base()
		{
			this.PublishThrottlingRateInMsg = DispatchThrottlingRateInMsg;
			this.PublishThrottlingRateInByte = DispatchThrottlingRateInByte;
		}

		public static PublishRate Normalize(PublishRate PublishRate)
		{
			if (PublishRate != null && (PublishRate.PublishThrottlingRateInMsg > 0 || PublishRate.PublishThrottlingRateInByte > 0))
			{
				return PublishRate;
			}
			else
			{
				return null;
			}
		}

		public override int GetHashCode()
		{
			return Objects.hash(PublishThrottlingRateInMsg, PublishThrottlingRateInByte);
		}

		public override bool Equals(object Obj)
		{
			if (Obj is PublishRate)
			{
				PublishRate Rate = (PublishRate) Obj;
				return Objects.equals(PublishThrottlingRateInMsg, Rate.PublishThrottlingRateInMsg) && Objects.equals(PublishThrottlingRateInByte, Rate.PublishThrottlingRateInByte);
			}
			return false;
		}

	}

}