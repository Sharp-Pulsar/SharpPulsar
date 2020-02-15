using System.Collections.Generic;

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
namespace SharpPulsar.Util.Circe
{

	/// <summary>
	/// Provides access to a singleton hash function cache, if an implementation is
	/// available.
	/// </summary>
	public sealed class HashCacheLoader
	{

		private static readonly HashCache HashCache;
		private static readonly IList<HashCache> Caches = new List<HashCache>();

		static HashCacheLoader()
		{
            using IEnumerator<HashCache> iterator = Caches.GetEnumerator();

			HashCache = iterator.MoveNext() ? iterator.Current : null;
		}

        
		/// <summary>
		/// Returns whether a hash function cache is available.
		/// </summary>
		/// <returns> true if a cache is available, false if <seealso cref="getCache"/> will
		///         throw an exception </returns>
		public static bool HasCache()
		{
			return HashCache != null;
		}

		/// <summary>
		/// Returns the single hash function cache.
		/// </summary>
		/// <returns> the single hash cache </returns>
		/// <exception cref="UnsupportedOperationException"> if no hash cache is available </exception>
		public static HashCache Cache
		{
			get
			{
				if (HashCache == null)
				{
					throw new System.NotSupportedException();
				}
				return HashCache;
			}
            set => Caches.Add(value);
        }

		private HashCacheLoader()
		{
		}
	}

}