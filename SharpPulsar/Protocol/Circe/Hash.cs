/// <summary>
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
	/// Abstract hash function. Each actual hash function is provided using a
	/// <seealso cref="StatefulHash stateful"/> derived interface. Hash functions with an
	/// output length that fits within an {@code int} or a {@code long} are also
	/// generally provided using a <seealso cref="StatelessHash stateless"/> derived
	/// interface. Given a stateless hash object, a method is provided for obtaining
	/// a new corresponding stateful object.
	/// </summary>
	public interface Hash
	{

		/// <summary>
		/// Returns the canonical name of this hash algorithm.
		/// </summary>
		/// <returns> the name of this hash algorithm </returns>
		string Algorithm();

		/// <summary>
		/// Returns the length in bytes of the output of this hash function.
		/// </summary>
		/// <returns> the hash length in bytes </returns>
		int Length();

		/// <summary>
		/// Returns whether this hash function supports unsafe access to arbitrary
		/// memory addresses using methods such as
		/// <seealso cref="StatefulHash.update(long, long)"/>,
		/// <seealso cref="StatelessIntHash.calculate(long, long)"/>, or
		/// <seealso cref="IncrementalIntHash.resume(int, long, long)"/>. Such functions are
		/// generally implemented in native code.
		/// </summary>
		/// <returns> true if unsafe access is supported, false if not </returns>
		bool SupportsUnsafe();
	}

}