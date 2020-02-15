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
	/// Interface implemented by hash function caches.
	/// </summary>
	public interface HashCache
	{

		/// <summary>
		/// Requests a cached hash function with the given parameters and required
		/// support flags. If no matching function is cached, the given loader is
		/// called to obtain one to cache.
		/// </summary>
		/// <param name="params"> the hash algorithm parameters </param>
		/// <param name="required"> the required hash support flags </param>
		/// <param name="loader"> a cache loader that creates the function if not cached </param>
		/// <returns> a hash with the given parameters and support flags </returns>
		/// <exception cref="ExecutionException"> if the loader throws an exception </exception>
		Hash Get(HashParameters Params, ISet<HashSupport> Required, Callable<Hash> Loader);
	}

}