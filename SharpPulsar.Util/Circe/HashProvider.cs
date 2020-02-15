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

using System.Collections.Generic;

namespace SharpPulsar.Util.Circe
{

	/// <summary>
	/// Interface used to obtain instances of various kinds of hash algorithms.
	/// </summary>
	public interface HashProvider
	{

		/// <summary>
		/// Returns information about the available implementations corresponding to
		/// the given hash algorithm parameters.
		/// </summary>
		/// <param name="params"> the hash algorithm parameters </param>
		/// <returns> a set of flags indicating the level of support </returns>
		ISet<HashSupport> QuerySupport(HashParameters Params);

		/// <summary>
		/// Creates a stateful hash function using the given parameters.
		/// </summary>
		/// <param name="params"> the hash algorithm parameters </param>
		/// <returns> a stateful hash function </returns>
		/// <exception cref="UnsupportedOperationException"> if this provider cannot support the
		///             given parameters </exception>
		StatefulHash CreateStateful(HashParameters Params);

		/// <summary>
		/// Requests a stateless, int-width hash function with the given parameters.
		/// Because not all stateless hash functions are incremental, this method may
		/// be able to return implementations not supported by or more optimized than
		/// <seealso cref="getIncrementalInt"/>.
		/// </summary>
		/// <param name="params"> the hash algorithm parameters </param>
		/// <returns> a stateless int-width hash function </returns>
		/// <exception cref="UnsupportedOperationException"> if this provider cannot support the
		///             given parameters </exception>
		StatelessIntHash GetStatelessInt(HashParameters Params);

		/// <summary>
		/// Requests a stateless, long-width hash function with the given parameters.
		/// Because not all stateless hash functions are incremental, this method may
		/// be able to return implementations not supported by or more optimized than
		/// <seealso cref="getIncrementalLong"/>.
		/// <para>
		/// Note that this method may return a less efficient hash function than
		/// <seealso cref="getStatelessInt"/> for hashes of 32 bits or less.
		/// 
		/// </para>
		/// </summary>
		/// <param name="params"> the hash algorithm parameters </param>
		/// <returns> a stateless long-width hash function </returns>
		/// <exception cref="UnsupportedOperationException"> if this provider cannot support the
		///             given parameters </exception>
		StatelessLongHash GetStatelessLong(HashParameters Params);

		/// <summary>
		/// Requests an incremental, stateless, int-width hash function with the
		/// given parameters. Note that although an algorithm may be available in
		/// incremental form, some potentially more optimized implementations may not
		/// support that form, and therefore cannot be provided be this method.
		/// </summary>
		/// <param name="params"> the hash algorithm parameters </param>
		/// <returns> a stateful int-width hash function </returns>
		/// <exception cref="UnsupportedOperationException"> if this provider cannot support the
		///             given parameters </exception>
		IncrementalIntHash GetIncrementalInt(HashParameters Params);

		/// <summary>
		/// Requests an incremental, stateless, long-width hash function with the
		/// given parameters. Note that although an algorithm may be available in
		/// incremental form, some potentially more optimized implementations may not
		/// support that form, and therefore cannot be provided be this method.
		/// <para>
		/// Also note that this method may return a less efficient hash function than
		/// <seealso cref="getIncrementalInt"/> for hashes of 32 bits or less.
		/// 
		/// </para>
		/// </summary>
		/// <param name="params"> the hash algorithm parameters </param>
		/// <returns> a stateful long-width hash function </returns>
		/// <exception cref="UnsupportedOperationException"> if this provider cannot support the
		///             given parameters </exception>
		IncrementalLongHash GetIncrementalLong(HashParameters Params);
	}

}