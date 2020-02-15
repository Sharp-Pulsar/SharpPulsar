﻿/// <summary>
///*****************************************************************************
/// Copyright 2014 Trevor Robinson
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
/// *****************************************************************************
/// </summary>
namespace SharpPulsar.Util.Circe
{
	/// <summary>
	/// Interface implemented by stateful hash functions with an output length of 4
	/// bytes or less. Such functions can provide a corresponding stateless
	/// implementation.
	/// </summary>
	public interface StatefulIntHash : StatefulHash
	{

		/// <summary>
		/// Returns an instance of stateless version of this hash function.
		/// </summary>
		/// <returns> the stateless version of this hash function </returns>
		StatelessIntHash AsStateless();
	}

}