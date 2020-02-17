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
namespace SharpPulsar.Protocol.Circe
{
	/// <summary>
	/// Interface implemented by stateless hash functions with an output length
	/// greater than 4 bytes and less than or equal to 8 bytes.
	/// </summary>
	public interface StatefulLongHash : StatefulHash
	{

		/// <summary>
		/// Returns an instance of stateless version of this hash function.
		/// </summary>
		/// <returns> the stateless version of this hash function </returns>
		StatelessLongHash AsStateless();
	}

}