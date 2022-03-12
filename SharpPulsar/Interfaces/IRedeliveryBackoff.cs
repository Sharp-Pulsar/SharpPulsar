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
namespace SharpPulsar.Interfaces
{
	/// <summary>
	/// Interface for custom message is negativeAcked policy, users can specify a <seealso cref="IRedeliveryBackoff"/> for
	/// a consumer.
	/// 
	/// Notice: the consumer crashes will trigger the redelivery of the unacked message, this case will not respect the
	/// <seealso cref="IRedeliveryBackoff"/>, which means the message might get redelivered earlier than the delay time
	/// from the backoff.
	/// </summary>
    public interface IRedeliveryBackoff
	{
		/// <param name="redeliveryCount"> indicates the number of times the message was redelivered </param>
		long Next(int redeliveryCount);
	}

}